// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "SocketSubsystem.h"
#include "BSDIPv6Sockets/SocketSubsystemBSDIPv6.h"
#include "BSDIPv6Sockets/SocketsBSDIPv6.h"
#include "SocketSubsystemPackage.h"


/**
 * UWP specific socket subsystem implementation
 */
class FSocketSubsystemUWP
	: public FSocketSubsystemBSDIPv6
{
public:

	/** Default Constructor. */
	FSocketSubsystemUWP() :
		bTriedToInit(false)
	{ }

	/** Virtual destructor. */
	virtual ~FSocketSubsystemUWP() { }

public:

	// FSocketSubsystemBSD overrides

	virtual class FSocket* CreateSocket(const FName& SocketType, const FString& SocketDescription, bool bForceUDP = false) override;
	virtual ESocketErrors GetHostByName(const ANSICHAR* HostName, FInternetAddr& OutAddr) override;
	virtual bool HasNetworkDevice() override;
	virtual ESocketErrors GetLastErrorCode() override;
	virtual bool GetLocalAdapterAddresses(TArray<TSharedPtr<FInternetAddr> >& OutAdresses) override;
	virtual const TCHAR* GetSocketAPIName() const override;
	virtual bool Init(FString& Error) override;
	virtual void Shutdown() override;
	virtual ESocketErrors TranslateErrorCode(int32 Code) override;

PACKAGE_SCOPE:

	/**
	* Singleton interface for this subsystem
	*
	* @return the only instance of this subsystem
	*/
	static FSocketSubsystemUWP* Create();

	/** Performs UWP specific socket clean up. */
	static void Destroy();

protected:

	/** Holds a flag indicating whether Init() has been called before or not. */
	bool bTriedToInit;

	/** Holds the single instantiation of this subsystem. */
	static FSocketSubsystemUWP* SocketSingleton;

private:

	// Used to prevent multiple threads accessing the shared data.
	FCriticalSection HostByNameSynch;
};