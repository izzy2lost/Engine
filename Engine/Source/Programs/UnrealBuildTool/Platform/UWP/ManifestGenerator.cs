// Copyright 1998-2017 Epic Games, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
using System.Xml;
using Tools.DotNETCommon;

namespace UnrealBuildTool
{
	/// <summary>
	///  Class to handle generating an AppxManifest.xml file
	/// </summary>
	public class UWPManifestGenerator
	{
		// Global path configuration
		private const string BuildResourceSubPath = "Resources";
		private const string EngineResourceSubPath = "DefaultImages";

		// Manifest compliance values
		private const int MaxResourceEntries = 200;

		// INI configuration cache
		private ConfigHierarchy EngineIni;
		private ConfigHierarchy GameIni;

		// Manifest configuration values/paths
		private List<WinMDRegistrationInfo> WinMDReferences;
		private UnrealTargetPlatform Platform;
		private string TargetSettings;
		private string BuildResourceProjectRelativePath;
		private string ProjectPath;
		private string OutputPath;
		private string IntermediatePath;
		private List<string> CulturesToStage;

		// Manifest generation state
		private UEResXWriter NeutralResourceWriter;
		private List<UEResXWriter> PerCultureResourceWriters;
		private XmlDocument AppxManifestXmlDocument;
		private List<string> UpdatedFilePaths;

		// Analagous to RelativeProjectRootForStage in UAT so that VS (UBT only) and UAT layouts match
		private string RelativeProjectRootForStage;
		bool IsGameSpecificExe;
		private bool IsDlc;
		private Dictionary<string, string> ParsedDlcInfo;


		/// <summary>
		/// Retrieve a package configuration option from the deprecated [AppxManifest] INI settings and return the value.
		/// NOTE: Do not use this method for storing values, it is left in purely for compatibility. All package configuration
		/// should be done using the target settings in the editor or in the [/Script/UWPPlatformEditor.UWPTargetSettings] INI section.
		/// Values are stored in the following format.
		/// Sample INI setting: Package.Capabilities.mx:Capability[0].Name=kinectAudio
		/// Corresponding XML output:
		///		&lt;Package&gt;
		///		&lt;Capabilities&gt;
		///		&lt;mx:Capability Name="kinectAudio"&gt;
		/// Symbol Key:
		/// $Section:Key$		Look up Key in Section in INI files (Game first then fall back to Engine).
		/// 					Replace symbol with INI setting value.
		/// %RelativeExePath%	Replace value with path to exe from package root.
		/// %Insert:Path%		Insert contents of file at Path based off of project root path. Will indent all lines in the file
		/// 					by the current value of Indent. File should contain valid XML (this is not verified).
		/// </summary>
		/// <param name="LookupString"> INI key to locate</param>
		/// <param name="Index">Optional index of setting for [n] type LookupStrings</param>
		/// <param name="Indent">Optional current indent count (used when writing %Insert:Path% values</param>
		/// <returns>INI value for key LookupString post interpretation</returns>
		private string GetInterprettedSettingValue(string LookupString, int Index = 0, int Indent = 0)
		{
			char[] VariableMarkers = { '$', '%' };
			string BaseSetting;
			string InterprettedSetting = "";

			// Manifest settings are only (validly) located in Engine INI files
			if (!EngineIni.GetString("AppxManifest", LookupString, out BaseSetting))
			{
				return "";
			}

			// Parse results for any operators
			int NextSetting = BaseSetting.IndexOfAny(VariableMarkers);
			while (NextSetting >= 0)
			{
				// This will parse multiple operator types within a single setting, but not nested operators
				if (NextSetting > 0)
				{
					// Copy any leading text (non-operator) to our output
					InterprettedSetting += BaseSetting.Substring(0, NextSetting);
				}
				int LenOfSetting = BaseSetting.Substring(NextSetting + 1).IndexOfAny(VariableMarkers);
				if (LenOfSetting < 0)
				{
					Log.TraceError("Could not parse setting {0}. Unmatched variable symbol '{1}'", LookupString, BaseSetting[NextSetting]);
					return InterprettedSetting + BaseSetting;
				}
				if (BaseSetting[NextSetting] != BaseSetting[NextSetting + LenOfSetting + 1])
				{
					// Probable nested operators
					Log.TraceError("Could not parse setting {0}. Mismatched variable symbols '{1}' and '{2}'", LookupString, BaseSetting[NextSetting], BaseSetting[NextSetting + LenOfSetting + 1]);
					return InterprettedSetting + BaseSetting;
				}

				// Complete contents of operator
				string VariableName = BaseSetting.Substring(NextSetting + 1, LenOfSetting);

				switch (BaseSetting[NextSetting])
				{
					case '$':
						// Look up $Section:Key$ in Game INIs
						string IniSection = VariableName.Substring(0, VariableName.IndexOf(':'));
						string IniSetting = VariableName.Substring(VariableName.IndexOf(':') + 1);
						string IniValue;
						GameIni.GetString(IniSection, IniSetting, out IniValue);
						// If not found in Game INIs, search for the same Key in Engine INIs
						if (IniValue.Length == 0)
						{
							EngineIni.GetString(IniSection, IniSetting, out IniValue);
						}
						// Replace operator with value recovered
						if (IniValue.Length == 0)
						{
							// @todo: Is there any better way to handle not finding the value? If we leave a value blank it will
							// likely produce invalid XML and be difficult to trace. At least this hardcoded string should lead
							// users back here.
							InterprettedSetting += "InvalidIniValue";
						}
						else
						{
							InterprettedSetting += IniValue;
						}
						break;
					case '%':
						if (VariableName.StartsWith("Insert:"))
						{
							// Attempt to open path provided based off of the current project path
							string InsertSource = Path.Combine(ProjectPath, VariableName.Substring(VariableName.IndexOf(':') + 1));
							if (!File.Exists(InsertSource))
							{
								Log.TraceWarning("Invalid path for insertion: {0}", InsertSource);
								// @todo: Can't think of a way to insert valid XML in this case, so it's just going to be left out.
								// It would be better at least to insert something that would lead back here (as is done with
								// "InvalidIniValue" above.
								break;
							}
							string[] InsertContents = null;
							try
							{
								InsertContents = File.ReadAllLines(InsertSource);
							}
							catch (Exception)
							{
								Log.TraceWarning("Error while trying to read data for insert from {0}.", InsertSource);
								// @todo: Can't think of a way to insert valid XML in this case, so it's just going to be left out.
								// It would be better at least to insert something that would lead back here (as is done with
								// "InvalidIniValue" above.
								break;
							}
							// Insert file contents one line at a time so that we can add indentation as needed.
							foreach (string InsertLine in InsertContents)
							{
								InterprettedSetting += InsertLine + "\n";
							}
						}
						else if (VariableName.StartsWith("ResourceString:") || VariableName.StartsWith("ResourceBinary:"))
						{
							string SectionKeyPair = VariableName.Substring(VariableName.IndexOf(':') + 1);
							// Look up $Section:Key$ in Game INIs
							string SettingSection = SectionKeyPair.Substring(0, SectionKeyPair.IndexOf(':'));
							string SettingKey = SectionKeyPair.Substring(SectionKeyPair.IndexOf(':') + 1);
							String SettingValue = null;
							GameIni.GetString(SettingSection, SettingKey, out SettingValue);
							// If not found in Game INIs, search for the same Key in Engine INIs
							if (SettingValue == null || SettingValue.Length == 0)
							{
								EngineIni.GetString(SettingSection, SettingKey, out SettingValue);
							}
							// Replace operator with value recovered
							if (SettingValue != null && SettingValue.Length > 0)
							{
								InterprettedSetting += SettingValue;
							}
						}
						else if (VariableName.StartsWith("Array:"))
						{
							string SectionKeyPair = VariableName.Substring(VariableName.IndexOf(':') + 1);
							// Look up $Section:Key$ in Game INIs
							string ArraySection = SectionKeyPair.Substring(0, SectionKeyPair.IndexOf(':'));
							string ArrayKey = SectionKeyPair.Substring(SectionKeyPair.IndexOf(':') + 1);
							List<string> ArraySettingValue = null;
							GameIni.GetArray(ArraySection, ArrayKey, out ArraySettingValue);
							// If not found in Game INIs, search for the same Key in Engine INIs
							if (ArraySettingValue == null || ArraySettingValue.Count == 0)
							{
								EngineIni.GetArray(ArraySection, ArrayKey, out ArraySettingValue);
							}
							// Replace operator with value recovered
							if (ArraySettingValue == null || ArraySettingValue.Count == 0)
							{
								// @todo: Is there any better way to handle not finding the value? If we leave a value blank it will
								// likely produce invalid XML and be difficult to trace. At least this hardcoded string should lead
								// users back here.
								InterprettedSetting += "InvalidIniValue";
							}
							else
							{
								List<string> ArraySettingValueDeduplicated = ArraySettingValue.Distinct().ToList();
								if (ArraySettingValueDeduplicated.Count > Index)
								{
									InterprettedSetting += ArraySettingValueDeduplicated[Index];
								}
							}
						}
						else if (VariableName.StartsWith("AlphaNumericDot:"))
						{
							string SectionKeyPair = VariableName.Substring(VariableName.IndexOf(':') + 1);
							// Look up $Section:Key$ in Game INIs
							string SettingSection = SectionKeyPair.Substring(0, SectionKeyPair.IndexOf(':'));
							string SettingKey = SectionKeyPair.Substring(SectionKeyPair.IndexOf(':') + 1);
							string SettingValue = null;
							GameIni.GetString(SettingSection, SettingKey, out SettingValue);
							// If not found in Game INIs, search for the same Key in Engine INIs
							if (SettingValue == null || SettingValue.Length == 0)
							{
								EngineIni.GetString(SettingSection, SettingKey, out SettingValue);
							}
							// Replace operator with value recovered
							if (SettingValue == null || SettingValue.Length == 0)
							{
								// @todo: Is there any better way to handle not finding the value? If we leave a value blank it will
								// likely produce invalid XML and be difficult to trace. At least this hardcoded string should lead
								// users back here.
								InterprettedSetting += "InvalidIniValue";
							}
							else
							{
								foreach (char Character in SettingValue.ToCharArray())
								{
									if ((Character >= 'A' && Character <= 'Z') ||
									   (Character >= 'a' && Character <= 'z') ||
									   (Character >= '0' && Character <= '9') ||
									   (Character == '.'))
									{
										InterprettedSetting += Character;
									}
								}
							}
						}
						else if (VariableName.StartsWith("DefaultValue:"))
						{
							int SectionIndex = VariableName.IndexOf(':') + 1;
							int KeyIndex = VariableName.IndexOf(':', SectionIndex) + 1;
							int ValueTypeIndex = VariableName.IndexOf(':', KeyIndex) + 1;
							int DefaultValueIndex = VariableName.IndexOf(':', ValueTypeIndex) + 1;
							string DefaultValue = VariableName.Substring(DefaultValueIndex);
							string ValueType = VariableName.Substring(ValueTypeIndex, DefaultValueIndex - ValueTypeIndex - 1);
							string SettingSection = VariableName.Substring(SectionIndex, KeyIndex - SectionIndex - 1);
							string SettingKey = VariableName.Substring(KeyIndex, ValueTypeIndex - KeyIndex - 1);
							// Look up $Section:Key$ in Game INIs
							string SettingValue = null;
							if (ValueType.Equals("Int32", StringComparison.InvariantCultureIgnoreCase))
							{
								int Int32SettingValue;
								GameIni.GetInt32(SettingSection, SettingKey, out Int32SettingValue);
								SettingValue = Int32SettingValue.ToString();
							}
							else if (ValueType.Equals("GUID", StringComparison.InvariantCultureIgnoreCase))
							{
								Guid GuidSettingValue;
								GameIni.TryGetValue(SettingSection, SettingKey, out GuidSettingValue);
								SettingValue = GuidSettingValue.ToString("N");
							}
							else
							{
								GameIni.GetString(SettingSection, SettingKey, out SettingValue);
							}
							// If not found in Game INIs, search for the same Key in Engine INIs
							if (SettingValue == null || SettingValue.Length == 0)
							{
								if (ValueType.Equals("Int32", StringComparison.InvariantCultureIgnoreCase))
								{
									int Int32SettingValue;
									EngineIni.GetInt32(SettingSection, SettingKey, out Int32SettingValue);
									SettingValue = Int32SettingValue.ToString();
								}
								else if (ValueType.Equals("GUID", StringComparison.InvariantCultureIgnoreCase))
								{
									Guid GuidSettingValue;
									EngineIni.TryGetValue(SettingSection, SettingKey, out GuidSettingValue);
									SettingValue = GuidSettingValue.ToString("N");
								}
								else
								{
									EngineIni.GetString(SettingSection, SettingKey, out SettingValue);
								}
							}
							// Replace operator with value recovered
							if (SettingValue == null || SettingValue.Length == 0)
							{
								InterprettedSetting += DefaultValue;
							}
							else
							{
								InterprettedSetting += SettingValue;
							}
						}
						else
						{
							Log.TraceWarning("Unable to parse AppxManifest variable value for {0}.", VariableName);
							// @todo: Is there any better way to handle not finding the value? If we leave a value blank it will
							// likely produce invalid XML and be difficult to trace. At least this hardcoded string should lead
							// users back here.
							InterprettedSetting += "InvalidVariableValue";
						}
						break;
				}

				// Find next operator pair (if any)
				BaseSetting = BaseSetting.Substring(NextSetting + LenOfSetting + 2);
				NextSetting = BaseSetting.IndexOfAny(VariableMarkers);
			}

			// Insert any tail (non-operator) text from the original setting to our output
			InterprettedSetting += BaseSetting;

			return InterprettedSetting;
		}

		/// <summary>
		/// Checks that path is a directory and tries to create it if it doesn't exist.
		/// </summary>
		/// <returns>true if the directory is present and usable</returns>
		private bool CreateCheckDirectory(string TargetDirectory)
		{
			if (!Directory.Exists(TargetDirectory))
			{
				try
				{
					Directory.CreateDirectory(TargetDirectory);
				}
				catch (Exception)
				{
					Log.TraceError("Could not create directory {0}.", TargetDirectory);
					return false;
				}
				if (!Directory.Exists(TargetDirectory))
				{
					Log.TraceError("Path {0} does not exist or is not a directory.", TargetDirectory);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if an intermediate file has any modifications from the current target. Replaces the target file
		/// if there are changes.
		/// </summary>
		private void CompareAndReplaceModifiedTarget(string IntermediatePath, string TargetPath)
		{
			if (!File.Exists(IntermediatePath))
			{
				Log.TraceError("Tried to copy non-existant intermediate file {0}.", IntermediatePath);
				return;
			}

			CreateCheckDirectory(Path.GetDirectoryName(TargetPath));

			// Check for differences in file contents
			if (File.Exists(TargetPath))
			{
				byte[] OriginalContents = File.ReadAllBytes(TargetPath);
				byte[] NewContents = File.ReadAllBytes(IntermediatePath);
				if (!OriginalContents.SequenceEqual(NewContents))
				{
					try
					{
						FileAttributes attributes = File.GetAttributes(TargetPath);
						if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
						{
							attributes &= ~FileAttributes.ReadOnly;
							File.SetAttributes(TargetPath, attributes);
						}
						File.Delete(TargetPath);
					}
					catch (Exception)
					{
						Log.TraceError("Could not replace file {0}.", TargetPath);
						return;
					}
				}
			}

			// If the file is present it is unmodified and should not be overwritten
			if (!File.Exists(TargetPath))
			{
				try
				{
					File.Copy(IntermediatePath, TargetPath);
				}
				catch (Exception)
				{
					Log.TraceError("Unable to copy file {0}.", TargetPath);
					return;
				}
				UpdatedFilePaths.Add(TargetPath);
			}
		}

		/// <summary>
		/// Copies all cultures of a source resource to the intermediate directory.
		/// <returns>true on success, false if the operation fails (i.e. the default source file doesn't exist)</returns>
		/// </summary>
		private bool CopyAndReplaceBinaryIntermediate(string ResourceFileName, bool AllowEngineFallback = true)
		{
			string TargetPath = Path.Combine(IntermediatePath, BuildResourceSubPath);
			string SourcePath = Path.Combine(ProjectPath, BuildResourceProjectRelativePath, BuildResourceSubPath);

			// Try falling back to the engine defaults if requested
			bool bFileExists = File.Exists(Path.Combine(SourcePath, ResourceFileName));
			if (!bFileExists)
			{
				if (AllowEngineFallback)
				{
					SourcePath = Path.Combine(UnrealBuildTool.EngineDirectory.FullName, BuildResourceProjectRelativePath, EngineResourceSubPath);
					bFileExists = File.Exists(Path.Combine(SourcePath, ResourceFileName));
				}
			}

			// At least the default culture entry for any resource binary must always exist
			if (!bFileExists)
			{
				return false;
			}

			// If the target resource folder doesn't exist yet, create it
			if (!CreateCheckDirectory(TargetPath))
			{
				return false;
			}

			// Find all copies of the resource file in the source directory (could be up to one for each culture and the default).
			IEnumerable<string> SourceResourceInstances = Directory.EnumerateFiles(SourcePath, ResourceFileName, SearchOption.AllDirectories);

			// Copy new resource files
			foreach (string SourceResourceFile in SourceResourceInstances)
			{
				//@todo only copy files for cultures we are staging
				string TargetResourcePath = Path.Combine(TargetPath, SourceResourceFile.Substring(SourcePath.Length + 1));
				if (!CreateCheckDirectory(Path.GetDirectoryName(TargetResourcePath)))
				{
					Log.TraceError("Unable to create intermediate directory {0}.", Path.GetDirectoryName(TargetResourcePath));
					continue;
				}
				if (!File.Exists(TargetResourcePath))
				{
					try
					{
						File.Copy(SourceResourceFile, TargetResourcePath);
					}
					catch (Exception)
					{
						Log.TraceError("Unable to copy file {0} to {1}.", SourceResourceFile, TargetResourcePath);
						return false;
					}
				}
			}

			// Now find specially named qualified versions of the resource (e.g. logo.scale-200.png) and give them the same treatment
			string QualifiedResourceFileName = ResourceFileName.Replace(".png", ".*.png");
			IEnumerable<string> SourceResourceQualifiedInstances = Directory.EnumerateFiles(SourcePath, QualifiedResourceFileName, SearchOption.AllDirectories);

			// Copy new resource files
			foreach (string SourceResourceFile in SourceResourceQualifiedInstances)
			{
				//@todo only copy files for cultures we are staging
				string TargetResourcePath = Path.Combine(TargetPath, SourceResourceFile.Substring(SourcePath.Length + 1));
				if (!CreateCheckDirectory(Path.GetDirectoryName(TargetResourcePath)))
				{
					Log.TraceError("Unable to create intermediate directory {0}.", Path.GetDirectoryName(TargetResourcePath));
					continue;
				}
				if (!File.Exists(TargetResourcePath))
				{
					try
					{
						File.Copy(SourceResourceFile, TargetResourcePath);
					}
					catch (Exception)
					{
						Log.TraceError("Unable to copy file {0} to {1}.", SourceResourceFile, TargetResourcePath);
						return false;
					}
				}
			}


			return true;
		}

		/// <summary>
		/// Copies modified intermediate resource binaries to the output path and deletes any stale resources in the
		/// output path that do not exist in the intermediate directory.
		/// <returns>A list of updated files.</returns>
		/// </summary>
		private void CopyResourcesToTargetDir()
		{
			string TargetPath = Path.Combine(OutputPath, BuildResourceSubPath);
			string SourcePath = Path.Combine(IntermediatePath, BuildResourceSubPath);

			// If the target resource folder doesn't exist yet, create it
			if (!CreateCheckDirectory(TargetPath))
			{
				return;
			}

			// Find all copies of the resource file in both target and source directories (could be up to one for each culture and the default, but must have at least the default).
			IEnumerable<string> TargetResourceInstances = Directory.EnumerateFiles(TargetPath, "*.*", SearchOption.AllDirectories);
			IEnumerable<string> SourceResourceInstances = Directory.EnumerateFiles(SourcePath, "*.*", SearchOption.AllDirectories);

			// Remove any target files that aren't part of the source file list
			foreach (string TargetResourceFile in TargetResourceInstances)
			{
				// Ignore string tables (the only non-binary resources that will be present
				if (!TargetResourceFile.Contains(".resw"))
				{
					//@todo always delete for cultures we aren't staging
					bool bRelativeSourceFileFound = false;
					foreach (string SourceResourceFile in SourceResourceInstances)
					{
						string SourceRelativeFile = SourceResourceFile.Substring(SourcePath.Length + 1);
						string TargetRelativeFile = TargetResourceFile.Substring(TargetPath.Length + 1);
						if (SourceRelativeFile.Equals(TargetRelativeFile))
						{
							bRelativeSourceFileFound = true;
							break;
						}
					}
					if (!bRelativeSourceFileFound)
					{
						try
						{
							File.Delete(TargetResourceFile);
						}
						catch (Exception)
						{
							Log.TraceError("Could not remove stale resource file {0}.", TargetResourceFile);
						}
					}
				}
			}

			// Copy new resource files only if they differ from the destination
			foreach (string SourceResourceFile in SourceResourceInstances)
			{
				//@todo only copy files for cultures we are staging
				string TargetResourcePath = Path.Combine(TargetPath, SourceResourceFile.Substring(SourcePath.Length + 1));
				CompareAndReplaceModifiedTarget(SourceResourceFile, TargetResourcePath);
			}
		}

		/// <summary>
		/// Deletes a directory and everything it contains.
		/// </summary>
		/// <param name="InDirectoryToDelete">Directory to delete</param>
		private void RecursivelyForceDeleteDirectory(string InDirectoryToDelete)
		{
			if (Directory.Exists(InDirectoryToDelete))
			{
				try
				{
					List<string> SubDirectories = new List<string>(Directory.GetDirectories(InDirectoryToDelete, "*.*", SearchOption.AllDirectories));
					foreach (string DirectoryToRemove in SubDirectories)
					{
						RecursivelyForceDeleteDirectory(DirectoryToRemove);
					}
					List<string> FilesInDirectory = new List<string>(Directory.GetFiles(InDirectoryToDelete));
					foreach (string FileToRemove in FilesInDirectory)
					{
						try
						{
							FileAttributes Attributes = File.GetAttributes(FileToRemove);
							if ((Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
							{
								Attributes &= ~FileAttributes.ReadOnly;
								File.SetAttributes(FileToRemove, Attributes);
							}
							File.Delete(FileToRemove);
						}
						catch (Exception)
						{
							Log.TraceWarning("Could not remove file {0} to remove directory {1}.", FileToRemove, InDirectoryToDelete);
						}
					}
					Directory.Delete(InDirectoryToDelete, true);
				}
				catch (Exception)
				{
					Log.TraceWarning("Could not remove directory {0}.", InDirectoryToDelete);
				}
			}

		}

		/// <summary>
		/// Kicks off manifest generation. Will always attempt to fully calculate a new manifest but will not update the output
		/// file unless there are changes (to avoid unnecessary copies when deploying).
		/// </summary>
		/// <param name="TargetPlatform">The platform we're generating a manifest for.</param>
		/// <param name="InOutputPath">Path to write manifest files to.</param>
		/// <param name="InIntermediatePath">Path to store temporary intermediate data (e.g. XML resource file).</param>
		/// <param name="InProjectFile">Path to the uproject file</param>
		/// <param name="InProjectDirectory">Directory containing the uproject file or the base engine path if no project file is specified (for content only builds).</param>
		/// <param name="InTargetConfigs">Configurations to build manifest data for. Each configuration will generate it's own application entry.</param>
		/// <param name="InExecutables">The launch executable for each configuration. Must match the length and order of InTargetConfigs.</param>
		/// <param name="InWinMDReferences">The WinMD references that should be added as activatable types</param>
		/// <returns>A list of all updated target files</returns>
		public List<string> CreateManifest(UnrealTargetPlatform TargetPlatform, string InOutputPath, string InIntermediatePath, FileReference InProjectFile, string InProjectDirectory, List<UnrealTargetConfiguration> InTargetConfigs, List<string> InExecutables, IEnumerable<WinMDRegistrationInfo> InWinMDReferences)
		{
			// Check parameter values are valid
			if (InTargetConfigs.Count != InExecutables.Count)
			{
				Log.TraceError("The number of target configurations ({0}) and executables ({1}) passed to manifest generation do not match.", InTargetConfigs.Count, InExecutables.Count);
				return null;
			}
			if (File.Exists(InOutputPath))
			{
				Log.TraceWarning("InOutputPath {0} is a file. Should be a directory. Continuing using parent directory.", InOutputPath);
				InOutputPath = Path.GetDirectoryName(InOutputPath);
			}
			if (File.Exists(InIntermediatePath))
			{
				Log.TraceWarning("InIntermediatePath {0} is a file. Should be a directory. Continuing using parent directory.", InIntermediatePath);
				InIntermediatePath = Path.GetDirectoryName(InIntermediatePath);
			}
			if (!CreateCheckDirectory(InOutputPath))
			{
				return null;
			}
			if (!CreateCheckDirectory(InIntermediatePath))
			{
				return null;
			}

			OutputPath = InOutputPath;
			IntermediatePath = InIntermediatePath;

			UpdatedFilePaths = new List<string>();

			WinMDReferences = ((InWinMDReferences == null) ? new List<WinMDRegistrationInfo>() : new List<WinMDRegistrationInfo>(InWinMDReferences));
			Platform = TargetPlatform;
			TargetSettings = "/Script/UWPPlatformEditor.UWPTargetSettings";
			BuildResourceProjectRelativePath = "Build\\UWP";

			// Clean out the resources intermediate path so that we know there are no stale binary files.
			string IntermediateResourceDirectory = Path.Combine(IntermediatePath, BuildResourceSubPath);
			RecursivelyForceDeleteDirectory(IntermediateResourceDirectory);
			if (!Directory.Exists(IntermediateResourceDirectory))
			{
				try
				{
					Directory.CreateDirectory(IntermediateResourceDirectory);
				}
				catch (Exception)
				{
					Log.TraceError("Could not create directory {0}.", IntermediateResourceDirectory);
					return null;
				}
			}

			// Load up INI settings. We'll use engine settings to retrieve the manifest configuration, but these may reference
			// values in either game or engine settings, so we'll keep both.
			// Use the project directory here since this accounts for 'RemoteIniDir' when InProjectFile is null
			if (InProjectFile != null)
			{
				IsDlc = InProjectFile.GetExtension() == ".uplugin";

				if (IsDlc)
				{
					DirectoryReference IniDirRef = DirectoryReference.FromFile(InProjectFile).ParentDirectory.ParentDirectory;
					GameIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Game, IniDirRef, TargetPlatform);
					EngineIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Engine, IniDirRef, TargetPlatform);
					IsGameSpecificExe = new DirectoryReference(InOutputPath).IsUnderDirectory(IniDirRef);

					List<string> DlcStoreMapping = new List<string>();
					if (EngineIni.GetArray("/Script/UWPPlatformEditor.UWPTargetSettings", "DLCStoreMapping", out DlcStoreMapping))
					{
						foreach (string DlcEntry in DlcStoreMapping)
						{
							Dictionary<string, string> PossibleParsedDlcInfo = new Dictionary<string, string>();
							InterpretINIStruct(DlcEntry, out PossibleParsedDlcInfo);
							string DlcName = null;
							PossibleParsedDlcInfo.TryGetValue("PluginName", out DlcName);
							if (DlcName == InProjectFile.GetFileNameWithoutExtension())
							{
								ParsedDlcInfo = PossibleParsedDlcInfo;
								break;
							}
						}
					}

					if (ParsedDlcInfo == null)
					{
						Log.TraceWarning("Could not map {0} to a Store identity.  Using a temporary identity to enable local deployment.  For Store upload configure identity in the UWP Project Settings.", InProjectFile);
						ParsedDlcInfo = new Dictionary<string, string>();
						ParsedDlcInfo["PluginName"] = InProjectFile.GetFileNameWithoutExtension();
						ParsedDlcInfo["PackageIdentityName"] = ParsedDlcInfo["PluginName"];
						ParsedDlcInfo["PackageIdentityVersion"] = "1.0.0.0";
					}
				}
				else
				{
					DirectoryReference IniDirRef = DirectoryReference.FromFile(InProjectFile);
					GameIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Game, IniDirRef, TargetPlatform);
					EngineIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Engine, IniDirRef, TargetPlatform);
					IsGameSpecificExe = new DirectoryReference(InOutputPath).IsUnderDirectory(IniDirRef);
				}
			}
			else if (!string.IsNullOrEmpty(UnrealBuildTool.GetRemoteIniPath()))
			{
				DirectoryReference IniDirRef = new DirectoryReference(UnrealBuildTool.GetRemoteIniPath());
				GameIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Game, IniDirRef, TargetPlatform);
				EngineIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Engine, IniDirRef, TargetPlatform);
				IsGameSpecificExe = false;
			}
			else
			{
				GameIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Game, null, TargetPlatform);
				EngineIni = ConfigCache.ReadHierarchy(ConfigHierarchyType.Engine, null, TargetPlatform);
				IsGameSpecificExe = false;
			}

			ProjectPath = InProjectDirectory;
			RelativeProjectRootForStage = IsGameSpecificExe ? InProjectFile.GetFileNameWithoutAnyExtensions() : "Engine";

			// Load and verify/clean culture list
			List<string> CulturesToStageWithDuplicates = null;
			GameIni.GetArray("/Script/UnrealEd.ProjectPackagingSettings", "CulturesToStage", out CulturesToStageWithDuplicates);
			if (CulturesToStageWithDuplicates == null || CulturesToStageWithDuplicates.Count < 1)
			{
				Log.TraceError("At least one culture must be selected to stage.");
				return null;
			}

			CulturesToStage = CulturesToStageWithDuplicates.Distinct().ToList();

			// Construct a single resource writer for the default (no-culture) values
			string NeutralResourceIntermediatePath = Path.Combine(IntermediateResourceDirectory, "resources.resw");
			NeutralResourceWriter = new UEResXWriter(NeutralResourceIntermediatePath);

			PerCultureResourceWriters = new List<UEResXWriter>();
			for (int i = 0; i < CulturesToStage.Count; ++i)
			{
				string Culture = CulturesToStage[i];
				string IntermediateStringResourcePath = Path.Combine(IntermediateResourceDirectory, Culture);
				string IntermediateStringResourceFile = Path.Combine(IntermediateStringResourcePath, "resources.resw");
				if (!CreateCheckDirectory(IntermediateStringResourcePath))
				{
					Log.TraceWarning("Failed to create {0}.  Culture {1} resources not staged.", IntermediateStringResourcePath, Culture);
					CulturesToStage.RemoveAt(i);
					--i;
					continue;
				}
				PerCultureResourceWriters.Add(new UEResXWriter(IntermediateStringResourceFile));
			}

			if (CulturesToStage.Count == 0)
			{
				Log.TraceError("Failed to create intermediate files for any culture.  Manifest could not be generated.");
				return null;
			}

			// Create the appxmanifest document
			AppxManifestXmlDocument = new XmlDocument();
			XmlDeclaration Declaration = AppxManifestXmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.BodyName, null);
			AppxManifestXmlDocument.AppendChild(Declaration);
			
			// Begin document content construction. Resources entries will be setup as required to support the manifest generation.
			XmlNode Package = GetPackage(InTargetConfigs, InExecutables);
			AppxManifestXmlDocument.AppendChild(Package);

			// Export appxmanifest.xml to the intermediate directory then compare the contents to any existing target manifest
			// and replace if there are differences.
			string ManifestIntermediatePath = Path.Combine(IntermediatePath, "AppxManifest.xml");
			string ManifestTargetPath = Path.Combine(OutputPath, "AppxManifest.xml");
			AppxManifestXmlDocument.Save(ManifestIntermediatePath);

			// Check we produced a reasonable manifest document
			ValidateAppxManifest(ManifestIntermediatePath);

			CompareAndReplaceModifiedTarget(ManifestIntermediatePath, ManifestTargetPath);

			// Clean out any resource directories that we aren't staging
			string TargetResourcePath = Path.Combine(OutputPath, BuildResourceSubPath);
			if (Directory.Exists(TargetResourcePath))
			{
				List<string> TargetResourceDirectories = new List<string>(Directory.GetDirectories(TargetResourcePath, "*.*", SearchOption.AllDirectories));
				foreach (string ResourceDirectory in TargetResourceDirectories)
				{
					if (!CulturesToStage.Contains(Path.GetFileName(ResourceDirectory)))
					{
						RecursivelyForceDeleteDirectory(ResourceDirectory);
					}
				}
			}

			// MS staging code requires an AppxManifest to convert
			//@todo remove if possible
			// DLC packages do not contain an exe
			if (InExecutables.Count > 0)
			{
				string ManifestBinaryPath = Path.Combine(Path.GetDirectoryName(InExecutables[0]), "AppxManifest.xml");
				CompareAndReplaceModifiedTarget(ManifestIntermediatePath, ManifestBinaryPath);
			}

			// Export the resource tables starting with the neutral culture
			string NeutralResourceTargetPath = Path.Combine(OutputPath, BuildResourceSubPath, "resources.resw");
			NeutralResourceWriter.Close();
			CompareAndReplaceModifiedTarget(NeutralResourceIntermediatePath, NeutralResourceTargetPath);

			for (int CultureIndex = 0; CultureIndex < CulturesToStage.Count; CultureIndex++)
			{
				string Culture = CulturesToStage[CultureIndex];
				string IntermediateStringResourceFile = Path.Combine(IntermediateResourceDirectory, Culture, "resources.resw");
				string TargetStringResourceFile = Path.Combine(OutputPath, BuildResourceSubPath, Culture, "resources.resw");
				PerCultureResourceWriters[CultureIndex].Close();
				CompareAndReplaceModifiedTarget(IntermediateStringResourceFile, TargetStringResourceFile);
			}

			// Copy all the binary resources into the target directory.
			CopyResourcesToTargetDir();

			// The resource database is dependent on everything else calculated here (manifest, resource string tables, binary resources).
			// So if any file has been updated we'll need to run the config.
			IEnumerable<string> TargetPriFiles = Directory.EnumerateFiles(OutputPath, "*.pri");
			if (UpdatedFilePaths.Count > 0 || TargetPriFiles.Count() == 0)
			{
				// Create resource index configuration
				string PriExecutable = UniversalWindowsPlatformToolChain.GetWindowsSdkToolPath("makepri.exe").FullName;

				// We're not currently splitting pri files along the culture dimension, so all supported languages should be defaults
				string AllDefaultCultures = CulturesToStage.Aggregate((c1, c2) => (c1 + "_" + c2));

				string ResourceConfigFile = Path.Combine(IntermediatePath, "priconfig.xml");
				string MakePriArgs = "createconfig /cf \"" + ResourceConfigFile + "\" /dq " + AllDefaultCultures + " /o";
				System.Diagnostics.ProcessStartInfo StartInfo = new System.Diagnostics.ProcessStartInfo(PriExecutable, MakePriArgs);
				StartInfo.UseShellExecute = false;
				StartInfo.RedirectStandardOutput = true;
				StartInfo.CreateNoWindow = true;
				int ExitCode = Utils.RunLocalProcessAndLogOutput(StartInfo);
				if (ExitCode != 0)
				{
					throw new BuildException("Failed to generate config file for Package Resource Index.  See log for details.");
				}

				// Modify configuration to restrict indexing to the Resources directory (saves time and space)
				XmlDocument PriConfig = new XmlDocument();
				PriConfig.Load(ResourceConfigFile);

				// Remove the 'Packaging' node so that we have no autoResourcePackage entries
				XmlNodeList PackagingNodes = PriConfig.SelectNodes("/resources/packaging");
				foreach (XmlNode Node in PackagingNodes)
				{
					Node.ParentNode.RemoveChild(Node);
				}

				// The Xbox One approach to limiting the indexer causes files to have dodgy uris in the 
				// generated pri e.g. ms-resource://PackageIdentityName/Files/Logo.png instead of ms-resource://PackageIdentityName/Files/Resources/Logo.png
				// This appears to affect Windows's ability to locate a valid image in some scenarios such as a
				// desktop shortcut.  So on UWP we start from the root and add exclusions.
				XmlNodeList ConfigNodes = PriConfig.SelectNodes("/resources/index/indexer-config");
				foreach (XmlNode ConfigNode in ConfigNodes)
				{
					if (ConfigNode.Attributes["type"].Value == "folder")
					{
						IEnumerable<string> AllSubItems = Directory.EnumerateFileSystemEntries(OutputPath);
						foreach (string FileSystemEntry in AllSubItems)
						{
							if (Path.GetFileName(FileSystemEntry) != "Resources")
							{
								XmlElement ExcludeElement = PriConfig.CreateElement("exclude");
								if (File.Exists(FileSystemEntry))
								{
									ExcludeElement.SetAttribute("type", "path");
								}
								else
								{
									ExcludeElement.SetAttribute("type", "tree");
								}
								ExcludeElement.SetAttribute("value", Path.GetFileName(FileSystemEntry));
								ExcludeElement.SetAttribute("doNotTraverse", "true");
								ExcludeElement.SetAttribute("doNotIndex", "true");
								ConfigNode.AppendChild(ExcludeElement);
							}
						}
					}
				}
				PriConfig.Save(ResourceConfigFile);

				// Remove previous pri files so we can enumerate which ones are new since the resource generator could produce a file for each staged language.
				IEnumerable<string> OldPriFiles = Directory.EnumerateFiles(IntermediatePath, "*.pri");
				foreach (string OldPri in OldPriFiles)
				{
					try
					{
						File.Delete(OldPri);
					}
					catch (Exception)
					{
						Log.TraceError("Could not delete file {0}.", OldPri);
					}
				}

				// Generate the resource index
				string ResourceLogFile = Path.Combine(IntermediatePath, "ResIndexLog.xml");
				string ResourceIndexFile = Path.Combine(IntermediatePath, "resources.pri");
				MakePriArgs = "new /pr \"" + OutputPath + "\" /cf \"" + ResourceConfigFile + "\" /mn \"" + ManifestTargetPath + "\" /il \"" + ResourceLogFile + "\" /of \"" + ResourceIndexFile + "\" /o";
				StartInfo = new System.Diagnostics.ProcessStartInfo(PriExecutable, MakePriArgs);
				StartInfo.UseShellExecute = false;
				StartInfo.RedirectStandardOutput = true;
				StartInfo.CreateNoWindow = true;
				StartInfo.StandardErrorEncoding = System.Text.Encoding.Unicode;
				StartInfo.StandardOutputEncoding = System.Text.Encoding.Unicode;
				ExitCode = Utils.RunLocalProcessAndLogOutput(StartInfo);
				if (ExitCode != 0)
				{
					throw new BuildException("Failed to generate Package Resource Index file.  See log for details.");
				}

				// Remove any existing pri target files that were not generated by this latest update
				IEnumerable<string> NewPriFiles = Directory.EnumerateFiles(IntermediatePath, "*.pri");
				foreach (string TargetPri in TargetPriFiles)
				{
					if (!NewPriFiles.Contains(TargetPri))
					{
						try
						{
							File.Delete(TargetPri);
						}
						catch (Exception)
						{
							Log.TraceError("Could not remove stale file {0}.", TargetPri);
						}
					}
				}

				// Stage all the modified pri files to the output directory
				foreach (string NewPri in NewPriFiles)
				{
					string FinalResourceIndexFile = Path.Combine(OutputPath, Path.GetFileName(NewPri));
					CompareAndReplaceModifiedTarget(ResourceIndexFile, FinalResourceIndexFile);
				}
			}

			return UpdatedFilePaths;
		}

		/// <summary>
		/// Interpret a struct stored within a single INI entry into key-value pairs.
		/// Note: Will parse all nested structs.
		/// </summary>
		private void InterpretINIStruct(string INIStruct, out Dictionary<string, string> StructDictionary)
		{
			StructDictionary = new Dictionary<string, string>();
			char[] EntryEndChars = { '(', ')', ',' };
			string[] StructEntries = INIStruct.Split(EntryEndChars, StringSplitOptions.RemoveEmptyEntries);
			foreach (string CurrentEntry in StructEntries)
			{
				int AssignmentIndex = CurrentEntry.IndexOf('=');
				if (AssignmentIndex > 0)
				{
					string EntryKey = CurrentEntry.Substring(0, AssignmentIndex);
					string EntryValue = CurrentEntry.Substring(AssignmentIndex + 1);
					char[] EntryTrimChars = { '"' };
					EntryValue = EntryValue.Trim(EntryTrimChars);
					StructDictionary.Add(EntryKey, EntryValue);
				}
			}
		}
		
		/// <summary>
		/// Calculate a manifest string value based on a system of fallback possibilities and return it.
		/// Selection priorities:
		/// 1. PlatformINIKey under section [/Script/UWPPlatformEditor.UWPTargetSettings]
		/// 2. Deprecated [AppxManifest] setting where the key equals ManifestFullPath
		/// 3. Engine INI value with section equal to GenericINISection and key equal to GenericINIKey
		/// 4. Game INI value with section equal to GenericINISection and key equal to GenericINIKey
		/// 5. The DefaultValue passed in
		/// </summary>
		private string CreateStringValue(string PlatformINIKey, string ManifestFullPath, string GenericINISection, string GenericINIKey, string DefaultValue, Func<string, string> ValueValidationDelegate = null)
		{
			string ConfigScratchValue = "";
			if (!EngineIni.GetString(TargetSettings, PlatformINIKey, out ConfigScratchValue) || ConfigScratchValue.Length <= 0)
			{
				if (ManifestFullPath != null)
				{
					ConfigScratchValue = GetInterprettedSettingValue(ManifestFullPath);
				}
				if (ConfigScratchValue == null || ConfigScratchValue.Length <= 0)
				{
					// If a config value wasn't specified, don't try to read from the configs and just go with the default value
					if (GenericINISection != null && GenericINIKey != null)
					{
						bool EngineConfigReadSuccess = EngineIni.GetString(GenericINISection, GenericINIKey, out ConfigScratchValue);
						// If the engine config read failed or the returned value was empty/null, keep searching, otherwise use the value we already retrieved in ConfigScratchValue
						if (!EngineConfigReadSuccess || ConfigScratchValue == null || ConfigScratchValue.Length <= 0)
						{
							bool GameConfigReadSuccess = GameIni.GetString(GenericINISection, GenericINIKey, out ConfigScratchValue);
							// If the game config read failed or the returned value was empty/null, use the default value, otherwise use the value we already retrieved in ConfigScratchValue
							if (!GameConfigReadSuccess || ConfigScratchValue == null || ConfigScratchValue.Length <= 0)
							{
								ConfigScratchValue = DefaultValue;
							}
						}
					}
					else
					{
						ConfigScratchValue = DefaultValue;
					}
				}
			}
			if (ValueValidationDelegate != null)
			{
				return ValueValidationDelegate(ConfigScratchValue);
			}
			else
			{
				return ConfigScratchValue;
			}
		}

		/// <summary>
		/// Calculate a manifest string value using CreateStringValue and compare that value to TrueValue, return the result.
		/// </summary>
		private bool CreateBoolValue(string PlatformINIKey, string ManifestFullPath, string GenericINISection, string GenericINIKey, string DefaultValue, string TrueValue = "True")
		{
			string ConfigScratchValue = CreateStringValue(PlatformINIKey, ManifestFullPath, GenericINISection, GenericINIKey, DefaultValue);
			return ConfigScratchValue.Equals(TrueValue, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Calculate a manifest string value using CreateStringValue and return the result as an integer.
		/// </summary>
		private int CreateIntValue(string PlatformINIKey, string ManifestFullPath, string GenericINISection, string GenericINIKey, string DefaultValue)
		{
			string ConfigScratchValue = CreateStringValue(PlatformINIKey, ManifestFullPath, GenericINISection, GenericINIKey, DefaultValue);
			return Int32.Parse(ConfigScratchValue);
		}

		/// <summary>
		/// Calculate a manifest array value based on a system of fallback possibilities and return it.
		/// Selection priorities:
		/// 1. PlatformINIKey under section [/Script/UWPPlatformEditor.UWPTargetSettings]
		/// 2. Deprecated [AppxManifest] setting where the key equals ManifestFullPath
		/// 3. Engine INI value with section equal to GenericINISection and key equal to GenericINIKey
		/// 4. Game INI value with section equal to GenericINISection and key equal to GenericINIKey
		/// 5. The DefaultValue passed in
		/// </summary>
		private List<string> CreateArrayValue(string PlatformINIKey, string ManifestPath, string ManifestSubKey, string GenericINISection, string GenericINIKey, List<string> DefaultValue)
		{
			List<string> ConfigScratchValue = null;
			if (!EngineIni.GetArray(TargetSettings, PlatformINIKey, out ConfigScratchValue))
			{
				if (ManifestPath != null)
				{
					// Retrieve the deprecated [AppxManifest] value, but it's an array so we will have to pull from multiple entries
					int ArrayIndex = 0;
					while (true)
					{
						string FullManifestPath = ManifestPath + "[" + ArrayIndex + "]";
						if (ManifestSubKey != null && ManifestSubKey.Length <= 0)
						{
							FullManifestPath += "." + ManifestSubKey;
						}
						string ArrayElement = GetInterprettedSettingValue(FullManifestPath);
						if (ArrayElement == null || ArrayElement.Length <= 0)
						{
							break;
						}
						if (ConfigScratchValue == null)
						{
							ConfigScratchValue = new List<string>(1);
						}
						ConfigScratchValue.Add(ArrayElement);
						ArrayIndex++;
					}
				}
				if (ConfigScratchValue == null || ConfigScratchValue.Count <= 0)
				{
					if (GenericINISection == null || GenericINIKey == null || (!EngineIni.GetArray(GenericINISection, GenericINIKey, out ConfigScratchValue) && !GameIni.GetArray(GenericINISection, GenericINIKey, out ConfigScratchValue)))
					{
						ConfigScratchValue = DefaultValue;
					}
				}
			}
			return ConfigScratchValue;
		}

		/// <summary>
		/// Calculate a manifest string value using CreateStringValue and return it as an XmlElement.
		/// </summary>
		private XmlElement CreateStringElement(string ElementName, string PlatformINIKey, string ManifestFullPath, string GenericINISection, string GenericINIKey, string DefaultValue)
		{
			XmlElement TargetElement = AppxManifestXmlDocument.CreateElement(ElementName);
			string ConfigScratchValue = CreateStringValue(PlatformINIKey, ManifestFullPath, GenericINISection, GenericINIKey, DefaultValue);
			TargetElement.InnerText = ConfigScratchValue;
			return TargetElement;
		}

		/// <summary>
		/// Calculate a manifest string value using CreateStringValue and return it as an XmlAttribute.
		/// </summary>
		private XmlAttribute CreateStringAttribute(string ElementName, string PlatformINIKey, string ManifestFullPath, string GenericINISection, string GenericINIKey, string DefaultValue, Func<string, string> ValueValidationDelegate = null)
		{
			XmlAttribute TargetAttribute = AppxManifestXmlDocument.CreateAttribute(ElementName);
			string ConfigScratchValue = CreateStringValue(PlatformINIKey, ManifestFullPath, GenericINISection, GenericINIKey, DefaultValue, ValueValidationDelegate);
			TargetAttribute.Value = ConfigScratchValue;
			return TargetAttribute;
		}

		/// <summary>
		/// Calculate a manifest hex color value based on a system of fallback possibilities and return an attribute containing it.
		/// Selection priorities:
		/// 1. PlatformINIKey under section [/Script/UWPPlatformEditor.UWPTargetSettings]
		/// 2. Deprecated [AppxManifest] setting where the key equals ManifestFullPath
		/// 3. The DefaultValue passed in
		/// </summary>
		private XmlAttribute CreateColorAttribute(string ElementName, string PlatformINIKey, string ManifestFullPath, string DefaultValue)
		{
			string ColorValue = "";
			if (EngineIni.GetString(TargetSettings, PlatformINIKey, out ColorValue))
			{
				// Break the setting down by color
				Dictionary<string, string> StructValues;
				InterpretINIStruct(ColorValue, out StructValues);
				int Red = 0;
				string RedText = StructValues["R"];
				Int32.TryParse(RedText, out Red);
				int Green = 0;
				string GreenText = StructValues["G"];
				Int32.TryParse(GreenText, out Green);
				int Blue = 0;
				string BlueText = StructValues["B"];
				Int32.TryParse(BlueText, out Blue);
				ColorValue = "#" + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
			}
			else
			{
				ColorValue = GetInterprettedSettingValue(ManifestFullPath);
				if (ColorValue == null || ColorValue.Length <= 0)
				{
					ColorValue = DefaultValue;
				}
			}

			XmlAttribute ColorAttribute = AppxManifestXmlDocument.CreateAttribute("BackgroundColor");
			ColorAttribute.Value = ColorValue;
			return ColorAttribute;
		}

		/// <summary>
		/// Using the old style manifest settings, loop through all elements in an array looking for a subkey that matches the specified value.
		/// </summary>
		/// <returns>The index of the first matching array element. -1 if no match is found.</returns>
		private int FindIndexOfArrayValue(string ManifestArrayKey, string ManifestTestElement, string ManifestTestValue)
		{
			int ArrayIndex = 0;
			while (true)
			{
				string ManifestValue = GetInterprettedSettingValue(ManifestArrayKey + "[" + ArrayIndex + "]." + ManifestTestElement);
				if (ManifestValue == null || ManifestValue.Length == 0)
				{
					return -1;
				}
				if (ManifestValue.Equals(ManifestTestValue, StringComparison.InvariantCultureIgnoreCase))
				{
					return ArrayIndex;
				}
				ArrayIndex++;
			}
		}

		/// <summary>
		/// Determine whether a resource is per-DLC or should be inherited from the main package
		/// </summary>
		private bool UseDlcResourcesForResourceEntry(string ResourceEntryName)
		{
			return IsDlc &&
				(ResourceEntryName == "PackageDisplayName" ||
				ResourceEntryName == "PackageDescription");
		}

		/// <summary>
		/// Calculate the per culture manifest string value and add a resource table entry encompassing the values.
		/// </summary>
		private void AddResourceEntry(string ResourceEntryName, string ConfigKey, string ManifestFullPath, string GenericINISection, string GenericINIKey, string DefaultValue, string ValuePostfix = "")
		{
			// Enter the default (no-culture) value
			string ConfigScratchValue = "";
			string DefaultCultureScratchValue = "";
			if (EngineIni.GetString(TargetSettings, "CultureStringResources", out DefaultCultureScratchValue))
			{
				Dictionary<string, string> DefaultCultureStringValues;
				InterpretINIStruct(DefaultCultureScratchValue, out DefaultCultureStringValues);
				ConfigScratchValue = DefaultCultureStringValues[ConfigKey];
			}
			if (ConfigScratchValue == null || ConfigScratchValue.Length <= 0)
			{
				if (ManifestFullPath != null)
				{
					ConfigScratchValue = GetInterprettedSettingValue(ManifestFullPath);
				}
				if (ConfigScratchValue == null || ConfigScratchValue.Length <= 0)
				{
					if (GenericINISection == null || GenericINIKey == null || (!EngineIni.GetString(GenericINISection, GenericINIKey, out ConfigScratchValue) && !GameIni.GetString(GenericINISection, GenericINIKey, out ConfigScratchValue)))
					{
						ConfigScratchValue = DefaultValue;
					}
				}
			}

			bool IsDlcDefinedResource = UseDlcResourcesForResourceEntry(ResourceEntryName);

			Dictionary<string, string> IniLocalizedValues = new Dictionary<string, string>();
			List<string> PerCultureValues;
			if (EngineIni.GetArray(TargetSettings, IsDlcDefinedResource ? "DlcPerCultureResources" : "PerCultureResources", out PerCultureValues))
			{
				foreach (string CultureCombinedValues in PerCultureValues)
				{
					Dictionary<string, string> SeparatedCultureValues;
					InterpretINIStruct(CultureCombinedValues, out SeparatedCultureValues);
					if (!IsDlcDefinedResource ||
						string.Compare(SeparatedCultureValues["AppliesToDlcPlugin"], ParsedDlcInfo["PluginName"], StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						string CultureId = SeparatedCultureValues["CultureId"];
						bool IsStagedCulture = string.IsNullOrEmpty(CultureId) || CulturesToStage.Contains(CultureId);
						if (IsStagedCulture)
						{
							if (SeparatedCultureValues[ConfigKey] != null && SeparatedCultureValues[ConfigKey].Length > 0)
							{
								IniLocalizedValues.Add(CultureId, SeparatedCultureValues[ConfigKey] + ValuePostfix);
							}
						}
					}
				}
			}

			string NeutralValue = string.Empty;
			if (!IniLocalizedValues.TryGetValue("", out NeutralValue))
			{
				NeutralValue = ConfigScratchValue + ValuePostfix;
			}

			bool IsEverLocalized = false;
			for (int i = 0; i < CulturesToStage.Count; ++i)
			{
				string ValueToWrite = string.Empty;
				if (IniLocalizedValues.TryGetValue(CulturesToStage[i], out ValueToWrite))
				{
					IsEverLocalized = true;
					PerCultureResourceWriters[i].AddResource(ResourceEntryName, ValueToWrite);
				}
				else
				{ 
					Log.TraceVerbose("No localized value for {0} in culture {1}.  Neutral value ({2}) will be used", ResourceEntryName, CulturesToStage[i], NeutralValue);
				}
			}

			// Any culture with a culture-specific value will override the neutral value,
			// even for unrelated cultures.  So propagate the neutral value to avoid this happening.
			if (IsEverLocalized)
			{
				for (int i = 0; i < CulturesToStage.Count; ++i)
				{
					string ValueToWrite = string.Empty;
					if (!IniLocalizedValues.ContainsKey(CulturesToStage[i]))
					{
						PerCultureResourceWriters[i].AddResource(ResourceEntryName, NeutralValue);
					}
				}
			}
			else
			{
				// No culture has a specific value for this string.  Write the shared value to the neutral resource collection.
				NeutralResourceWriter.AddResource(ResourceEntryName, NeutralValue);
			}
		}

		/// <summary>
		/// Add a child XML node to a parent node if the child node is valid.
		/// <param name="Parent">Parent node to add child to.</param>
		/// <param name="Child">Child node to be evaluated and added.</param>
		/// <param name="bNodeRequired">Display an error if the node is invalid and won't be added to the parent.</param>
		/// <param name="bNodeMustNotBeEmpty">The child node must contain child nodes of it's own to be considered valid.</param>
		/// </summary>
		private void AddElementIfValid(XmlNode Parent, XmlNode Child, bool bNodeRequired, bool bNodeMustNotBeEmpty = false)
		{
			if (Child != null)
			{
				if (!bNodeMustNotBeEmpty || Child.HasChildNodes)
				{
					Parent.AppendChild(Child);
				}
				else if (bNodeRequired)
				{
					Log.TraceError("Node {0} that requires a value is empty.", Child.Name);
				}
			}
			else if (bNodeRequired)
			{
				Log.TraceError("Unable to create required manifest entry {0}.", Child.Name);
			}
		}

		/// <summary>
		/// Gather all information for the Package element of the manifest.
		/// </summary>
		private XmlNode GetPackage(List<UnrealTargetConfiguration> TargetConfigs, List<string> Executables)
		{
			XmlElement Package = AppxManifestXmlDocument.CreateElement("Package");

			XmlAttribute ManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns");
			ManifestNamespace.Value = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
			Package.Attributes.Append(ManifestNamespace);

			XmlAttribute UapManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns:uap");
			UapManifestNamespace.Value = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
			Package.Attributes.Append(UapManifestNamespace);

			XmlAttribute Uap2ManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns:uap2");
			Uap2ManifestNamespace.Value = "http://schemas.microsoft.com/appx/manifest/uap/windows10/2";
			Package.Attributes.Append(Uap2ManifestNamespace);

			XmlAttribute Uap3ManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns:uap3");
			Uap3ManifestNamespace.Value = "http://schemas.microsoft.com/appx/manifest/uap/windows10/3";
			Package.Attributes.Append(Uap3ManifestNamespace);

			XmlAttribute Uap4ManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns:uap4");
			Uap4ManifestNamespace.Value = "http://schemas.microsoft.com/appx/manifest/uap/windows10/4";
			Package.Attributes.Append(Uap4ManifestNamespace);

			XmlAttribute Uap5ManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns:uap5");
			Uap5ManifestNamespace.Value = "http://schemas.microsoft.com/appx/manifest/uap/windows10/5";
			Package.Attributes.Append(Uap5ManifestNamespace);

			XmlAttribute MpManifestNamespace = AppxManifestXmlDocument.CreateAttribute("xmlns:mp");
			MpManifestNamespace.Value = "http://schemas.microsoft.com/appx/2014/phone/manifest";
			Package.Attributes.Append(MpManifestNamespace);

			XmlAttribute IgnorableNamespaces = AppxManifestXmlDocument.CreateAttribute("IgnorableNamespaces");
			IgnorableNamespaces.Value = "mp uap uap2 uap3 uap4 uap5";
			Package.Attributes.Append(IgnorableNamespaces);


			XmlNode Identity = GetIdentity();
			AddElementIfValid(Package, Identity, true);

			XmlNode Properties = GetProperties();
			AddElementIfValid(Package, Properties, true);

			XmlNode Dependencies = GetDependencies();
			AddElementIfValid(Package, Dependencies, true);

			XmlNode Resources = GetResources();
			AddElementIfValid(Package, Resources, true);

			if (!IsDlc)
			{
				XmlNode Applications = GetApplications(TargetConfigs, Executables);
				AddElementIfValid(Package, Applications, true);

				XmlNode Capabilities = GetCapabilities();
				AddElementIfValid(Package, Capabilities, true);

				XmlNode Extensions = GetPackageExtensions();
				AddElementIfValid(Package, Extensions, false, true);
			}
			return Package;
		}

		/// <summary>
		/// Validate a package name. Must contain only characters [-.A-Za-z0-9].
		/// </summary>
		private string ValidatePackageName(string InPackageName)
		{
			string ReturnVal = Regex.Replace(InPackageName, "[^-.A-Za-z0-9]", "");
			if (ReturnVal == null || ReturnVal.Length <= 0)
			{
				Log.TraceError("Invalid package name {0}. Package names must only contain letters, numbers, dash, and period and must be at least one character long.", InPackageName);
				Log.TraceError("Consider using the setting [/Script/UWPPlatformEditor.UWPTargetSettings]:PackageName to provide a UWP specific value.");
			}
			return ReturnVal;
		}

		/// <summary>
		/// Gather all information for the Identity element of the manifest.
		/// </summary>
		private XmlNode GetIdentity()
		{
			XmlElement Identity = AppxManifestXmlDocument.CreateElement("Identity");

			if (!IsDlc)
			{
				XmlAttribute PackageName = CreateStringAttribute("Name", "PackageName", "Package.Identity.Name", "/Script/EngineSettings.GeneralProjectSettings", "ProjectName", "DefaultUE4Project", ValidatePackageName);
				Identity.Attributes.Append(PackageName);
			}
			else
			{
				XmlAttribute PackageName = AppxManifestXmlDocument.CreateAttribute("Name");
				PackageName.Value = ParsedDlcInfo["PackageIdentityName"];
				ValidatePackageName(PackageName.Value);
				Identity.Attributes.Append(PackageName);
			}

			XmlAttribute ProcessorArchitecture = AppxManifestXmlDocument.CreateAttribute("ProcessorArchitecture");
			ProcessorArchitecture.Value = Platform == UnrealTargetPlatform.UWP64 ? "x64" : "x86";
			Identity.Attributes.Append(ProcessorArchitecture);

			XmlAttribute PublisherName = CreateStringAttribute("Publisher", "PublisherName", "Package.Identity.Publisher", "/Script/EngineSettings.GeneralProjectSettings", "CompanyDistinguishedName", "CN=NoPublisher");
			Identity.Attributes.Append(PublisherName);

			if (!IsDlc)
			{
				XmlAttribute VersionNumber = CreateStringAttribute("Version", "PackageVersion", "Package.Identity.Version", "/Script/EngineSettings.GeneralProjectSettings", "ProjectVersion", "1.0.0.0");
				Identity.Attributes.Append(VersionNumber);
			}
			else
			{
				XmlAttribute VersionNumber = AppxManifestXmlDocument.CreateAttribute("Version");
				VersionNumber.Value = ParsedDlcInfo["PackageIdentityVersion"];
				Identity.Attributes.Append(VersionNumber);
			}

			return Identity;
		}

		/// <summary>
		/// Gather all information for the Properties element of the manifest.
		/// </summary>
		private XmlNode GetProperties()
		{
			XmlElement Properties = AppxManifestXmlDocument.CreateElement("Properties");

			XmlElement DisplayName = AppxManifestXmlDocument.CreateElement("DisplayName");
			DisplayName.InnerText = "ms-resource:PackageDisplayName";
			Properties.AppendChild(DisplayName);
			AddResourceEntry("PackageDisplayName", "PackageDisplayName", "Package.Properties.DisplayName", "/Script/EngineSettings.GeneralProjectSettings", "ProjectDisplayedTitle", "DefaultUE4Project");

			XmlElement PublisherDisplayName = AppxManifestXmlDocument.CreateElement("PublisherDisplayName");
			PublisherDisplayName.InnerText = "ms-resource:PublisherDisplayName";
			Properties.AppendChild(PublisherDisplayName);
			AddResourceEntry("PublisherDisplayName", "PublisherDisplayName", "Package.Properties.PublisherDisplayName", "/Script/EngineSettings.GeneralProjectSettings", "CompanyName", "NoPublisher");

			XmlElement PackageDescription = AppxManifestXmlDocument.CreateElement("Description");
			PackageDescription.InnerText = "ms-resource:PackageDescription";
			Properties.AppendChild(PackageDescription);
			AddResourceEntry("PackageDescription", "PackageDescription", "Package.Properties.Description", "/Script/EngineSettings.GeneralProjectSettings", "Description", "");

			XmlElement PackageLogo = AppxManifestXmlDocument.CreateElement("Logo");
			// Some applications may not have a package logo and use the application logo instead.
			// Try logos in the following order:
			//   1. Project package logo
			//   2. Project application logo
			//   3. Engine application logo (the engine always uses a single logo for package and application)
			if (CopyAndReplaceBinaryIntermediate("StoreLogo.png", false))
			{
				PackageLogo.InnerText = BuildResourceSubPath + "\\StoreLogo.png";
				Properties.AppendChild(PackageLogo);
			}
			else if (CopyAndReplaceBinaryIntermediate("Logo.png"))
			{
				PackageLogo.InnerText = BuildResourceSubPath + "\\Logo.png";
				Properties.AppendChild(PackageLogo);
			}
			else
			{
				Log.TraceError("Unable to stage package logo.");
			}

			return Properties;
		}

		/// <summary>
		/// Gather all information for the Dependencies element of the manifest.
		/// </summary>
		private XmlNode GetDependencies()
		{
			XmlElement Dependencies = AppxManifestXmlDocument.CreateElement("Dependencies");
			
			{
				XmlElement TargetDeviceFamily = AppxManifestXmlDocument.CreateElement("TargetDeviceFamily");
				Dependencies.AppendChild(TargetDeviceFamily);

				XmlAttribute NameAttribute = AppxManifestXmlDocument.CreateAttribute("Name");
				NameAttribute.Value = CreateStringValue("TargetDeviceFamily", "Package.Dependencies.TargetDeviceFamily[0].Name", "TargetDeviceFamily", "Name", "Windows.Universal");
				TargetDeviceFamily.Attributes.Append(NameAttribute);

				XmlAttribute MinVersionAttribute = AppxManifestXmlDocument.CreateAttribute("MinVersion");
				MinVersionAttribute.Value = CreateStringValue("MinimumPlatformVersion", "Package.Dependencies.TargetDeviceFamily[0].MinVersion", "MinimumPlatformVersion", "MinVersion", "10.0.10240.0");
				TargetDeviceFamily.Attributes.Append(MinVersionAttribute);

				XmlAttribute MaxVersionTestedAttribute = AppxManifestXmlDocument.CreateAttribute("MaxVersionTested");
				MaxVersionTestedAttribute.Value = CreateStringValue("MaximumPlatformVersionTested", "Package.Dependencies.TargetDeviceFamily[0].MaxVersionTested", "MaximumPlatformVersionTested", "MaxVersionTested", "10.0.10586.0");
				TargetDeviceFamily.Attributes.Append(MaxVersionTestedAttribute);
			}

			{
				XmlElement PackageDependency = AppxManifestXmlDocument.CreateElement("PackageDependency");
				Dependencies.AppendChild(PackageDependency);

				XmlAttribute NameAttribute = AppxManifestXmlDocument.CreateAttribute("Name");
				NameAttribute.Value = "Microsoft.VCLibs.140.00";
				PackageDependency.Attributes.Append(NameAttribute);

				XmlAttribute PublisherAttribute = AppxManifestXmlDocument.CreateAttribute("Publisher");
				PublisherAttribute.Value = "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US";
				PackageDependency.Attributes.Append(PublisherAttribute);

				XmlAttribute MinVersionAttribute = AppxManifestXmlDocument.CreateAttribute("MinVersion");
				MinVersionAttribute.Value = "14.0.0.0";
				PackageDependency.Attributes.Append(MinVersionAttribute);
			}

			if (IsDlc)
			{
				// Add a dependency for the main package
				XmlElement PackageDependency = AppxManifestXmlDocument.CreateElement("uap3:MainPackageDependency", "http://schemas.microsoft.com/appx/manifest/uap/windows10/3");
				Dependencies.AppendChild(PackageDependency);

				XmlAttribute NameAttribute = CreateStringAttribute("Name", "PackageName", "Package.Identity.Name", "/Script/EngineSettings.GeneralProjectSettings", "ProjectName", "DefaultUE4Project", ValidatePackageName);
				PackageDependency.Attributes.Append(NameAttribute);
			}

			return Dependencies;
		}
 

		/// <summary>
		/// Gather all information for the Prerequisites element of the manifest.
		/// </summary>
		private XmlNode GetPrerequisites()
		{
			XmlElement Prerequisites = AppxManifestXmlDocument.CreateElement("Prerequisites");
			XmlElement OSMinVersion = CreateStringElement("OSMinVersion", "MinimumOSVersion", "Package.Prerequisites.OSMinVersion", TargetSettings, "MinimumOSVersion", "6.2");
			Prerequisites.AppendChild(OSMinVersion);

			XmlElement OSMaxVersionTested = CreateStringElement("OSMaxVersionTested", "MaximumOSVersion", "Package.Prerequisites.OSMaxVersionTested", TargetSettings, "MaximumOSVersion", "6.2");
			Prerequisites.AppendChild(OSMaxVersionTested);

			return Prerequisites;
		}

		/// <summary>
		/// Gather all information for the Resources element of the manifest.
		/// </summary>
		private XmlNode GetResources()
		{
			XmlElement Resources = AppxManifestXmlDocument.CreateElement("Resources");

			// Check that we have a valid number of cultures.
			if (CulturesToStage.Count < 1 || CulturesToStage.Count >= MaxResourceEntries)
			{
				Log.TraceWarning("Incorrect number of cultures to stage. There must be between 1 and {0} cultures selected.", MaxResourceEntries);
			}

			// Create the culture list.
			for (int ResourceIndex = 0; ResourceIndex < CulturesToStage.Count; ResourceIndex++)
			{
				XmlNode Resource = AppxManifestXmlDocument.CreateElement("Resource");

				XmlAttribute LanguageAttribute = AppxManifestXmlDocument.CreateAttribute("Language");
				LanguageAttribute.Value = CulturesToStage[ResourceIndex];
				Resource.Attributes.Append(LanguageAttribute);

				Resources.AppendChild(Resource);
			}

			return Resources;
		}

		/// <summary>
		/// Create an Application manifest entry for each target configuration
		/// </summary>
		private XmlNode GetApplications(List<UnrealTargetConfiguration> TargetConfigs, List<string> Executables)
		{
			XmlElement Applications = AppxManifestXmlDocument.CreateElement("Applications");

			if (TargetConfigs.Count < 1)
			{
				Log.TraceError("No configurations to deploy");
				return Applications;
			}
			if (TargetConfigs.Count != Executables.Count)
			{
				Log.TraceError("The number of executables does not match the number of configurations.");
				return Applications;
			}

			for (int ApplicationIndex = 0; ApplicationIndex < TargetConfigs.Count; ApplicationIndex++)
			{
				bool bIncludeConfigPostfix = TargetConfigs.Count > 1 && TargetConfigs[ApplicationIndex] != UnrealTargetConfiguration.Development;
				XmlNode Application = GetApplication(ApplicationIndex, TargetConfigs[ApplicationIndex], Executables[ApplicationIndex], bIncludeConfigPostfix);
				AddElementIfValid(Applications, Application, true, true);
			}

			return Applications;
		}

		/// <summary>
		/// Validate the base name we use to construct the application id and entry point. Must match [A-Za-z][A-Za-z0-9]*.
		/// </summary>
		private string ValidateApplicationName(string InApplicationId)
		{
			string ReturnVal = Regex.Replace(InApplicationId, "[^A-Za-z0-9]", "");
			if (ReturnVal != null)
			{
				// Remove any leading numbers (must start with a letter)
				ReturnVal = Regex.Replace(ReturnVal, "^[0-9]*", "");
			}
			if (ReturnVal == null || ReturnVal.Length <= 0)
			{
				Log.TraceError("Invalid application ID {0}. Application IDs must only contain letters and numbers. And they must begin with a letter.", InApplicationId);
				Log.TraceError("Consider using the setting [/Script/UWPPlatformEditor.UWPTargetSettings]:ValidateApplicationName to provide a UWP specific value.");
			}
			return ReturnVal;
		}

		/// <summary>
		/// Create an Application manifest entry for a specific target configuration
		/// </summary>
		private XmlNode GetApplication(int ApplicationIndex, UnrealTargetConfiguration TargetConfig, string ExecutablePath, bool bIncludeConfigPostfix)
		{
			XmlElement Application = AppxManifestXmlDocument.CreateElement("Application");

			string PackageBaseName = CreateStringValue("ApplicationName", "Package.Applications.Application[" + ApplicationIndex + "].Id", "/Script/EngineSettings.GeneralProjectSettings", "ProjectName", "UE4Game", ValidateApplicationName);

			string ConfigPostfix = "";
			if (bIncludeConfigPostfix)
			{
				ConfigPostfix = TargetConfig.ToString();
			}

			string MakeRelativeTo = IsGameSpecificExe ? ProjectPath : UnrealBuildTool.EngineDirectory.FullName;
			string RelativeExePath = Path.Combine(RelativeProjectRootForStage, Utils.MakePathRelativeTo(ExecutablePath, MakeRelativeTo));

			XmlAttribute Id = AppxManifestXmlDocument.CreateAttribute("Id");
			Id.Value = "App" + PackageBaseName + ConfigPostfix;
			Application.Attributes.Append(Id);

			XmlAttribute Executable = AppxManifestXmlDocument.CreateAttribute("Executable");
			Executable.Value = RelativeExePath;
			Application.Attributes.Append(Executable);

			XmlAttribute EntryPoint = AppxManifestXmlDocument.CreateAttribute("EntryPoint");
			EntryPoint.Value = PackageBaseName + ".app";
			Application.Attributes.Append(EntryPoint);

			XmlNode VisualElements = GetVisualElements(/*Document, */ApplicationIndex, ConfigPostfix);
			AddElementIfValid(Application, VisualElements, true, true);

			return Application;
		}

		/// <summary>
		/// Gather all information for the VisualElements element of the manifest.
		/// </summary>
		private XmlNode GetVisualElements(int ApplicationIndex, string ConfigPostfix)
		{
			XmlElement VisualElements = AppxManifestXmlDocument.CreateElement("uap:VisualElements", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

			XmlAttribute DisplayName = AppxManifestXmlDocument.CreateAttribute("DisplayName");
			if (ConfigPostfix != null && ConfigPostfix.Length > 0)
			{
				DisplayName.Value = "ms-resource:AppDisplayName" + ConfigPostfix;
				AddResourceEntry("AppDisplayName" + ConfigPostfix, "ApplicationDisplayName", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.DisplayName", "/Script/EngineSettings.GeneralProjectSettings", "ProjectName", "UE4Game", " - " + ConfigPostfix);
			}
			else
			{
				DisplayName.Value = "ms-resource:AppDisplayName";
				AddResourceEntry("AppDisplayName" + ConfigPostfix, "ApplicationDisplayName", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.DisplayName", "/Script/EngineSettings.GeneralProjectSettings", "ProjectName", "UE4Game");
			}
			VisualElements.Attributes.Append(DisplayName);

			XmlAttribute Description = AppxManifestXmlDocument.CreateAttribute("Description");
			Description.Value = "ms-resource:AppDescription";
			VisualElements.Attributes.Append(Description);
			AddResourceEntry("AppDescription", "ApplicationDescription", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.Description", "/Script/EngineSettings.GeneralProjectSettings", "Description", "");

			XmlAttribute BackgroundColor = CreateColorAttribute("BackgroundColor", "TileBackgroundColor", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.BackgroundColor", "#000040");
			VisualElements.Attributes.Append(BackgroundColor);

			XmlAttribute Logo = AppxManifestXmlDocument.CreateAttribute("Square150x150Logo");
			if (CopyAndReplaceBinaryIntermediate("Logo.png"))
			{
				Logo.Value = BuildResourceSubPath + "\\Logo.png";
				VisualElements.Attributes.Append(Logo);
			}
			else
			{
				Log.TraceError("Unable to stage application logo.");
			}

			XmlAttribute SmallLogo = AppxManifestXmlDocument.CreateAttribute("Square44x44Logo");
			if (CopyAndReplaceBinaryIntermediate("SmallLogo.png"))
			{
				SmallLogo.Value = BuildResourceSubPath + "\\SmallLogo.png";
				VisualElements.Attributes.Append(SmallLogo);
			}
			else
			{
				Log.TraceError("Unable to stage application small logo.");
			}

			XmlNode SplashScreen = GetSplashScreen(ApplicationIndex);
			VisualElements.AppendChild(SplashScreen);

			//@todo application support
			// 			XmlNode ViewStates = GetViewStates(Document, ApplicationIndex);
			// 			VisualElements.AppendChild(ViewStates);

			return VisualElements;
		}

		/// <summary>
		/// Gather all information for the DefaultTile element of the manifest.
		/// </summary>
		private XmlNode GetDefaultTile(int ApplicationIndex)
		{
			XmlElement DefaultTile = AppxManifestXmlDocument.CreateElement("DefaultTile");

			XmlAttribute WideLogo = AppxManifestXmlDocument.CreateAttribute("WideLogo");
			if (CopyAndReplaceBinaryIntermediate("WideLogo.png"))
			{
				WideLogo.Value = BuildResourceSubPath + "\\WideLogo.png";
				DefaultTile.Attributes.Append(WideLogo);
			}
			else
			{
				Log.TraceError("Unable to stage application wide logo.");
			}

			// Calculate the short name display conditions and the short name if it will be used
			XmlAttribute ShowName = AppxManifestXmlDocument.CreateAttribute("ShowName");
			ShowName.Value = "noLogos";
			bool bUseShortNameForLogo = false;
			bool bUseShortNameForWideLogo = false;
			bool bShortNameForLogoValueRead = EngineIni.GetBool(TargetSettings, "bUseShortNameForLogo", out bUseShortNameForLogo);
			bool bShortNameForWideLogoValueRead = EngineIni.GetBool(TargetSettings, "bUseShortNameForWideLogo", out bUseShortNameForWideLogo);
			if (bShortNameForLogoValueRead || bShortNameForWideLogoValueRead)
			{
				if (bUseShortNameForLogo && bUseShortNameForWideLogo)
				{
					ShowName.Value = "allLogos";
				}
				else if (bUseShortNameForLogo)
				{
					ShowName.Value = "logoOnly";
				}
				else if (bUseShortNameForWideLogo)
				{
					ShowName.Value = "wideLogoOnly";
				}
			}
			else
			{
				ShowName.Value = GetInterprettedSettingValue("Package.Applications.Application[" + ApplicationIndex + "].VisualElements.DefaultTile.ShowName");
			}
			if (ShowName.Value != null && ShowName.Value.Length > 0 && !ShowName.Value.Equals("noLogos"))

			{
				XmlAttribute ShortName = AppxManifestXmlDocument.CreateAttribute("ShortName");// CreateStringAttribute("ShortName", "ApplicationShortName", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.DefaultTile.ShortName", null, null, "");
				ShortName.Value = "ms-resource:AppShortName";
				AddResourceEntry("AppShortName", "ApplicationShortName", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.DefaultTile.ShortName", null, null, "UE4Game");

				DefaultTile.Attributes.Append(ShortName);
				DefaultTile.Attributes.Append(ShowName);
			}

			return DefaultTile;
		}

		/// <summary>
		/// Gather all information for the SplashScreen element of the manifest.
		/// </summary>
		private XmlNode GetSplashScreen(int ApplicationIndex)
		{
			XmlElement SplashScreen = AppxManifestXmlDocument.CreateElement("uap:SplashScreen", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

			XmlAttribute BackgroundColor = CreateColorAttribute("BackgroundColor", "SplashScreenBackgroundColor", "Package.Applications.Application[" + ApplicationIndex + "].VisualElements.SplashScreen.BackgroundColor", "#000040");
			SplashScreen.Attributes.Append(BackgroundColor);

			XmlAttribute Image = AppxManifestXmlDocument.CreateAttribute("Image");
			if (CopyAndReplaceBinaryIntermediate("SplashScreen.png"))
			{
				Image.Value = BuildResourceSubPath + "\\SplashScreen.png";
				SplashScreen.Attributes.Append(Image);
			}
			else
			{
				Log.TraceError("Unable to stage splash screen image.");
			}

			return SplashScreen;
		}

        // for ease of integration with mainline, allow Epic's implementation for XboxOne to flow through unchanged
        private XmlNode GetCapabilities()
        {
            XmlElement Capabilities = AppxManifestXmlDocument.CreateElement("Capabilities");

            List<string> CapabilityList = new List<string>();
            List<string> DeviceCapabilityList = new List<string>();
            List<string> UapCapabilityList = new List<string>();
            List<string> Uap2CapabilityList = new List<string>();

            if (EngineIni.GetArray(TargetSettings, "CapabilityList", out CapabilityList))
            {
                foreach (string capName in CapabilityList)
                {
                    XmlElement CapabilityElement = AppxManifestXmlDocument.CreateElement("Capability");
                    XmlAttribute Name = AppxManifestXmlDocument.CreateAttribute("Name");
                    Name.Value = capName;
                    CapabilityElement.Attributes.Append(Name);
                    Capabilities.AppendChild(CapabilityElement);
                }
            }

            if (EngineIni.GetArray(TargetSettings, "UapCapabilityList", out UapCapabilityList))
            {
                foreach (string capName in UapCapabilityList)
                {
                    XmlElement CapabilityElement = AppxManifestXmlDocument.CreateElement("uap:Capability", "http://schemas.microsoft.com/appx/manifest/uap/windows10");
                    XmlAttribute Name = AppxManifestXmlDocument.CreateAttribute("Name");
                    Name.Value = capName;
                    CapabilityElement.Attributes.Append(Name);
                    Capabilities.AppendChild(CapabilityElement);
                }
            }

            if (EngineIni.GetArray(TargetSettings, "Uap2CapabilityList", out Uap2CapabilityList))
            {
                foreach (string capName in Uap2CapabilityList)
                {
                    XmlElement CapabilityElement = AppxManifestXmlDocument.CreateElement("uap2:Capability", "http://schemas.microsoft.com/appx/manifest/uap/windows10/2");
                    XmlAttribute Name = AppxManifestXmlDocument.CreateAttribute("Name");
                    Name.Value = capName;
                    CapabilityElement.Attributes.Append(Name);
                    Capabilities.AppendChild(CapabilityElement);
                }
            }

            if (EngineIni.GetArray(TargetSettings, "DeviceCapabilityList", out DeviceCapabilityList))
            {
                foreach (string capName in DeviceCapabilityList)
                {
                    XmlElement CapabilityElement = AppxManifestXmlDocument.CreateElement("DeviceCapability");
                    XmlAttribute Name = AppxManifestXmlDocument.CreateAttribute("Name");
                    Name.Value = capName;
                    CapabilityElement.Attributes.Append(Name);
                    Capabilities.AppendChild(CapabilityElement);
                }
            }

            return Capabilities;
        }

		/// <summary>
		/// Gather and create manifest for the package extension entries. There are multiple possible extension types
		/// that can occur in any order and may individually be present or absent. A great deal of the complexity of this
		/// function deals with correlating the old and new style INI entries across these order differences.
		/// </summary>
		private XmlNode GetPackageExtensions()
		{
			XmlElement Extensions = AppxManifestXmlDocument.CreateElement("Extensions");

			foreach (var WinMD in WinMDReferences)
			{
				XmlElement ExtensionElement = AppxManifestXmlDocument.CreateElement("Extension");
				Extensions.AppendChild(ExtensionElement);

				XmlAttribute CategoryAttribute = AppxManifestXmlDocument.CreateAttribute("Category");
				CategoryAttribute.Value = "windows.activatableClass.inProcessServer";
				ExtensionElement.Attributes.Append(CategoryAttribute);

				XmlElement InProcessServerElement = AppxManifestXmlDocument.CreateElement("InProcessServer");
				ExtensionElement.AppendChild(InProcessServerElement);

				XmlElement PathElement = AppxManifestXmlDocument.CreateElement("Path");
				InProcessServerElement.AppendChild(PathElement);
				PathElement.InnerText = WinMD.PackageRelativeDllPath;

				foreach (var WinMDType in WinMD.ActivatableTypes)
				{
					XmlElement ActivatableClassElement = AppxManifestXmlDocument.CreateElement("ActivatableClass");
					InProcessServerElement.AppendChild(ActivatableClassElement);

					XmlAttribute ActivatableClassIdAttribute = AppxManifestXmlDocument.CreateAttribute("ActivatableClassId");
					ActivatableClassIdAttribute.Value = WinMDType.TypeName;
					ActivatableClassElement.Attributes.Append(ActivatableClassIdAttribute);

					XmlAttribute ThreadingModelAttribute = AppxManifestXmlDocument.CreateAttribute("ThreadingModel");
					ThreadingModelAttribute.Value = WinMDType.ThreadingModelName;
					ActivatableClassElement.Attributes.Append(ThreadingModelAttribute);
				}
			}

			//@todo outOfProcessServer
			//@todo proxyStub
			//@todo windows.certificates

			if (!Extensions.HasChildNodes)
			{
				return null;
			}

			return Extensions;
		}

		private void ValidateAppxManifest(string ManifestPath)
		{
			System.Xml.Schema.XmlSchemaSet AppxSchema = new System.Xml.Schema.XmlSchemaSet();

			DirectoryReference VSInstallDir;
			DirectoryReference SdkSchemaFolder = null;
			DirectoryReference VSSchemaFolder = null;
			DirectoryReference PhoneSchemaFolder = null;

			DirectoryReference SDKRootFolder;
			Version SDKVersion;

			UWPExports.GetWindowsSDKInstallationFolder(out SDKRootFolder, out SDKVersion);

			SdkSchemaFolder = DirectoryReference.Combine(SDKRootFolder, "Include", SDKVersion.ToString(), "winrt");
			PhoneSchemaFolder = DirectoryReference.Combine(SDKRootFolder, "Extension SDKs", "WindowsMobile", SDKVersion.ToString(), "Include", "WinRT");

			if (WindowsPlatform.TryGetVSInstallDir(WindowsCompiler.VisualStudio2019, out VSInstallDir))
			{
				VSSchemaFolder = DirectoryReference.Combine(VSInstallDir, "Xml", "Schemas");
			}

			string[] RequiredSchemas =
			{
				"AppxManifestTypes.xsd",
				"UapManifestSchema.xsd",
				"UapManifestSchema_v2.xsd",
				"UapManifestSchema_v3.xsd",
				"UapManifestSchema_v4.xsd",
				"UapManifestSchema_v5.xsd",
				"UapManifestSchema_v6.xsd",
				"FoundationManifestSchema.xsd",
				"AppxManifestSchema2010_v3.xsd",
				"AppxManifestSchema2013_v2.xsd",
				"AppxManifestSchema2014.xsd",
				"AppxPhoneManifestSchema2014.xsd",
				"DesktopManifestSchema_v2.xsd",
				"DesktopManifestSchema_v3.xsd",
				"DesktopManifestSchema_v4.xsd",
				"IotManifestSchema_v2.xsd"
			};

			foreach (string SchemaName in RequiredSchemas)
			{
				FileReference SchemaFile = null;

				if (VSSchemaFolder != null)
				{
					SchemaFile = FileReference.Combine(VSSchemaFolder, SchemaName);
				}

				if ((SchemaFile == null || !FileReference.Exists(SchemaFile)) && SdkSchemaFolder != null)
				{
					SchemaFile = FileReference.Combine(SdkSchemaFolder, SchemaName);
				}

				if ((SchemaFile == null || !FileReference.Exists(SchemaFile)) && PhoneSchemaFolder != null)
				{
					SchemaFile = FileReference.Combine(PhoneSchemaFolder, SchemaName);
				}

				if (SchemaFile != null && FileReference.Exists(SchemaFile))
				{
					AppxSchema.Add(null, XmlReader.Create(SchemaFile.FullName));
				}
			}

			try
			{
				AppxSchema.Compile();
			}
			catch (System.Xml.Schema.XmlSchemaException e)
			{
				string InvalidSchemaWarning =
					"\r\n" +
					"{0}({1}): {2}\r\n" +
					"XML schema failed to compile; validation of the final AppxManifest.xml will be skipped.\r\n" +
					"If your AppxManifest.xml is valid then this is harmless, but if it contains invalid content you may encounter packaging or deployment errors.\r\n" +
					"Updating your Windows SDK and/or Visual Studio installation may correct the schema problems and simplify diagnosis of invalid content.\r\n";

				Log.TraceWarning(InvalidSchemaWarning, e.SourceUri, e.LineNumber, e.Message);
				return;
			}

			bool ValidationSucceeded = true;
			XmlReaderSettings ReaderSettings = new XmlReaderSettings();
			ReaderSettings.ValidationType = ValidationType.Schema;
			ReaderSettings.Schemas = AppxSchema;
			ReaderSettings.ValidationEventHandler += (source, args) =>
			{
				switch (args.Severity)
				{
					case System.Xml.Schema.XmlSeverityType.Error:
						Log.TraceError(args.Message);
						ValidationSucceeded = false;
						break;

					case System.Xml.Schema.XmlSeverityType.Warning:
						Log.TraceWarning(args.Message);
						break;

					default:
						break;
				}
			};

			using (XmlReader ValidatingReader = XmlReader.Create(ManifestPath, ReaderSettings))
			{
				while (ValidatingReader.Read())
				{
					// No-op, just reading to end to force validation.
				}
			}

			if (!ValidationSucceeded)
			{
				throw new BuildException("Generated AppxManifest ({0}) is invalid.  See log for details and check your UWP Project Settings.", ManifestPath);
			}
		}
	};
}
