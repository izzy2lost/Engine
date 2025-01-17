// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

/*=============================================================================
	UWPEvent.h: Declares the FEventWin class.
=============================================================================*/

#pragma once

#include "AllowWindowsPlatformTypes.h"

/**
 * Implements the UWP version of the FEvent interface.
 */
class FEventUWP : public FEvent
{
public:

	/**
	 * Default constructor.
	 */
	FEventUWP()
		: Event(NULL)
	{
	}

	/**
	 * Destructor.
	 */
	virtual ~FEventUWP()
	{
		if (Event != NULL)
		{
			CloseHandle(Event);
		}
	}


public:

	virtual bool Create (bool bIsManualReset = false) override
	{
		// Create the event and default it to non-signaled
//		Event = CreateEvent(NULL, bIsManualReset, 0, NULL);
		Event = CreateEventEx(NULL, NULL, bIsManualReset ? CREATE_EVENT_MANUAL_RESET : 0, EVENT_ALL_ACCESS);
		ManualReset = bIsManualReset;

		return Event != NULL;
	}

	virtual bool IsManualReset() override
	{
		return ManualReset;
	}

	virtual void Trigger () override
	{
		check(Event);

		SetEvent(Event);
	}

	virtual void Reset () override
	{
		check(Event);

		ResetEvent(Event);
	}

	virtual bool Wait(uint32 WaitTime, const bool bIgnoreThreadIdleStats = false) override;


private:

	/** Whether the signaled state of the event needs to be reset manually. */
	bool ManualReset;

	// Holds the handle to the event
	HANDLE Event;
};

#include "HideWindowsPlatformTypes.h"
