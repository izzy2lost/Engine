//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

#if WITH_WEBSOCKETS

#include "UWPWebSocketsManager.h"

TSharedRef<IWebSocket> FUWPWebSocketsManager::CreateWebSocket(const FString& Url, const TArray<FString>& Protocols, const TMap<FString, FString>& UpgradeHeaders)
{
	return MakeShared<FUWPWebSocket>(Url, Protocols, UpgradeHeaders);
}

#endif // #if WITH_WEBSOCKETS