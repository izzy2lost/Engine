// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

#include "UWPPlatformAtomics.h"
#include "CoreMinimal.h"

#if PLATFORM_UWP

void FUWPAtomics::HandleAtomicsFailure( const TCHAR* InFormat, ... )
{	
	TCHAR TempStr[1024];
	va_list Ptr;

	va_start( Ptr, InFormat );	
	FPlatformString::GetVarArgs(TempStr, ARRAY_COUNT(TempStr), InFormat, Ptr);
	va_end( Ptr );

	UE_LOG(LogTemp, Log, TempStr);
	check( 0 );
}

#endif //PLATFORM_UWP