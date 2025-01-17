#include "UWPDlcSettingsCustomization.h"

#include "DetailLayoutBuilder.h"
#include "DetailCategoryBuilder.h"
#include "DetailWidgetRow.h"
#include "PropertyHandle.h"
#include "PropertyCustomizationHelpers.h"
#include "IDetailChildrenBuilder.h"
#include "IDetailGroup.h"
#include "Widgets/Input/STextComboBox.h"
#include "Interfaces/IPluginManager.h"
#include "Widgets/STooltip.h"
#include "Settings/ProjectPackagingSettings.h"
#include "UWPLocalizedResourcesCustomization.h"

#define LOCTEXT_NAMESPACE "UWPDlcSettingsCustomization"

TSharedRef<IPropertyTypeCustomization> FUWPDlcSettingsCustomization::MakeInstance()
{
	return MakeShareable(new FUWPDlcSettingsCustomization);
}

void FUWPDlcSettingsCustomization::CustomizeHeader(TSharedRef<class IPropertyHandle> StructPropertyHandle, class FDetailWidgetRow& HeaderRow, IPropertyTypeCustomizationUtils& StructCustomizationUtils)
{
	TSharedRef<IPropertyHandle> PluginNameProperty = StructPropertyHandle->GetChildHandle(FName("PluginName")).ToSharedRef();

	HeaderRow
	.NameContent()
	[
		PluginNameProperty->CreatePropertyNameWidget()
	]
	.ValueContent()
	[
		PropertyCustomizationHelpers::MakePropertyComboBox(PluginNameProperty, FOnGetPropertyComboBoxStrings::CreateSP(this, &FUWPDlcSettingsCustomization::GeneratePluginNameComboOptions))
	];
}

void FUWPDlcSettingsCustomization::CustomizeChildren(TSharedRef<IPropertyHandle> InStructPropertyHandle, class IDetailChildrenBuilder& ChildBuilder, IPropertyTypeCustomizationUtils& StructCustomizationUtils)
{
	ChildBuilder.AddProperty(InStructPropertyHandle->GetChildHandle(FName("PackageIdentityName")).ToSharedRef());
	ChildBuilder.AddProperty(InStructPropertyHandle->GetChildHandle(FName("PackageIdentityVersion")).ToSharedRef());

	// Up 2 levels - first is for the array of DlcSettings, second reaches DLC category of UWPTargetSettings
	TSharedPtr<IPropertyHandle> LocalizedResourcesProperty = InStructPropertyHandle->GetParentHandle()->GetParentHandle()->GetChildHandle("DLCPerCultureResources");

	TSharedRef<IPropertyHandle> PluginNameProperty = InStructPropertyHandle->GetChildHandle(FName("PluginName")).ToSharedRef();
	FString CurrentPluginName;
	PluginNameProperty->GetValue(CurrentPluginName);
	ChildBuilder.AddCustomBuilder(MakeShared<FUWPLocalizedResourcesNodeBuilder>(LocalizedResourcesProperty.ToSharedRef(), CurrentPluginName));
}

void FUWPDlcSettingsCustomization::GeneratePluginNameComboOptions(TArray<TSharedPtr<FString>>& ComboOptions, TArray<TSharedPtr<SToolTip>>& Tooltips, TArray<bool>& RestrictedItems)
{
	TArray<TSharedRef<IPlugin>> DiscoveredPlugins = IPluginManager::Get().GetDiscoveredPlugins();
	for (TSharedRef<IPlugin>& Plugin : DiscoveredPlugins)
	{
		if (Plugin->GetType() == EPluginType::Project &&
			!Plugin->IsEnabled())
		{
			const FPluginDescriptor& Descriptor = Plugin->GetDescriptor();
			if (Descriptor.SupportsTargetPlatform(TEXT("UWP64")) || Descriptor.SupportsTargetPlatform(TEXT("UWP32")))
			{
				ComboOptions.Add(MakeShared<FString>(Plugin->GetName()));
				Tooltips.Add(SNew(SToolTip).Text(FText::FromString(Plugin->GetDescriptor().Description)));
				RestrictedItems.Add(false);
			}
		}
	}
}

#undef LOCTEXT_NAMESPACE