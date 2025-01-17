// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Tools.DotNETCommon;

namespace UnrealBuildTool
{
	static class UnrealBuildTool
	{
		/// <summary>
		/// Save the application startup time. This can be used as the timestamp for build makefiles, to determine a base time after which any
		/// modifications should invalidate it.
		/// </summary>
		static public DateTime StartTimeUtc { get; } = DateTime.UtcNow;

		/// <summary>
		/// The environment at boot time.
		/// </summary>
		static public System.Collections.IDictionary InitialEnvironment;

		/// <summary>
		/// Whether we're running with engine installed
		/// </summary>
		static private bool? bIsEngineInstalled;

		/// <summary>
		/// Whether we're running with enterprise installed
		/// </summary>
		static private bool? bIsEnterpriseInstalled;

		/// <summary>
		/// Whether we're running with an installed project
		/// </summary>
		static private bool? bIsProjectInstalled;

		/// <summary>
		/// If we are running with an installed project, specifies the path to it
		/// </summary>
		static FileReference InstalledProjectFile;

		/// <summary>
		/// Directory for saved application settings (typically Engine/Programs)
		/// </summary>
		static DirectoryReference CachedEngineProgramSavedDirectory;

		/// <summary>
		/// The path to UBT
		/// </summary>
		public static readonly FileReference UnrealBuildToolPath = FileReference.FindCorrectCase(new FileReference(Assembly.GetExecutingAssembly().GetOriginalLocation()));

		/// <summary>
		/// The full name of the Root UE4 directory
		/// </summary>
		public static readonly DirectoryReference RootDirectory = DirectoryReference.Combine(UnrealBuildToolPath.Directory, "..", "..", "..");

		/// <summary>
		/// The full name of the Engine directory
		/// </summary>
		public static readonly DirectoryReference EngineDirectory = DirectoryReference.Combine(RootDirectory, "Engine");

		/// <summary>
		/// The full name of the Engine/Source directory
		/// </summary>
		public static readonly DirectoryReference EngineSourceDirectory = DirectoryReference.Combine(EngineDirectory, "Source");

		/// <summary>
		/// Full path to the Engine/Source/Runtime directory
		/// </summary>
		[Obsolete("Please use UnrealBuildTool.GetExtensionDirs(UnrealBuildTool.EngineDirectory, \"Source/Runtime\") instead.")]
		public static readonly DirectoryReference EngineSourceRuntimeDirectory = DirectoryReference.Combine(EngineSourceDirectory, "Runtime");

		/// <summary>
		/// Full path to the Engine/Source/Developer directory
		/// </summary>
		[Obsolete("Please use UnrealBuildTool.GetExtensionDirs(UnrealBuildTool.EngineDirectory, \"Source/Developer\") instead.")]
		public static readonly DirectoryReference EngineSourceDeveloperDirectory = DirectoryReference.Combine(EngineSourceDirectory, "Developer");

		/// <summary>
		/// Full path to the Engine/Source/Editor directory
		/// </summary>
		[Obsolete("Please use UnrealBuildTool.GetExtensionDirs(UnrealBuildTool.EngineDirectory, \"Source/Editor\") instead.")]
		public static readonly DirectoryReference EngineSourceEditorDirectory = DirectoryReference.Combine(EngineSourceDirectory, "Editor");

		/// <summary>
		/// Full path to the Engine/Source/Programs directory
		/// </summary>
		[Obsolete("Please use UnrealBuildTool.GetExtensionDirs(UnrealBuildTool.EngineDirectory, \"Source/Programs\") instead.")]
		public static readonly DirectoryReference EngineSourceProgramsDirectory = DirectoryReference.Combine(EngineSourceDirectory, "Programs");

		/// <summary>
		/// Full path to the Engine/Source/ThirdParty directory
		/// </summary>
		[Obsolete("Please use UnrealBuildTool.GetExtensionDirs(UnrealBuildTool.EngineDirectory, \"Source/ThirdParty\") instead.")]
		public static readonly DirectoryReference EngineSourceThirdPartyDirectory = DirectoryReference.Combine(EngineSourceDirectory, "ThirdParty");

		/// <summary>
		/// The full name of the Enterprise directory
		/// </summary>
		public static readonly DirectoryReference EnterpriseDirectory = DirectoryReference.Combine(RootDirectory, "Enterprise");

		/// <summary>
		/// The full name of the Enterprise/Source directory
		/// </summary>
		public static readonly DirectoryReference EnterpriseSourceDirectory = DirectoryReference.Combine(EnterpriseDirectory, "Source");

		/// <summary>
		/// The full name of the Enterprise/Plugins directory
		/// </summary>
		public static readonly DirectoryReference EnterprisePluginsDirectory = DirectoryReference.Combine(EnterpriseDirectory, "Plugins");

		/// <summary>
		/// The full name of the Enterprise/Intermediate directory
		/// </summary>
		public static readonly DirectoryReference EnterpriseIntermediateDirectory = DirectoryReference.Combine(EnterpriseDirectory, "Intermediate");

		/// <summary>
		/// The engine programs directory
		/// </summary>
		public static DirectoryReference EngineProgramSavedDirectory
		{
			get
			{
				if (CachedEngineProgramSavedDirectory == null)
				{
					if (IsEngineInstalled())
					{
						CachedEngineProgramSavedDirectory = Utils.GetUserSettingDirectory() ?? DirectoryReference.Combine(EngineDirectory, "Programs");
					}
					else
					{
						CachedEngineProgramSavedDirectory = DirectoryReference.Combine(EngineDirectory, "Programs");
					}
				}
				return CachedEngineProgramSavedDirectory;
			}
		}

		// cached dictionary of BaseDir to extension directories
		private static Dictionary<DirectoryReference, Tuple<List<DirectoryReference>, List<DirectoryReference>>> CachedExtensionDirectories = new Dictionary<DirectoryReference, Tuple<List<DirectoryReference>, List<DirectoryReference>>>();

		/// <summary>
		/// Finds all the extension directories for the given base directory. This includes platform extensions and restricted folders.
		/// </summary>
		/// <param name="BaseDir">Location of the base directory</param>
		/// <param name="bIncludePlatformDirectories">If true, platform subdirectories are included (will return platform directories under Restricted dirs, even if bIncludeRestrictedDirectories is false)</param>
		/// <param name="bIncludeRestrictedDirectories">If true, restricted (NotForLicensees, NoRedist) subdirectories are included</param>
		/// <param name="bIncludeBaseDirectory">If true, BaseDir is included</param>
		/// <returns>List of extension directories, including the given base directory</returns>
		public static List<DirectoryReference> GetExtensionDirs(DirectoryReference BaseDir, bool bIncludePlatformDirectories=true, bool bIncludeRestrictedDirectories=true, bool bIncludeBaseDirectory=true)
		{
			Tuple<List<DirectoryReference>, List<DirectoryReference>> CachedDirs;
			if (!CachedExtensionDirectories.TryGetValue(BaseDir, out CachedDirs))
			{
				CachedDirs = Tuple.Create(new List<DirectoryReference>(), new List<DirectoryReference>());

				CachedExtensionDirectories[BaseDir] = CachedDirs;

				DirectoryReference PlatformExtensionBaseDir = DirectoryReference.Combine(BaseDir, "Platforms");
				if (DirectoryReference.Exists(PlatformExtensionBaseDir))
				{
					CachedDirs.Item1.AddRange(DirectoryReference.EnumerateDirectories(PlatformExtensionBaseDir));
				}

				DirectoryReference RestrictedBaseDir = DirectoryReference.Combine(BaseDir, "Restricted");
				if (DirectoryReference.Exists(RestrictedBaseDir))
				{
					IEnumerable<DirectoryReference> RestrictedDirs = DirectoryReference.EnumerateDirectories(RestrictedBaseDir);
					CachedDirs.Item2.AddRange(RestrictedDirs);

					// also look for nested platforms in the restricted
					foreach (DirectoryReference RestrictedDir in RestrictedDirs)
					{
						DirectoryReference RestrictedPlatformExtensionBaseDir = DirectoryReference.Combine(RestrictedDir, "Platforms");
						if (DirectoryReference.Exists(RestrictedPlatformExtensionBaseDir))
						{
							CachedDirs.Item1.AddRange(DirectoryReference.EnumerateDirectories(RestrictedPlatformExtensionBaseDir));
						}
					}
				}

				// remove any platform directories in non-engine locations if the engine doesn't have the platform 
				if (BaseDir != UnrealBuildTool.EngineDirectory && CachedDirs.Item1.Count > 0)
				{
					// if the DDPI.ini file doesn't exist, we haven't synced the platform, so just skip this directory
					CachedDirs.Item1.RemoveAll(x => DataDrivenPlatformInfo.GetDataDrivenInfoForPlatform(x.GetDirectoryName()) == null);
				}
			}

			// now return what the caller wanted (always include BaseDir)
			List<DirectoryReference> ExtensionDirs = new List<DirectoryReference>();
			if (bIncludeBaseDirectory)
			{
				ExtensionDirs.Add(BaseDir);
			}
			if (bIncludePlatformDirectories)
			{
				ExtensionDirs.AddRange(CachedDirs.Item1);
			}
			if (bIncludeRestrictedDirectories)
			{
				ExtensionDirs.AddRange(CachedDirs.Item2);
			}
			return ExtensionDirs;
		}

		/// <summary>
		/// Finds all the extension directories for the given base directory. This includes platform extensions and restricted folders.
		/// </summary>
		/// <param name="BaseDir">Location of the base directory</param>
		/// <param name="SubDir">The subdirectory to find</param>
		/// <param name="bIncludePlatformDirectories">If true, platform subdirectories are included (will return platform directories under Restricted dirs, even if bIncludeRestrictedDirectories is false)</param>
		/// <param name="bIncludeRestrictedDirectories">If true, restricted (NotForLicensees, NoRedist) subdirectories are included</param>
		/// <param name="bIncludeBaseDirectory">If true, BaseDir is included</param>
		/// <returns>List of extension directories, including the given base directory</returns>
		public static List<DirectoryReference> GetExtensionDirs(DirectoryReference BaseDir, string SubDir, bool bIncludePlatformDirectories=true, bool bIncludeRestrictedDirectories=true, bool bIncludeBaseDirectory=true)
		{
			return GetExtensionDirs(BaseDir, bIncludePlatformDirectories, bIncludeRestrictedDirectories, bIncludeBaseDirectory).Select(x => DirectoryReference.Combine(x, SubDir)).Where(x => DirectoryReference.Exists(x)).ToList();
		}

		/// <summary>
		/// The Remote Ini directory.  This should always be valid when compiling using a remote server.
		/// </summary>
		static string RemoteIniPath = null;

		/// <summary>
		/// Returns true if UnrealBuildTool is running using installed Engine components
		/// </summary>
		/// <returns>True if running using installed Engine components</returns>
		static public bool IsEngineInstalled()
		{
			if (!bIsEngineInstalled.HasValue)
			{
				bIsEngineInstalled = FileReference.Exists(FileReference.Combine(EngineDirectory, "Build", "InstalledBuild.txt"));
			}
			return bIsEngineInstalled.Value;
		}

		/// <summary>
		/// Returns true if UnrealBuildTool is running using installed Enterprise components
		/// </summary>
		/// <returns>True if running using installed Enterprise components</returns>
		static public bool IsEnterpriseInstalled()
		{
			if (!bIsEnterpriseInstalled.HasValue)
			{
				bIsEnterpriseInstalled = FileReference.Exists(FileReference.Combine(EnterpriseDirectory, "Build", "InstalledBuild.txt"));
			}
			return bIsEnterpriseInstalled.Value;
		}

		/// <summary>
		/// Returns true if UnrealBuildTool is running using an installed project (ie. a mod kit)
		/// </summary>
		/// <returns>True if running using an installed project</returns>
		static public bool IsProjectInstalled()
		{
			if (!bIsProjectInstalled.HasValue)
			{
				FileReference InstalledProjectLocationFile = FileReference.Combine(UnrealBuildTool.RootDirectory, "Engine", "Build", "InstalledProjectBuild.txt");
				if (FileReference.Exists(InstalledProjectLocationFile))
				{
					InstalledProjectFile = FileReference.Combine(UnrealBuildTool.RootDirectory, File.ReadAllText(InstalledProjectLocationFile.FullName).Trim());
					bIsProjectInstalled = true;
				}
				else
				{
					InstalledProjectFile = null;
					bIsProjectInstalled = false;
				}
			}
			return bIsProjectInstalled.Value;
		}

		/// <summary>
		/// Gets the installed project file
		/// </summary>
		/// <returns>Location of the installed project file</returns>
		static public FileReference GetInstalledProjectFile()
		{
			if(IsProjectInstalled())
			{
				return InstalledProjectFile;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Checks whether the given file is under an installed directory, and should not be overridden
		/// </summary>
		/// <param name="File">File to test</param>
		/// <returns>True if the file is part of the installed distribution, false otherwise</returns>
		static public bool IsFileInstalled(FileReference File)
		{
			if(IsEngineInstalled() && File.IsUnderDirectory(EngineDirectory))
			{
				return true;
			}
			if(IsEnterpriseInstalled() && File.IsUnderDirectory(EnterpriseDirectory))
			{
				return true;
			}
			if(IsProjectInstalled() && File.IsUnderDirectory(InstalledProjectFile.Directory))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the absolute path to the UBT assembly.
		/// </summary>
		/// <returns>A string containing the path to the UBT assembly.</returns>
		static public FileReference GetUBTPath()
		{
			return UnrealBuildToolPath;
		}

		/// <summary>
		/// The Unreal remote tool ini directory.  This should be valid if compiling using a remote server
		/// </summary>
		/// <returns>The directory path</returns>
		static public string GetRemoteIniPath()
		{
			return RemoteIniPath;
		}

		static public void SetRemoteIniPath(string Path)
		{
			RemoteIniPath = Path;
		}

		/// <summary>
		/// Global options for UBT (any modes)
		/// </summary>
		class GlobalOptions
		{
			/// <summary>
			/// The amount of detail to write to the log
			/// </summary>
			[CommandLine(Prefix = "-Verbose", Value ="Verbose")]
			[CommandLine(Prefix = "-VeryVerbose", Value ="VeryVerbose")]
			public LogEventType LogOutputLevel = LogEventType.Log;

			/// <summary>
			/// Specifies the path to a log file to write. Note that the default mode (eg. building, generating project files) will create a log file by default if this not specified.
			/// </summary>
			[CommandLine(Prefix = "-Log")]
			public FileReference LogFileName = null;

			/// <summary>
			/// Whether to include timestamps in the log
			/// </summary>
			[CommandLine(Prefix = "-Timestamps")]
			public bool bLogTimestamps = false;

			/// <summary>
			/// Whether to format messages in MsBuild format
			/// </summary>
			[CommandLine(Prefix = "-FromMsBuild")]
			public bool bLogFromMsBuild = false;

			/// <summary>
			/// Whether to write progress markup in a format that can be parsed by other programs
			/// </summary>
			[CommandLine(Prefix = "-Progress")]
			public bool bWriteProgressMarkup = false;

			/// <summary>
			/// Whether to ignore the mutex
			/// </summary>
			[CommandLine(Prefix = "-NoMutex")]
			public bool bNoMutex = false;

			/// <summary>
			/// Whether to wait for the mutex rather than aborting immediately
			/// </summary>
			[CommandLine(Prefix = "-WaitMutex")]
			public bool bWaitMutex = false;

			/// <summary>
			/// Whether to wait for the mutex rather than aborting immediately
			/// </summary>
			[CommandLine(Prefix = "-RemoteIni")]
			public string RemoteIni = "";

			/// <summary>
			/// The mode to execute
			/// </summary>
			[CommandLine]
			[CommandLine("-Clean", Value="Clean")]
			[CommandLine("-ProjectFiles", Value="GenerateProjectFiles")]
			[CommandLine("-ProjectFileFormat=", Value="GenerateProjectFiles")]
			[CommandLine("-Makefile", Value="GenerateProjectFiles")]
			[CommandLine("-CMakefile", Value="GenerateProjectFiles")]
			[CommandLine("-QMakefile", Value="GenerateProjectFiles")]
			[CommandLine("-KDevelopfile", Value="GenerateProjectFiles")]
			[CommandLine("-CodeliteFiles", Value="GenerateProjectFiles")]
			[CommandLine("-XCodeProjectFiles", Value="GenerateProjectFiles")]
			[CommandLine("-EdditProjectFiles", Value="GenerateProjectFiles")]
			[CommandLine("-VSCode", Value="GenerateProjectFiles")]
			[CommandLine("-VSMac", Value="GenerateProjectFiles")]
			[CommandLine("-CLion", Value="GenerateProjectFiles")]
			[CommandLine("-Rider", Value="GenerateProjectFiles")]
			public string Mode = null;

			/// <summary>
			/// Initialize the options with the given command line arguments
			/// </summary>
			/// <param name="Arguments"></param>
			public GlobalOptions(CommandLineArguments Arguments)
			{
				Arguments.ApplyTo(this);
				if (!string.IsNullOrEmpty(RemoteIni))
				{
					UnrealBuildTool.SetRemoteIniPath(RemoteIni);
				}
			}
		}

		/// <summary>
		/// Main entry point. Parses any global options and initializes the logging system, then invokes the appropriate command.
		/// </summary>
		/// <param name="ArgumentsArray">Command line arguments</param>
		/// <returns>Zero on success, non-zero on error</returns>
		private static int Main(string[] ArgumentsArray)
		{
			SingleInstanceMutex Mutex = null;
			try
			{
				// Start capturing performance info
				Timeline.Start();

				// Parse the command line arguments
				CommandLineArguments Arguments = new CommandLineArguments(ArgumentsArray);

				// Parse the global options
				GlobalOptions Options = new GlobalOptions(Arguments);

				// Configure the log system
				Log.OutputLevel = Options.LogOutputLevel;
				Log.IncludeTimestamps = Options.bLogTimestamps;
				Log.IncludeProgramNameWithSeverityPrefix = Options.bLogFromMsBuild;

				// Configure the progress writer
				ProgressWriter.bWriteMarkup = Options.bWriteProgressMarkup;

				// Add the log writer if requested. When building a target, we'll create the writer for the default log file later.
				if(Options.LogFileName != null)
				{
					Log.AddFileWriter("LogTraceListener", Options.LogFileName);
				}

				// Ensure we can resolve any external assemblies that are not in the same folder as our assembly.
				AssemblyUtils.InstallAssemblyResolver(Path.GetDirectoryName(Assembly.GetEntryAssembly().GetOriginalLocation()));

				// Change the working directory to be the Engine/Source folder. We are likely running from Engine/Binaries/DotNET
				// This is critical to be done early so any code that relies on the current directory being Engine/Source will work.
				DirectoryReference.SetCurrentDirectory(UnrealBuildTool.EngineSourceDirectory);

				// Get the type of the mode to execute, using a fast-path for the build mode.
				Type ModeType = typeof(BuildMode);
				if(Options.Mode != null)
				{
					// Find all the valid modes
					Dictionary<string, Type> ModeNameToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
					foreach(Type Type in Assembly.GetExecutingAssembly().GetTypes())
					{
						if(Type.IsClass && !Type.IsAbstract && Type.IsSubclassOf(typeof(ToolMode)))
						{
							ToolModeAttribute Attribute = Type.GetCustomAttribute<ToolModeAttribute>();
							if(Attribute == null)
							{
								throw new BuildException("Class '{0}' should have a ToolModeAttribute", Type.Name);
							}
							ModeNameToType.Add(Attribute.Name, Type);
						}
					}

					// Try to get the correct mode
					if(!ModeNameToType.TryGetValue(Options.Mode, out ModeType))
					{
						Log.TraceError("No mode named '{0}'. Available modes are:\n  {1}", Options.Mode, String.Join("\n  ", ModeNameToType.Keys));
						return 1;
					}
				}

				// Get the options for which systems have to be initialized for this mode
				ToolModeOptions ModeOptions = ModeType.GetCustomAttribute<ToolModeAttribute>().Options;

				// Start prefetching the contents of the engine folder
				if((ModeOptions & ToolModeOptions.StartPrefetchingEngine) != 0)
				{
					using(Timeline.ScopeEvent("FileMetadataPrefetch.QueueEngineDirectory()"))
					{
						FileMetadataPrefetch.QueueEngineDirectory();
					}
				}

				// Read the XML configuration files
				if((ModeOptions & ToolModeOptions.XmlConfig) != 0)
				{
					using(Timeline.ScopeEvent("XmlConfig.ReadConfigFiles()"))
					{
						string XmlConfigMutexName = SingleInstanceMutex.GetUniqueMutexForPath("UnrealBuildTool_Mutex_XmlConfig", Assembly.GetExecutingAssembly().CodeBase);
						using(SingleInstanceMutex XmlConfigMutex = new SingleInstanceMutex(XmlConfigMutexName, true))
						{
							FileReference XmlConfigCache = Arguments.GetFileReferenceOrDefault("-XmlConfigCache=", null);
							XmlConfig.ReadConfigFiles(XmlConfigCache);
						}
					}
				}

				// Acquire a lock for this branch
				if((ModeOptions & ToolModeOptions.SingleInstance) != 0 && !Options.bNoMutex)
				{
					using(Timeline.ScopeEvent("SingleInstanceMutex.Acquire()"))
					{
						string MutexName = SingleInstanceMutex.GetUniqueMutexForPath("UnrealBuildTool_Mutex", Assembly.GetExecutingAssembly().CodeBase);
						Mutex = new SingleInstanceMutex(MutexName, Options.bWaitMutex);
					}
				}

				// Register all the build platforms
				if((ModeOptions & ToolModeOptions.BuildPlatforms) != 0)
				{
					using(Timeline.ScopeEvent("UEBuildPlatform.RegisterPlatforms()"))
					{
						UEBuildPlatform.RegisterPlatforms(false, false);
					}
				}
				if ((ModeOptions & ToolModeOptions.BuildPlatformsHostOnly) != 0)
				{
					using (Timeline.ScopeEvent("UEBuildPlatform.RegisterPlatforms()"))
					{
						UEBuildPlatform.RegisterPlatforms(false, true);
					}
				}
				if ((ModeOptions & ToolModeOptions.BuildPlatformsForValidation) != 0)
				{
					using(Timeline.ScopeEvent("UEBuildPlatform.RegisterPlatforms()"))
					{
						UEBuildPlatform.RegisterPlatforms(true, false);
					}
				}

				// Create the appropriate handler
				ToolMode Mode = (ToolMode)Activator.CreateInstance(ModeType);

				// Execute the mode
				int Result = Mode.Execute(Arguments);
				if((ModeOptions & ToolModeOptions.ShowExecutionTime) != 0)
				{
					Log.TraceInformation("Total execution time: {0:0.00} seconds", Timeline.Elapsed.TotalSeconds);
				}

				
				return Result;
			}
			catch (CompilationResultException Ex)
			{
				// Used to return a propagate a specific exit code after an error has occurred. Does not log any message.
				Log.TraceLog(ExceptionUtils.FormatExceptionDetails(Ex));
				return (int)Ex.Result;
			}
			catch (BuildException Ex)
			{
				// BuildExceptions should have nicely formatted messages. We can log these directly.
				Log.TraceError(ExceptionUtils.FormatException(Ex));
				Log.TraceLog(ExceptionUtils.FormatExceptionDetails(Ex));
				return (int)CompilationResult.OtherCompilationError;
			}
			catch (Exception Ex)
			{
				// Unhandled exception.
				Log.TraceError("Unhandled exception: {0}", ExceptionUtils.FormatException(Ex));
				Log.TraceLog(ExceptionUtils.FormatExceptionDetails(Ex));
				return (int)CompilationResult.OtherCompilationError;
			}
			finally
			{
				// Cancel the prefetcher
				using(Timeline.ScopeEvent("FileMetadataPrefetch.Stop()"))
				{
					FileMetadataPrefetch.Stop();
				}

				// Print out all the performance info
				Timeline.Print(TimeSpan.FromMilliseconds(20.0), LogEventType.Log);

				// Make sure we flush the logs however we exit
				Trace.Close();

				// Write any trace logs
				TraceSpan.Flush();

				// Dispose of the mutex. Must be done last to ensure that another process does not startup and start trying to write to the same log file.
				if (Mutex != null)
				{
					Mutex.Dispose();
				}
			}
		}
	}
}
