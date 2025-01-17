// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

#include "UWPRunnableThread.h"
#include "Misc/OutputDeviceError.h"
#include "HAL/ExceptionHandling.h"


DEFINE_LOG_CATEGORY_STATIC(LogThreadingWindows, Log, All);


uint32 FRunnableThreadUWP::GuardedRun()
{
	uint32 ExitCode = 1;

	FPlatformProcess::SetThreadAffinityMask(ThreadAffintyMask);

#if PLATFORM_XBOXONE
	UE_LOG(LogThreadingWindows, Log, TEXT("Runnable thread %s is on Process %d."), *ThreadName, static_cast<uint32>(::GetCurrentProcessorNumber()));
#endif

#if !PLATFORM_SEH_EXCEPTIONS_DISABLED
	if (!FPlatformMisc::IsDebuggerPresent() || GAlwaysReportCrash)
	{
		__try
		{
			ExitCode = Run();
		}
		__except (ReportCrash(GetExceptionInformation()))
		{
			// Make sure the information which thread crashed makes it into the log.
			UE_LOG(LogThreadingWindows, Error, TEXT("Runnable thread %s crashed."), *ThreadName);
			GWarn->Flush();

			// Append the thread name at the end of the error report.
			FCString::Strncat(GErrorHist, LINE_TERMINATOR TEXT("Crash in runnable thread "), UE_ARRAY_COUNT(GErrorHist));
			FCString::Strncat(GErrorHist, *ThreadName, UE_ARRAY_COUNT(GErrorHist));

			ExitCode = 1;
			// Generate status report.				
			GError->HandleError();
			// Throw an error so that main thread shuts down too (otherwise task graph stalls forever).
			UE_LOG(LogThreadingWindows, Fatal, TEXT("Runnable thread %s crashed."), *ThreadName);
		}
	}
	else
#endif
	{
		ExitCode = Run();
	}

#if STATS
	FThreadStats::Shutdown();
#endif

	return ExitCode;
}


uint32 FRunnableThreadUWP::Run()
{
	// Assume we'll fail init
	uint32 ExitCode = 1;
	check(Runnable);

	// Initialize the runnable object
	if (Runnable->Init() == true)
	{
		// Initialization has completed, release the sync event
		ThreadInitSyncEvent->Trigger();
		// Now run the task that needs to be done
		ExitCode = Runnable->Run();
		// Allow any allocated resources to be cleaned up
		Runnable->Exit();
	}
	else
	{
		// Initialization has failed, release the sync event
		ThreadInitSyncEvent->Trigger();
	}

	return ExitCode;
}
