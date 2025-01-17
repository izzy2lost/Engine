// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class Launch : ModuleRules
{
	public Launch(ReadOnlyTargetRules Target) : base(Target)
	{
		PrivateIncludePaths.Add("Runtime/Launch/Private");

		PrivateIncludePathModuleNames.AddRange(new string[] {
				"AutomationController",
				"TaskGraph",
		});

		PrivateDependencyModuleNames.AddRange(new string[] {
				"Core",
				"CoreUObject",
				"Engine",
				"InputCore",
				"MoviePlayer",
				"Networking",
				"PakFile",
				"Projects",
				"RenderCore",
				"RHI",
				"SandboxFile",
				"Serialization",
				"ApplicationCore",
				"Slate",
				"SlateCore",
				"Sockets",
				"TraceLog",
				"Overlay",
				"PreLoadScreen",
				"InstallBundleManager"
			});

		// Set a macro allowing us to switch between debuggame/development configuration
		if (Target.Configuration == UnrealTargetConfiguration.DebugGame)
		{
			PrivateDefinitions.Add("UE_BUILD_DEVELOPMENT_WITH_DEBUGGAME=1");
		}
		else
		{
			PrivateDefinitions.Add("UE_BUILD_DEVELOPMENT_WITH_DEBUGGAME=0");
		}

		// Enable the LauncherCheck module to be used for platforms that support the Launcher.
		// Projects should set Target.bUseLauncherChecks in their Target.cs to enable the functionality.
		if (Target.bUseLauncherChecks &&
			((Target.Platform == UnrealTargetPlatform.Win32) ||
			(Target.Platform == UnrealTargetPlatform.Win64) ||
			(Target.Platform == UnrealTargetPlatform.Mac)))
		{
			PrivateDependencyModuleNames.Add("LauncherCheck");
			PublicDefinitions.Add("WITH_LAUNCHERCHECK=1");
		}
		else
		{
			PublicDefinitions.Add("WITH_LAUNCHERCHECK=0");
		}

		if (Target.Type != TargetType.Server)
		{
			PrivateDependencyModuleNames.AddRange(new string[] {
					"HeadMountedDisplay",
				"MediaUtils",
					"MRMesh",
			});

			if (Target.Platform == UnrealTargetPlatform.Win32 || Target.Platform == UnrealTargetPlatform.Win64)
			{
				DynamicallyLoadedModuleNames.AddRange(new string[] {
					"WindowsPlatformFeatures",
				});
			}

			if (Target.Platform.IsInGroup(UnrealPlatformGroup.Windows))
			{
				DynamicallyLoadedModuleNames.AddRange(new string[] {
					"AudioMixerXAudio2",
					"XAudio2",
				});
			}
			else if (Target.Platform == UnrealTargetPlatform.HoloLens)
			{
				DynamicallyLoadedModuleNames.Add("D3D11RHI");
				DynamicallyLoadedModuleNames.Add("XAudio2");
				DynamicallyLoadedModuleNames.Add("AudioMixerXAudio2");
			}
			// @EMMETTJNR_CHANGE : BEGIN UWP support
			else if (Target.Platform == UnrealTargetPlatform.UWP32 ||
					Target.Platform == UnrealTargetPlatform.UWP64)
			{
				DynamicallyLoadedModuleNames.Add("D3D12RHI");
                DynamicallyLoadedModuleNames.Add("XAudio2");
                DynamicallyLoadedModuleNames.Add("AudioMixerXAudio2");
			}
			// @EMMETTJNR_CHANGE : END
			else if (Target.Platform == UnrealTargetPlatform.Mac)
			{
				DynamicallyLoadedModuleNames.AddRange(new string [] {
					"AudioMixerCoreAudio",
					"CoreAudio"
				});
				PublicFrameworks.Add("CoreAudio");
			}
			else if (Target.IsInPlatformGroup(UnrealPlatformGroup.Unix))
			{
				DynamicallyLoadedModuleNames.Add("AudioMixerSDL");
				PrivateDependencyModuleNames.Add("Json");
			}

			PrivateIncludePathModuleNames.AddRange(new string[] {
				"Media",
					"SlateNullRenderer",
				"SlateRHIRenderer",
			});

			DynamicallyLoadedModuleNames.AddRange(new string[] {
				"Media",
					"SlateNullRenderer",
				"SlateRHIRenderer",
			});
		}

		// UFS clients are not available in shipping builds
		if (Target.Configuration != UnrealTargetConfiguration.Shipping)
		{
			PrivateDependencyModuleNames.AddRange(new string[] {
					"NetworkFile",
					"StreamingFile",
					"CookedIterativeFile",
					"AutomationWorker"
			});
		}

		DynamicallyLoadedModuleNames.AddRange(new string[] {
				"Renderer",
		});

		if (Target.bCompileAgainstEngine)
		{
			PrivateIncludePathModuleNames.AddRange(new string[] {
					"MessagingCommon",
			});

			PublicDependencyModuleNames.Add("SessionServices");
			PrivateIncludePaths.Add("Developer/DerivedDataCache/Public");

			// LaunchEngineLoop.cpp will still attempt to load XMPP but not all projects require it so it will silently fail unless referenced by the project's build.cs file.
			// DynamicallyLoadedModuleNames.Add("XMPP");

			DynamicallyLoadedModuleNames.AddRange(new string[] {
				"HTTP",
				"MediaAssets",
			});

			PrivateDependencyModuleNames.AddRange(new string[] {
				"ClothingSystemRuntimeNv",
				"ClothingSystemRuntimeInterface"
			});

			if (Target.Configuration != UnrealTargetConfiguration.Shipping)
			{
				PrivateDependencyModuleNames.AddRange(new string[] {
					"FunctionalTesting"
				});
			}
		}

		if (Target.Configuration != UnrealTargetConfiguration.Shipping)
		{
			PublicIncludePathModuleNames.Add("ProfilerService");

			DynamicallyLoadedModuleNames.AddRange(new string[] {
				"TaskGraph",
				"RealtimeProfiler",
				"ProfilerService"
			});
		}

		// The engine can use AutomationController in any connfiguration besides shipping.  This module is loaded
		// dynamically in LaunchEngineLoop.cpp in non-shipping configurations
		if (Target.bCompileAgainstEngine && Target.Configuration != UnrealTargetConfiguration.Shipping)
		{
			DynamicallyLoadedModuleNames.AddRange(new string[] { "AutomationController" });
		}

		if (Target.bBuildEditor == true)
		{
			PublicIncludePathModuleNames.Add("ProfilerClient");

			PrivateDependencyModuleNames.AddRange(new string[] {
					"SourceControl",
					"UnrealEd",
					"DesktopPlatform",
					"PIEPreviewDeviceProfileSelector",
			});


			// ExtraModules that are loaded when WITH_EDITOR=1 is true
			DynamicallyLoadedModuleNames.AddRange(new string[] {
					"AutomationWindow",
					"ProfilerClient",
					"Toolbox",
					"GammaUI",
					"ModuleUI",
					"OutputLog",
					"TextureCompressor",
					"MeshUtilities",
					"SourceCodeAccess"
			});

			if (Target.Platform == UnrealTargetPlatform.Mac)
			{
				PrivateDependencyModuleNames.AddRange(new string[] {
					"MainFrame",
					"Settings",
				});
			}
			else
			{
				DynamicallyLoadedModuleNames.Add("MainFrame");
			}
		}

		if (Target.IsInPlatformGroup(UnrealPlatformGroup.Android))
		{
			PrivateDependencyModuleNames.Add("OpenGLDrv");
			if (Target.Platform != UnrealTargetPlatform.Lumin)
			{
				PrivateDependencyModuleNames.Add("AndroidAudio");
				PrivateDependencyModuleNames.Add("AudioMixerAndroid");
			}
			// these are, for now, only for basic android
			if (Target.Platform == UnrealTargetPlatform.Android)
			{
				DynamicallyLoadedModuleNames.Add("AndroidRuntimeSettings");
				DynamicallyLoadedModuleNames.Add("AndroidLocalNotification");
			}
			else if (Target.Platform == UnrealTargetPlatform.Lumin)
			{
				DynamicallyLoadedModuleNames.Add("LuminRuntimeSettings");
			}
		}

		if (Target.Platform == UnrealTargetPlatform.IOS || Target.Platform == UnrealTargetPlatform.TVOS)
		{
			PrivateDependencyModuleNames.AddRange(new string[] {
				"AudioMixerAudioUnit",
				"IOSAudio",
				"LaunchDaemonMessages",
			});

			DynamicallyLoadedModuleNames.AddRange(new string[] {
				"IOSLocalNotification",
				"IOSRuntimeSettings",
			});

			// needed for Metal layer
			PublicFrameworks.Add("QuartzCore");
		}

		if ((Target.Platform == UnrealTargetPlatform.Win32) ||
			(Target.Platform == UnrealTargetPlatform.Win64) ||
			(Target.IsInPlatformGroup(UnrealPlatformGroup.Linux) && Target.Type != TargetType.Server))
		{
			// TODO: re-enable after implementing resource tables for OpenGL.
			DynamicallyLoadedModuleNames.Add("OpenGLDrv");
		}

        // @todo ps4 clang bug: this works around a PS4/clang compiler bug (optimizations)
        if (Target.Platform == UnrealTargetPlatform.PS4)
		{
			bUseUnity = true;
		}

		if (Target.IsInPlatformGroup(UnrealPlatformGroup.Unix))
		{
			// Clang 9.0.1 lld seems to end up having issues with resolving EditorStyle
			// when dealing with circular dependencies on SourceControl module
			if (Target.bBuildEditor == true)
			{
				PrivateDependencyModuleNames.Add("EditorStyle");
			}

			PrivateDependencyModuleNames.Add("UnixCommonStartup");
		}

		if(Target.LinkType == TargetLinkType.Monolithic && !Target.bFormalBuild)
		{
			PrivateDefinitions.Add(string.Format("COMPILED_IN_CL={0}", Target.Version.Changelist));
			PrivateDefinitions.Add(string.Format("COMPILED_IN_COMPATIBLE_CL={0}", Target.Version.EffectiveCompatibleChangelist));
			PrivateDefinitions.Add(string.Format("COMPILED_IN_BRANCH_NAME={0}", (Target.Version.BranchName == null || Target.Version.BranchName.Length == 0)? "UE4" : Target.Version.BranchName));
		}
	}
}
