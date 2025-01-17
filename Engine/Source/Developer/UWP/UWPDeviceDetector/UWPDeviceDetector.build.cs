using UnrealBuildTool;

public class UWPDeviceDetector : ModuleRules
{
	public UWPDeviceDetector(ReadOnlyTargetRules Target) : base(Target)
	{
		PrivateDependencyModuleNames.AddRange(
			new string[] {
				"Core",
				"CoreUObject",
				"TargetPlatform",
				"DesktopPlatform",
				"HTTP",
			}
		);

		PrivateIncludePaths.AddRange(
			new string[] {
				"Developer/UWP/UWPTargetPlatform/Private"
			}
		);

		if (Target.WindowsPlatform.bUseWindowsSDK10)
		{
			bEnableWinRTComponentExtensions = true;
			bEnableExceptions = true;
			PCHUsage = PCHUsageMode.NoSharedPCHs;
			PrivatePCHHeaderFile = "Public/IUWPDeviceDetectorModule.h";
			PublicDefinitions.Add("USE_WINRT_DEVICE_WATCHER=1");
		}
		else
		{
			PublicDefinitions.Add("USE_WINRT_DEVICE_WATCHER=0");
		}
	}
}