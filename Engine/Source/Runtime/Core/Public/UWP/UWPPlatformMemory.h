// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.


/*=============================================================================================
UWPPlatformMemory.h: UWP platform memory functions
==============================================================================================*/

#pragma once
#include "GenericPlatform/GenericPlatformMemory.h"
#include "UWP/UWPSystemIncludes.h"

/**
*	UWP implementation of the FGenericPlatformMemoryStats.
*	At this moment it's just the same as the FGenericPlatformMemoryStats.
*	Can be extended as shown in the following example.
*/
struct FPlatformMemoryStats : public FGenericPlatformMemoryStats
{
	/** Default constructor, clears all variables. */
	FPlatformMemoryStats()
		: FGenericPlatformMemoryStats()
		, UWPSpecificMemoryStat(0)
	{}

	/** Memory stat specific only for UWP. */
	SIZE_T UWPSpecificMemoryStat;
};

/**
* UWP implementation of the memory OS functions
**/
struct CORE_API FUWPPlatformMemory : public FGenericPlatformMemory
{
	enum EMemoryCounterRegion
	{
		MCR_Invalid, // not memory
		MCR_Physical, // main system memory
		MCR_GPU, // memory directly a GPU (graphics card, etc)
		MCR_GPUSystem, // system memory directly accessible by a GPU
		MCR_TexturePool, // presized texture pools
		MCR_StreamingPool, // amount of texture pool available for streaming.
		MCR_UsedStreamingPool, // amount of texture pool used for streaming.
		MCR_GPUDefragPool, // presized pool of memory that can be defragmented.
		MCR_SamplePlatformSpecifcMemoryRegion,
		MCR_PhysicalLLM, // total physical memory displayed in the LLM stats (on consoles CPU + GPU)
		MCR_MAX
	};

	/**
	* UWP representation of a shared memory region
	*/
	struct FUWPSharedMemoryRegion : public FSharedMemoryRegion
	{
		/** Returns the handle to file mapping object. */
		HANDLE GetMapping() const { return Mapping; }

		FUWPSharedMemoryRegion(const FString& InName, uint32 InAccessMode, void* InAddress, SIZE_T InSize, HANDLE InMapping)
			: FSharedMemoryRegion(InName, InAccessMode, InAddress, InSize)
			, Mapping(InMapping)
		{}

	protected:

		/** Handle of a file mapping object */
		HANDLE				Mapping;
	};

	//~ Begin FGenericPlatformMemory Interface
	static void Init();
	static bool SupportBackupMemoryPool()
	{
		return true;
	}
	static class FMalloc* BaseAllocator();
	static FPlatformMemoryStats GetStats();
	static void GetStatsForMallocProfiler(FGenericMemoryStats& out_Stats);
	static const FPlatformMemoryConstants& GetConstants();
	static void* BinnedAllocFromOS(SIZE_T Size);
	static void BinnedFreeToOS(void* Ptr, SIZE_T Size);

	class FPlatformVirtualMemoryBlock : public FBasicVirtualMemoryBlock
	{
	public:

		FPlatformVirtualMemoryBlock()
		{
		}

		FPlatformVirtualMemoryBlock(void* InPtr, uint32 InVMSizeDivVirtualSizeAlignment)
			: FBasicVirtualMemoryBlock(InPtr, InVMSizeDivVirtualSizeAlignment)
		{
		}
		FPlatformVirtualMemoryBlock(const FPlatformVirtualMemoryBlock& Other) = default;
		FPlatformVirtualMemoryBlock& operator=(const FPlatformVirtualMemoryBlock& Other) = default;

		void Commit(size_t InOffset, size_t InSize);
		void Decommit(size_t InOffset, size_t InSize);
		void FreeVirtual();

		FORCEINLINE void CommitByPtr(void* InPtr, size_t InSize)
		{
			Commit(size_t(((uint8*)InPtr) - ((uint8*)Ptr)), InSize);
		}

		FORCEINLINE void DecommitByPtr(void* InPtr, size_t InSize)
		{
			Decommit(size_t(((uint8*)InPtr) - ((uint8*)Ptr)), InSize);
		}

		FORCEINLINE void Commit()
		{
			Commit(0, GetActualSize());
		}

		FORCEINLINE void Decommit()
		{
			Decommit(0, GetActualSize());
		}

		FORCEINLINE size_t GetActualSize() const
		{
			return VMSizeDivVirtualSizeAlignment * GetVirtualSizeAlignment();
		}

		static FPlatformVirtualMemoryBlock AllocateVirtual(size_t Size, size_t InAlignment = FPlatformVirtualMemoryBlock::GetVirtualSizeAlignment());
		static size_t GetCommitAlignment();
		static size_t GetVirtualSizeAlignment();
	};

	static FSharedMemoryRegion* MapNamedSharedMemoryRegion(const FString& InName, bool bCreate, uint32 AccessMode, SIZE_T Size);
	static bool UnmapNamedSharedMemoryRegion(FSharedMemoryRegion * MemoryRegion);
protected:
	friend struct FGenericStatsUpdater;

	static void InternalUpdateStats(const FPlatformMemoryStats& MemoryStats);
	//~ End FGenericPlatformMemory Interface
};

typedef FUWPPlatformMemory FPlatformMemory;
