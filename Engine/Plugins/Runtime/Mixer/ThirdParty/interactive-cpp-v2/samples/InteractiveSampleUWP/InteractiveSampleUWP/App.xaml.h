﻿//
// App.xaml.h
// Declaration of the App class.
//

#pragma once

#include "App.g.h"

#include<memory>

namespace InteractiveSampleUWP
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	ref class App sealed
	{
	protected:
		virtual void OnLaunched(Windows::ApplicationModel::Activation::LaunchActivatedEventArgs^ e) override;

	internal:
		App();

	private:
		void OnSuspending(Platform::Object^ sender, Windows::ApplicationModel::SuspendingEventArgs^ e);
		void OnNavigationFailed(Platform::Object ^sender, Windows::UI::Xaml::Navigation::NavigationFailedEventArgs ^e);
		bool m_appIsRunning;
		std::unique_ptr<std::thread> m_interactiveThread;
		std::map<std::string, std::string> m_controlsByTransaction;
	};
}
