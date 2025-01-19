// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma once
#pragma pack(push, 16)

#include "EtwPlus.h"

#if defined(__cplusplus)
extern "C" {
#endif

// Field Descriptors, used in the ETX_EVENT_DESCRIPTOR array below
//
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_AchievementUpdate_Fields[5] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_EnemyDefeated_Fields[16] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Float,0},{EtxFieldType_Float,0},{EtxFieldType_Float,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_GameProgress_Fields[4] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_Float,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_MediaUsage_Fields[27] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UInt32,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UInt64,0},{EtxFieldType_UInt32,0},{EtxFieldType_Float,0},{EtxFieldType_UInt64,0},{EtxFieldType_UInt64,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Float,0},{EtxFieldType_UInt32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_MultiplayerRoundEnd_Fields[11] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Float,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_MultiplayerRoundStart_Fields[9] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_ObjectiveEnd_Fields[9] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_ObjectiveStart_Fields[8] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PageAction_Fields[9] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PageView_Fields[9] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PlayerSessionEnd_Fields[7] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PlayerSessionPause_Fields[4] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PlayerSessionResume_Fields[6] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PlayerSessionStart_Fields[6] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_PuzzleSolved_Fields[8] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Float,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_SectionEnd_Fields[8] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_SectionStart_Fields[7] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_GUID,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_Int32,0},{EtxFieldType_Int32,0}};
EXTERN_C __declspec(selectany) ETX_FIELD_DESCRIPTOR XDKS_0301D082_ViewOffer_Fields[5] = {{EtxFieldType_UnicodeString,0},{EtxFieldType_UnicodeString,0},{EtxFieldType_GUID,0},{EtxFieldType_GUID,0},{EtxFieldType_GUID,0}};

// Event name mapping
//
#define AchievementUpdate_value 1
#define EnemyDefeated_value 2
#define GameProgress_value 3
#define MediaUsage_value 4
#define MultiplayerRoundEnd_value 5
#define MultiplayerRoundStart_value 6
#define ObjectiveEnd_value 7
#define ObjectiveStart_value 8
#define PageAction_value 9
#define PageView_value 10
#define PlayerSessionEnd_value 11
#define PlayerSessionPause_value 12
#define PlayerSessionResume_value 13
#define PlayerSessionStart_value 14
#define PuzzleSolved_value 15
#define SectionEnd_value 16
#define SectionStart_value 17
#define ViewOffer_value 18

// Event Descriptor array
//
EXTERN_C __declspec(selectany) ETX_EVENT_DESCRIPTOR XDKS_0301D082Events[18] =  {
    {{ 1, 0, 0, 0, 0, 0, 0x0 }, "AchievementUpdate", "0.7.IGAU-1.0", XDKS_0301D082_AchievementUpdate_Fields, 5, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 2, 0, 0, 0, 0, 0, 0x0 }, "EnemyDefeated", "0.7.IGED-2.0", XDKS_0301D082_EnemyDefeated_Fields, 16, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 3, 0, 0, 0, 0, 0, 0x0 }, "GameProgress", "0.7.IGGP-2.0", XDKS_0301D082_GameProgress_Fields, 4, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 4, 0, 0, 0, 0, 0, 0x0 }, "MediaUsage", "0.7.MAUMU-2.0", XDKS_0301D082_MediaUsage_Fields, 27, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 5, 0, 0, 0, 0, 0, 0x0 }, "MultiplayerRoundEnd", "0.7.IGMRE-2.0", XDKS_0301D082_MultiplayerRoundEnd_Fields, 11, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 6, 0, 0, 0, 0, 0, 0x0 }, "MultiplayerRoundStart", "0.7.IGMRS-2.0", XDKS_0301D082_MultiplayerRoundStart_Fields, 9, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 7, 0, 0, 0, 0, 0, 0x0 }, "ObjectiveEnd", "0.7.IGOE-3.0", XDKS_0301D082_ObjectiveEnd_Fields, 9, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 8, 0, 0, 0, 0, 0, 0x0 }, "ObjectiveStart", "0.7.IGOS-2.0", XDKS_0301D082_ObjectiveStart_Fields, 8, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 9, 0, 0, 0, 0, 0, 0x0 }, "PageAction", "0.7.IGPA-1.0", XDKS_0301D082_PageAction_Fields, 9, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 10, 0, 0, 0, 0, 0, 0x0 }, "PageView", "0.7.IGPV-1.0", XDKS_0301D082_PageView_Fields, 9, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 11, 0, 0, 0, 0, 0, 0x0 }, "PlayerSessionEnd", "0.7.IGPSE-2.0", XDKS_0301D082_PlayerSessionEnd_Fields, 7, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 12, 0, 0, 0, 0, 0, 0x0 }, "PlayerSessionPause", "0.7.IGPSPA-2.0", XDKS_0301D082_PlayerSessionPause_Fields, 4, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 13, 0, 0, 0, 0, 0, 0x0 }, "PlayerSessionResume", "0.7.IGPSR-2.0", XDKS_0301D082_PlayerSessionResume_Fields, 6, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 14, 0, 0, 0, 0, 0, 0x0 }, "PlayerSessionStart", "0.7.IGPSS-2.0", XDKS_0301D082_PlayerSessionStart_Fields, 6, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 15, 0, 0, 0, 0, 0, 0x0 }, "PuzzleSolved", "0.7.IGPS-2.0", XDKS_0301D082_PuzzleSolved_Fields, 8, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 16, 0, 0, 0, 0, 0, 0x0 }, "SectionEnd", "0.7.IGSE-2.0", XDKS_0301D082_SectionEnd_Fields, 8, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 17, 0, 0, 0, 0, 0, 0x0 }, "SectionStart", "0.7.IGSS-2.0", XDKS_0301D082_SectionStart_Fields, 7, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault },
    {{ 18, 0, 0, 0, 0, 0, 0x0 }, "ViewOffer", "0.7.IGVO-1.0", XDKS_0301D082_ViewOffer_Fields, 5, 0, EtxEventEnabledState_Undefined, EtxEventEnabledState_ProviderDefault, EtxPopulationSample_Undefined, EtxPopulationSample_UseProviderPopulationSample, EtxEventLatency_Undefined, EtxEventLatency_ProviderDefault, EtxEventPriority_Undefined, EtxEventPriority_ProviderDefault }};

// Provider Descriptor for XDKS_0301D082
//
EXTERN_C __declspec(selectany) ETX_PROVIDER_DESCRIPTOR XDKS_0301D082Provider = {"XDKS_0301D082", {0x78a06edc,0xf3cd,0x4628,{0x98,0x94,0x39,0x7f,0x3c,0x2a,0x72,0x1b}}, 18, (ETX_EVENT_DESCRIPTOR*)&XDKS_0301D082Events, 0, EtxProviderEnabledState_Undefined, EtxProviderEnabledState_OnByDefault, 0, 100, EtxProviderLatency_Undefined, EtxProviderLatency_RealTime, EtxProviderPriority_Undefined, EtxProviderPriority_Critical};

// ETW handle for XDKS_0301D082
//
EXTERN_C __declspec(selectany) REGHANDLE XDKS_0301D082Handle = (REGHANDLE)0;

/*++

Routine Description:

    Register the provider with ETW+.

Arguments:
    
    None

Remarks:

    ERROR_SUCCESS if success or if the provider was already registered. 
    Otherwise, an error code.

--*/
#define EventRegisterXDKS_0301D082() EtxRegister(&XDKS_0301D082Provider, &XDKS_0301D082Handle)

/*++

Routine Description:

    Unregister the provider from ETW+.

Arguments:
            None
Remarks:
    ERROR_SUCCESS if success or if the provider was not registered. 
    Otherwise, an error code.
--*/
#define EventUnregisterXDKS_0301D082() EtxUnregister(&XDKS_0301D082Provider, &XDKS_0301D082Handle)

#define EventEnabledAchievementUpdate() (TRUE)

// Entry point to log the event AchievementUpdate
//
__inline
ULONG
EventWriteAchievementUpdate(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR AchievementId, __in const signed int PercentComplete)
{
#define ARGUMENT_COUNT_XDKS_0301D082_AchievementUpdate 5

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_AchievementUpdate];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], (AchievementId != NULL) ? AchievementId : L"", (AchievementId != NULL) ? (ULONG)((wcslen(AchievementId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[4], &PercentComplete, sizeof(PercentComplete));

    return EtxEventWrite(&XDKS_0301D082Events[0], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_AchievementUpdate, EventData);
}
#define EventEnabledEnemyDefeated() (TRUE)

// Entry point to log the event EnemyDefeated
//
__inline
ULONG
EventWriteEnemyDefeated(__in_opt PCWSTR UserId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId, __in LPCGUID RoundId, __in const signed int PlayerRoleId, __in const signed int PlayerWeaponId, __in const signed int EnemyRoleId, __in const signed int KillTypeId, __in const float LocationX, __in const float LocationY, __in const float LocationZ, __in const signed int EnemyWeaponId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_EnemyDefeated 16

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_EnemyDefeated];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[3], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[4], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[6], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[7], RoundId, sizeof(GUID));
    EventDataDescCreate(&EventData[8], &PlayerRoleId, sizeof(PlayerRoleId));
    EventDataDescCreate(&EventData[9], &PlayerWeaponId, sizeof(PlayerWeaponId));
    EventDataDescCreate(&EventData[10], &EnemyRoleId, sizeof(EnemyRoleId));
    EventDataDescCreate(&EventData[11], &KillTypeId, sizeof(KillTypeId));
    EventDataDescCreate(&EventData[12], &LocationX, sizeof(LocationX));
    EventDataDescCreate(&EventData[13], &LocationY, sizeof(LocationY));
    EventDataDescCreate(&EventData[14], &LocationZ, sizeof(LocationZ));
    EventDataDescCreate(&EventData[15], &EnemyWeaponId, sizeof(EnemyWeaponId));

    return EtxEventWrite(&XDKS_0301D082Events[1], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_EnemyDefeated, EventData);
}
#define EventEnabledGameProgress() (TRUE)

// Entry point to log the event GameProgress
//
__inline
ULONG
EventWriteGameProgress(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in const float CompletionPercent)
{
#define ARGUMENT_COUNT_XDKS_0301D082_GameProgress 4

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_GameProgress];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], &CompletionPercent, sizeof(CompletionPercent));

    return EtxEventWrite(&XDKS_0301D082Events[2], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_GameProgress, EventData);
}
#define EventEnabledMediaUsage() (TRUE)

// Entry point to log the event MediaUsage
//
__inline
ULONG
EventWriteMediaUsage(__in_opt PCWSTR AppSessionId, __in_opt PCWSTR AppSessionStartDateTime, __in const unsigned int UserIdType, __in_opt PCWSTR UserId, __in_opt PCWSTR SubscriptionTierType, __in_opt PCWSTR SubscriptionTier, __in_opt PCWSTR MediaType, __in_opt PCWSTR ProviderId, __in_opt PCWSTR ProviderMediaId, __in_opt PCWSTR ProviderMediaInstanceId, __in LPCGUID BingId, __in const unsigned __int64 MediaLengthMs, __in const unsigned int MediaControlAction, __in const float PlaybackSpeed, __in const unsigned __int64 MediaPositionMs, __in const unsigned __int64 PlaybackDurationMs, __in_opt PCWSTR AcquisitionType, __in_opt PCWSTR AcquisitionContext, __in_opt PCWSTR AcquisitionContextType, __in_opt PCWSTR AcquisitionContextId, __in const signed int PlaybackIsStream, __in const signed int PlaybackIsTethered, __in_opt PCWSTR MarketplaceLocation, __in_opt PCWSTR ContentLocale, __in const float TimeZoneOffset, __in const unsigned int ScreenState)
{
#define ARGUMENT_COUNT_XDKS_0301D082_MediaUsage 27

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_MediaUsage];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (AppSessionId != NULL) ? AppSessionId : L"", (AppSessionId != NULL) ? (ULONG)((wcslen(AppSessionId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], (AppSessionStartDateTime != NULL) ? AppSessionStartDateTime : L"", (AppSessionStartDateTime != NULL) ? (ULONG)((wcslen(AppSessionStartDateTime) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[3], &UserIdType, sizeof(UserIdType));
    EventDataDescCreate(&EventData[4], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], (SubscriptionTierType != NULL) ? SubscriptionTierType : L"", (SubscriptionTierType != NULL) ? (ULONG)((wcslen(SubscriptionTierType) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[6], (SubscriptionTier != NULL) ? SubscriptionTier : L"", (SubscriptionTier != NULL) ? (ULONG)((wcslen(SubscriptionTier) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[7], (MediaType != NULL) ? MediaType : L"", (MediaType != NULL) ? (ULONG)((wcslen(MediaType) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[8], (ProviderId != NULL) ? ProviderId : L"", (ProviderId != NULL) ? (ULONG)((wcslen(ProviderId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[9], (ProviderMediaId != NULL) ? ProviderMediaId : L"", (ProviderMediaId != NULL) ? (ULONG)((wcslen(ProviderMediaId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[10], (ProviderMediaInstanceId != NULL) ? ProviderMediaInstanceId : L"", (ProviderMediaInstanceId != NULL) ? (ULONG)((wcslen(ProviderMediaInstanceId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[11], BingId, sizeof(GUID));
    EventDataDescCreate(&EventData[12], &MediaLengthMs, sizeof(MediaLengthMs));
    EventDataDescCreate(&EventData[13], &MediaControlAction, sizeof(MediaControlAction));
    EventDataDescCreate(&EventData[14], &PlaybackSpeed, sizeof(PlaybackSpeed));
    EventDataDescCreate(&EventData[15], &MediaPositionMs, sizeof(MediaPositionMs));
    EventDataDescCreate(&EventData[16], &PlaybackDurationMs, sizeof(PlaybackDurationMs));
    EventDataDescCreate(&EventData[17], (AcquisitionType != NULL) ? AcquisitionType : L"", (AcquisitionType != NULL) ? (ULONG)((wcslen(AcquisitionType) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[18], (AcquisitionContext != NULL) ? AcquisitionContext : L"", (AcquisitionContext != NULL) ? (ULONG)((wcslen(AcquisitionContext) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[19], (AcquisitionContextType != NULL) ? AcquisitionContextType : L"", (AcquisitionContextType != NULL) ? (ULONG)((wcslen(AcquisitionContextType) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[20], (AcquisitionContextId != NULL) ? AcquisitionContextId : L"", (AcquisitionContextId != NULL) ? (ULONG)((wcslen(AcquisitionContextId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[21], &PlaybackIsStream, sizeof(PlaybackIsStream));
    EventDataDescCreate(&EventData[22], &PlaybackIsTethered, sizeof(PlaybackIsTethered));
    EventDataDescCreate(&EventData[23], (MarketplaceLocation != NULL) ? MarketplaceLocation : L"", (MarketplaceLocation != NULL) ? (ULONG)((wcslen(MarketplaceLocation) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[24], (ContentLocale != NULL) ? ContentLocale : L"", (ContentLocale != NULL) ? (ULONG)((wcslen(ContentLocale) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[25], &TimeZoneOffset, sizeof(TimeZoneOffset));
    EventDataDescCreate(&EventData[26], &ScreenState, sizeof(ScreenState));

    return EtxEventWrite(&XDKS_0301D082Events[3], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_MediaUsage, EventData);
}
#define EventEnabledMultiplayerRoundEnd() (TRUE)

// Entry point to log the event MultiplayerRoundEnd
//
__inline
ULONG
EventWriteMultiplayerRoundEnd(__in_opt PCWSTR UserId, __in LPCGUID RoundId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int MatchTypeId, __in const signed int DifficultyLevelId, __in const float TimeInSeconds, __in const signed int ExitStatusId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_MultiplayerRoundEnd 11

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_MultiplayerRoundEnd];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], RoundId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[4], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[5], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[6], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[7], &MatchTypeId, sizeof(MatchTypeId));
    EventDataDescCreate(&EventData[8], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[9], &TimeInSeconds, sizeof(TimeInSeconds));
    EventDataDescCreate(&EventData[10], &ExitStatusId, sizeof(ExitStatusId));

    return EtxEventWrite(&XDKS_0301D082Events[4], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_MultiplayerRoundEnd, EventData);
}
#define EventEnabledMultiplayerRoundStart() (TRUE)

// Entry point to log the event MultiplayerRoundStart
//
__inline
ULONG
EventWriteMultiplayerRoundStart(__in_opt PCWSTR UserId, __in LPCGUID RoundId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int MatchTypeId, __in const signed int DifficultyLevelId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_MultiplayerRoundStart 9

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_MultiplayerRoundStart];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], RoundId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[4], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[5], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[6], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[7], &MatchTypeId, sizeof(MatchTypeId));
    EventDataDescCreate(&EventData[8], &DifficultyLevelId, sizeof(DifficultyLevelId));

    return EtxEventWrite(&XDKS_0301D082Events[5], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_MultiplayerRoundStart, EventData);
}
#define EventEnabledObjectiveEnd() (TRUE)

// Entry point to log the event ObjectiveEnd
//
__inline
ULONG
EventWriteObjectiveEnd(__in_opt PCWSTR UserId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId, __in const signed int ObjectiveId, __in const signed int ExitStatusId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_ObjectiveEnd 9

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_ObjectiveEnd];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[3], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[4], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[6], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[7], &ObjectiveId, sizeof(ObjectiveId));
    EventDataDescCreate(&EventData[8], &ExitStatusId, sizeof(ExitStatusId));

    return EtxEventWrite(&XDKS_0301D082Events[6], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_ObjectiveEnd, EventData);
}
#define EventEnabledObjectiveStart() (TRUE)

// Entry point to log the event ObjectiveStart
//
__inline
ULONG
EventWriteObjectiveStart(__in_opt PCWSTR UserId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId, __in const signed int ObjectiveId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_ObjectiveStart 8

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_ObjectiveStart];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[3], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[4], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[6], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[7], &ObjectiveId, sizeof(ObjectiveId));

    return EtxEventWrite(&XDKS_0301D082Events[7], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_ObjectiveStart, EventData);
}
#define EventEnabledPageAction() (TRUE)

// Entry point to log the event PageAction
//
__inline
ULONG
EventWritePageAction(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in const signed int ActionTypeId, __in const signed int ActionInputMethodId, __in_opt PCWSTR Page, __in_opt PCWSTR TemplateId, __in_opt PCWSTR DestinationPage, __in_opt PCWSTR Content)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PageAction 9

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PageAction];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], &ActionTypeId, sizeof(ActionTypeId));
    EventDataDescCreate(&EventData[4], &ActionInputMethodId, sizeof(ActionInputMethodId));
    EventDataDescCreate(&EventData[5], (Page != NULL) ? Page : L"", (Page != NULL) ? (ULONG)((wcslen(Page) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[6], (TemplateId != NULL) ? TemplateId : L"", (TemplateId != NULL) ? (ULONG)((wcslen(TemplateId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[7], (DestinationPage != NULL) ? DestinationPage : L"", (DestinationPage != NULL) ? (ULONG)((wcslen(DestinationPage) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[8], (Content != NULL) ? Content : L"", (Content != NULL) ? (ULONG)((wcslen(Content) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));

    return EtxEventWrite(&XDKS_0301D082Events[8], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PageAction, EventData);
}
#define EventEnabledPageView() (TRUE)

// Entry point to log the event PageView
//
__inline
ULONG
EventWritePageView(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR Page, __in_opt PCWSTR RefererPage, __in const signed int PageTypeId, __in_opt PCWSTR PageTags, __in_opt PCWSTR TemplateId, __in_opt PCWSTR Content)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PageView 9

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PageView];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], (Page != NULL) ? Page : L"", (Page != NULL) ? (ULONG)((wcslen(Page) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[4], (RefererPage != NULL) ? RefererPage : L"", (RefererPage != NULL) ? (ULONG)((wcslen(RefererPage) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &PageTypeId, sizeof(PageTypeId));
    EventDataDescCreate(&EventData[6], (PageTags != NULL) ? PageTags : L"", (PageTags != NULL) ? (ULONG)((wcslen(PageTags) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[7], (TemplateId != NULL) ? TemplateId : L"", (TemplateId != NULL) ? (ULONG)((wcslen(TemplateId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[8], (Content != NULL) ? Content : L"", (Content != NULL) ? (ULONG)((wcslen(Content) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));

    return EtxEventWrite(&XDKS_0301D082Events[9], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PageView, EventData);
}
#define EventEnabledPlayerSessionEnd() (TRUE)

// Entry point to log the event PlayerSessionEnd
//
__inline
ULONG
EventWritePlayerSessionEnd(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId, __in const signed int ExitStatusId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionEnd 7

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionEnd];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[4], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[5], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[6], &ExitStatusId, sizeof(ExitStatusId));

    return EtxEventWrite(&XDKS_0301D082Events[10], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionEnd, EventData);
}
#define EventEnabledPlayerSessionPause() (TRUE)

// Entry point to log the event PlayerSessionPause
//
__inline
ULONG
EventWritePlayerSessionPause(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionPause 4

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionPause];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));

    return EtxEventWrite(&XDKS_0301D082Events[11], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionPause, EventData);
}
#define EventEnabledPlayerSessionResume() (TRUE)

// Entry point to log the event PlayerSessionResume
//
__inline
ULONG
EventWritePlayerSessionResume(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionResume 6

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionResume];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[4], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[5], &DifficultyLevelId, sizeof(DifficultyLevelId));

    return EtxEventWrite(&XDKS_0301D082Events[12], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionResume, EventData);
}
#define EventEnabledPlayerSessionStart() (TRUE)

// Entry point to log the event PlayerSessionStart
//
__inline
ULONG
EventWritePlayerSessionStart(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionStart 6

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionStart];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[4], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[5], &DifficultyLevelId, sizeof(DifficultyLevelId));

    return EtxEventWrite(&XDKS_0301D082Events[13], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PlayerSessionStart, EventData);
}
#define EventEnabledPuzzleSolved() (TRUE)

// Entry point to log the event PuzzleSolved
//
__inline
ULONG
EventWritePuzzleSolved(__in_opt PCWSTR UserId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId, __in const float TimeInSeconds)
{
#define ARGUMENT_COUNT_XDKS_0301D082_PuzzleSolved 8

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_PuzzleSolved];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[3], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[4], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[6], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[7], &TimeInSeconds, sizeof(TimeInSeconds));

    return EtxEventWrite(&XDKS_0301D082Events[14], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_PuzzleSolved, EventData);
}
#define EventEnabledSectionEnd() (TRUE)

// Entry point to log the event SectionEnd
//
__inline
ULONG
EventWriteSectionEnd(__in_opt PCWSTR UserId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId, __in const signed int ExitStatusId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_SectionEnd 8

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_SectionEnd];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[3], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[4], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[6], &DifficultyLevelId, sizeof(DifficultyLevelId));
    EventDataDescCreate(&EventData[7], &ExitStatusId, sizeof(ExitStatusId));

    return EtxEventWrite(&XDKS_0301D082Events[15], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_SectionEnd, EventData);
}
#define EventEnabledSectionStart() (TRUE)

// Entry point to log the event SectionStart
//
__inline
ULONG
EventWriteSectionStart(__in_opt PCWSTR UserId, __in const signed int SectionId, __in LPCGUID PlayerSessionId, __in_opt PCWSTR MultiplayerCorrelationId, __in const signed int GameplayModeId, __in const signed int DifficultyLevelId)
{
#define ARGUMENT_COUNT_XDKS_0301D082_SectionStart 7

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_SectionStart];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], &SectionId, sizeof(SectionId));
    EventDataDescCreate(&EventData[3], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[4], (MultiplayerCorrelationId != NULL) ? MultiplayerCorrelationId : L"", (MultiplayerCorrelationId != NULL) ? (ULONG)((wcslen(MultiplayerCorrelationId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[5], &GameplayModeId, sizeof(GameplayModeId));
    EventDataDescCreate(&EventData[6], &DifficultyLevelId, sizeof(DifficultyLevelId));

    return EtxEventWrite(&XDKS_0301D082Events[16], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_SectionStart, EventData);
}
#define EventEnabledViewOffer() (TRUE)

// Entry point to log the event ViewOffer
//
__inline
ULONG
EventWriteViewOffer(__in_opt PCWSTR UserId, __in LPCGUID PlayerSessionId, __in LPCGUID OfferGuid, __in LPCGUID ProductGuid)
{
#define ARGUMENT_COUNT_XDKS_0301D082_ViewOffer 5

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_XDKS_0301D082_ViewOffer];
    UINT8 scratch[64];

    EtxFillCommonFields_v7(&EventData[0], scratch, 64);

    EventDataDescCreate(&EventData[1], (UserId != NULL) ? UserId : L"", (UserId != NULL) ? (ULONG)((wcslen(UserId) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L""));
    EventDataDescCreate(&EventData[2], PlayerSessionId, sizeof(GUID));
    EventDataDescCreate(&EventData[3], OfferGuid, sizeof(GUID));
    EventDataDescCreate(&EventData[4], ProductGuid, sizeof(GUID));

    return EtxEventWrite(&XDKS_0301D082Events[17], &XDKS_0301D082Provider, XDKS_0301D082Handle, ARGUMENT_COUNT_XDKS_0301D082_ViewOffer, EventData);
}
#if defined(__cplusplus)
};
#endif

#pragma pack(pop)
