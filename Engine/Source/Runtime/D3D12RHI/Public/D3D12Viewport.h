// Copyright Epic Games, Inc. All Rights Reserved.

/*=============================================================================
	D3D12Viewport.h: D3D viewport RHI definitions.
=============================================================================*/

#pragma once

#include "CoreMinimal.h"
#include "HAL/Runnable.h"

#define ALTERNATE_TIMESTAMP_METRIC 0

#ifndef D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN
#define D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN 1
#endif // D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN

static DXGI_FORMAT GetRenderTargetFormat(EPixelFormat PixelFormat)
{
	DXGI_FORMAT	DXFormat = (DXGI_FORMAT)GPixelFormats[PixelFormat].PlatformFormat;
	switch (DXFormat)
	{
	case DXGI_FORMAT_B8G8R8A8_TYPELESS:		return DXGI_FORMAT_B8G8R8A8_UNORM;
	case DXGI_FORMAT_BC1_TYPELESS:			return DXGI_FORMAT_BC1_UNORM;
	case DXGI_FORMAT_BC2_TYPELESS:			return DXGI_FORMAT_BC2_UNORM;
	case DXGI_FORMAT_BC3_TYPELESS:			return DXGI_FORMAT_BC3_UNORM;
	case DXGI_FORMAT_R16_TYPELESS:			return DXGI_FORMAT_R16_UNORM;
	case DXGI_FORMAT_R8G8B8A8_TYPELESS:		return DXGI_FORMAT_R8G8B8A8_UNORM;
	default: 								return DXFormat;
	}
}

#if WITH_MGPU
class FD3D12FramePacing : public FRunnable, public FD3D12AdapterChild
{
public:
	explicit FD3D12FramePacing(FD3D12Adapter* Parent);
	~FD3D12FramePacing();
	bool Init() override;
	void Stop() override;
	void Exit() override;
	uint32 Run() override;

	void PrePresentQueued(ID3D12CommandQueue* Queue);

private:
	static const uint32 MaxFrames = MAX_NUM_GPUS + 1;

	TRefCountPtr<ID3D12Fence> Fence;
	uint64 NextIndex = 0;
	uint64 CurIndex = 0;
	uint32 SleepTimes[MaxFrames];
	HANDLE Semaphore;
	bool bKeepRunning;

	float AvgFrameTimeMs;
	uint64 LastFrameTimeMs;

	// ======== Some knobs for tweaking the algorithm ========
	// How long to average the GPU time over, in seconds.
	// - Higher = Smoother when framerate is steady, less smooth when frametime drops.
	// - Lower = Quicker to smooth out after frametime drops, less smooth from incremental changes.
	const float FramePacingAvgTimePeriod = 0.25f;
	// What percentage of average GPU time to wait for on the pacing thread.
	// - Higher = More consistent pacing, potential to starve the GPU in order to maintain pacing.
	// - Lower = More allowable deviation between frame times, depending on GPU workload.
#if ALTERNATE_TIMESTAMP_METRIC
	const float FramePacingPercentage = 1.15f;
#else
	const float FramePacingPercentage = 1.05f;
#endif

	FRunnableThread* Thread;
};
#endif //WITH_MGPU


class FD3D12Viewport : public FRHIViewport, public FD3D12AdapterChild
{
public:

	FD3D12Viewport(class FD3D12Adapter* InParent, HWND InWindowHandle, uint32 InSizeX, uint32 InSizeY, bool bInIsFullscreen, EPixelFormat InPixelFormat);

	void Init();

	~FD3D12Viewport();

	void Resize(uint32 InSizeX, uint32 InSizeY, bool bInIsFullscreen, EPixelFormat PreferredPixelFormat);

	/**
	 * If the swap chain has been invalidated by DXGI, resets the swap chain to the expected state; otherwise, does nothing.
	 * Called once/frame by the game thread on all viewports.
	 * @param bIgnoreFocus - Whether the reset should happen regardless of whether the window is focused.
	 */
	void ConditionalResetSwapChain(bool bIgnoreFocus);

	/** Presents the swap chain.
	 * Returns true if Present was done by Engine.
	 */
	bool Present(bool bLockToVsync);

	// Accessors.
	FIntPoint GetSizeXY() const { return FIntPoint(SizeX, SizeY); }

	FD3D12Texture2D* GetDummyBackBuffer_RenderThread(bool bInIsSDR) const { return bInIsSDR ? SDRDummyBackBuffer_RenderThread : DummyBackBuffer_RenderThread; }

	FD3D12Texture2D* GetBackBuffer_RHIThread() const { return BackBuffer_RHIThread; }
	FD3D12Texture2D* GetSDRBackBuffer_RHIThread() const { return (PixelFormat == SDRPixelFormat) ? GetBackBuffer_RHIThread() : SDRBackBuffer_RHIThread; }

	virtual void WaitForFrameEventCompletion() override;
	virtual void IssueFrameEvent() override;

#if D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN
	IDXGISwapChain1* GetSwapChain() const { return SwapChain1; }
	IDXGISwapChain1* GetSDRSwapChain() const { return (PixelFormat == SDRPixelFormat) ? GetSwapChain() : (IDXGISwapChain1*)SDRSwapChain1; }
	virtual void* GetNativeSwapChain() const override { return GetSwapChain(); }
#endif // #if D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN

	virtual void* GetNativeBackBufferTexture() const override { return GetBackBuffer_RHIThread()->GetResource(); }
	virtual void* GetNativeBackBufferRT() const override { return GetBackBuffer_RHIThread()->GetRenderTargetView(0, 0); }

	virtual void SetCustomPresent(FRHICustomPresent* InCustomPresent) override
	{
		CustomPresent = InCustomPresent;
	}
	virtual FRHICustomPresent* GetCustomPresent() const { return CustomPresent; }

	virtual void* GetNativeWindow(void** AddParam = nullptr) const override { return (void*)WindowHandle; }

	uint32 GetNumBackBuffers() const { return NumBackBuffers; }

	inline const bool IsFullscreen() const { return bIsFullscreen; }

	FD3D12Fence& GetFence() { return Fence; }

	/** Query the swap chain's current connected output for HDR support. */
	bool CurrentOutputSupportsHDR() const;

#if WITH_MGPU
	/** Advance and get the next present GPU index */
	void AdvanceExpectedBackBufferIndex_RenderThread();
	uint32 GetNextPresentGPUIndex() const
	{
		FScopeLock Lock(&ExpectedBackBufferIndexLock);
		return BackBufferGPUIndices.IsValidIndex(ExpectedBackBufferIndex_RenderThread) ? BackBufferGPUIndices[ExpectedBackBufferIndex_RenderThread] : 0;
	}
#endif // WITH_MGPU

	void OnResumeRendering();
	void OnSuspendRendering();

private:

	/**
	 * Create the dummy back buffer textures
	 */
	FD3D12Texture2D* CreateDummyBackBufferTextures(FD3D12Adapter* InAdapter, EPixelFormat InPixelFormat, uint32 InSizeX, uint32 InSizeY, bool bInIsSDR);

	/** Presents the frame synchronizing with DWM. */
	void PresentWithVsyncDWM();

	/**
	 * Presents the swap chain checking the return result.
	 * Returns true if Present was done by Engine.
	 */
	bool PresentChecked(int32 SyncInterval);

	/**
	 * Presents the backbuffer to the viewport window.
	 * Returns the HRESULT for the call.
	 */
	HRESULT PresentInternal(int32 SyncInterval);

	void ResizeInternal();
	void FinalDestroyInternal();

	uint64 LastFlipTime;
	uint64 LastFrameComplete;
	uint64 LastCompleteTime;
	int32 SyncCounter;
	bool bSyncedLastFrame;
	HWND WindowHandle;
	uint32 MaximumFrameLatency;
	uint32 SizeX;
	uint32 SizeY;
	bool bIsFullscreen;
	bool bFullscreenLost;
	EPixelFormat PixelFormat;
	bool bIsValid;
	bool bAllowTearing;

#if D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN
	TRefCountPtr<IDXGISwapChain1> SwapChain1;

// @ATG_CHANGE : BEGIN UWP support
#if PLATFORM_WINDOWS || PLATFORM_HOLOLENS || PLATFORM_UWP
// @ATG_CHANGE : END
	bool bHDRMetaDataSet;
	DXGI_COLOR_SPACE_TYPE ColorSpace;
	TRefCountPtr<IDXGISwapChain4> SwapChain4;
#endif
	TRefCountPtr<IDXGISwapChain1> SDRSwapChain1;
#endif // D3D12_VIEWPORT_EXPOSES_SWAP_CHAIN

	TArray<TRefCountPtr<FD3D12Texture2D>> BackBuffers;
	uint32 NumBackBuffers;

	TRefCountPtr<FD3D12Texture2D> DummyBackBuffer_RenderThread; // Dummy back buffer texture which always references the current back buffer on the RHI thread
	uint32 CurrentBackBufferIndex_RHIThread;
	FD3D12Texture2D* BackBuffer_RHIThread;

#if WITH_MGPU
	int32 BackbufferMultiGPUBinding; // where INDEX_NONE cycles through the GPU, otherwise the GPU index.
	mutable FCriticalSection ExpectedBackBufferIndexLock; // Can very rarely be modified on the RHI thread as well if present is skipped
	uint32 ExpectedBackBufferIndex_RenderThread; // Expected back buffer GPU index - used and updated on RenderThread!
	TArray<uint32> BackBufferGPUIndices;
#endif // WITH_MGPU

	/** 
	 * When HDR is enabled, SDR backbuffers may be required on some architectures for game DVR or broadcasting
	 */
	TArray<TRefCountPtr<FD3D12Texture2D>> SDRBackBuffers;
	TRefCountPtr<FD3D12Texture2D> SDRDummyBackBuffer_RenderThread;
	FD3D12Texture2D* SDRBackBuffer_RHIThread;
	EPixelFormat SDRPixelFormat;

	/** A fence value used to track the GPU's progress. */
	FD3D12Fence Fence;
	uint64 LastSignaledValue;

	// Determine how deep the swapchain should be (based on AFR or not)
	void CalculateSwapChainDepth(int32 DefaultSwapChainDepth);

	FCustomPresentRHIRef CustomPresent;

	DXGI_MODE_DESC SetupDXGI_MODE_DESC() const;

#if WITH_MGPU
	FD3D12FramePacing* FramePacerRunnable;
#endif //WITH_MGPU

	// Display gamut, format, and chromacities
	// Note: Must be kept in sync with CVars and Tonemapping shaders
	enum EDisplayGamut
	{
		DG_Rec709,
		DG_DCI_P3,
		DG_Rec2020,
		DG_ACES,
		DG_ACEScg
	};

	enum EDisplayFormat
	{
		DF_sRGB,
		DF_Rec709,
		DF_ExplicitGammaMapping,
		DF_ACES1000_ST_2084,
		DF_ACES2000_ST_2084,
		DF_ACES1000_ScRGB,
		DF_ACES2000_ScRGB,
	};

	struct DisplayChromacities
	{
		float RedX, RedY;
		float GreenX, GreenY;
		float BlueX, BlueY;
		float WpX, WpY;
	};

	/** See if HDR can be enabled or not based on RHI support and current engine settings. */
	bool CheckHDRSupport();

	/** Enable HDR meta data transmission and set the necessary color space. */
	void EnableHDR();

	/** Disable HDR meta data transmission and set the necessary color space. */
	void ShutdownHDR();

// @ATG_CHANGE : BEGIN UWP support
#if PLATFORM_WINDOWS || PLATFORM_HOLOLENS || PLATFORM_UWP
// @ATG_CHANGE : END
	/** Ensure the correct color space is set on the swap chain */
	void EnsureColorSpace(EDisplayGamut DisplayGamut, EDisplayFormat OutputDevice);

	/** 
	 * Set HDR meta data. 
	 * Note: Meta data should only be provided for TVs, not monitors. 
	 * This is because the TV is doing the work to display the colors correctly.
	 */
	void SetHDRTVMode(bool bEnableHDR, EDisplayGamut DisplayGamut, float MaxOutputNits, float MinOutputNits, float MaxCLL, float MaxFALL);
#endif
};

template<>
struct TD3D12ResourceTraits<FRHIViewport>
{
	typedef FD3D12Viewport TConcreteType;
};
