using UnrealBuildTool;
using System;
using System.IO;
using System.Collections.Generic;

public class MixerInteractivity : ModuleRules
{
	enum Backend
	{
		InteractiveCppV1,
		InteractiveCppV2,
		Null,
		UE,
	}

	public MixerInteractivity(ReadOnlyTargetRules Target) : base(Target)
	{
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				"Core",
				"CoreUObject",
				"Engine",
				"HTTP",
				"Json",
				"JsonUtilities",
				"SlateCore",
				"Slate",
				"UMG",
				"WebSockets",
			});

		// Need Version.h
		PrivateIncludePathModuleNames.Add("Launch");
		PrivateIncludePathModuleNames.Add("OnlineSubsystem");

		Backend SelectedBackend = Backend.Null;
		string ThirdPartyFolder = Path.Combine(ModuleDirectory, "..", "..", "ThirdParty");
		PrivateIncludePaths.Add(Path.Combine(ThirdPartyFolder, "Include"));
		
		string LibPath = Path.Combine(ThirdPartyFolder, "Lib", Target.Platform.ToString());

		if (Target.Platform == UnrealTargetPlatform.Win64 || Target.Platform == UnrealTargetPlatform.Win32)
		{
			SelectedBackend = Backend.InteractiveCppV2;

			AddPublicDefinition("PLATFORM_SUPPORTS_MIXER_OAUTH=1");
			PrivateDependencyModuleNames.AddRange(
				new string[]
				{
					"WebBrowser",
				});
		}
		else if (Target.Platform == UnrealTargetPlatform.XboxOne)
		{
			SelectedBackend = Backend.InteractiveCppV2;

			AddPublicDefinition("PLATFORM_SUPPORTS_MIXER_OAUTH=0");
		}
		else if (Target.Platform == UnrealTargetPlatform.UWP64 || Target.Platform == UnrealTargetPlatform.UWP32)
		{
			SelectedBackend = Backend.InteractiveCppV2;

			AddPublicDefinition("PLATFORM_SUPPORTS_MIXER_OAUTH=0");

			PrivateDependencyModuleNames.Add("OnlineSubsystemUtils");
		}
		else
		{
			AddPublicDefinition("PLATFORM_SUPPORTS_MIXER_OAUTH=0");
		}

		if (SelectedBackend == Backend.InteractiveCppV1)
		{
			PrivateIncludePaths.Add(Path.Combine(ThirdPartyFolder, "Include", "interactive-cpp"));
			if (Target.Platform == UnrealTargetPlatform.Win64 || Target.Platform == UnrealTargetPlatform.Win32)
			{
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "Interactivity.Win32.Cpp.lib"));
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "cpprest140_2_9.lib"));
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "winhttp.lib"));
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "crypt32.lib"));
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "bcrypt.lib"));
			}
			else if (Target.Platform == UnrealTargetPlatform.UWP64 || Target.Platform == UnrealTargetPlatform.UWP32)
			{
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "Interactivity.UWP.Cpp.lib"));
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "cpprest140_uwp_2_9.lib"));
			}
			else if (Target.Platform == UnrealTargetPlatform.XboxOne)
			{
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "Interactivity.Xbox.Cpp.lib"));
				PublicAdditionalLibraries.Add(Path.Combine(LibPath, "casablanca140.xbox.lib"));
			}
		}

		AddPrivateDefinition(string.Format("MIXER_BACKEND_INTERACTIVE_CPP={0}", SelectedBackend == Backend.InteractiveCppV1 ? 1 : 0));
		AddPrivateDefinition(string.Format("MIXER_BACKEND_INTERACTIVE_CPP_2={0}", SelectedBackend == Backend.InteractiveCppV2 ? 1 : 0));
		AddPrivateDefinition(string.Format("MIXER_BACKEND_NULL={0}", SelectedBackend == Backend.Null ? 1 : 0));
		AddPrivateDefinition(string.Format("MIXER_BACKEND_UE={0}", SelectedBackend == Backend.UE ? 1 : 0));

		bEnableExceptions = true;
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
		bUseUnity = false;
	}

	void AddPrivateDefinition(string Definition)
	{
#if UE_4_19_OR_LATER
		PrivateDefinitions.Add(Definition);
#else
		Definitions.Add(Definition);
#endif
	}

	void AddPublicDefinition(string Definition)
	{
#if UE_4_19_OR_LATER
		PublicDefinitions.Add(Definition);
#else
		Definitions.Add(Definition);
#endif
	}
}