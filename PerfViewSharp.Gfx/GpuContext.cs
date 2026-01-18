using SDL3;
using System.Runtime.InteropServices;

namespace PerfViewSharp.Gfx;

public class GpuContext : IDisposable
{
    private bool _disposed;
    private IntPtr _device;
    private IntPtr _whiteTexture;

    public IntPtr Device => _device;

    public GpuContext()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            throw new Exception($"Failed to initialize SDL3: {SDL.GetError()}");
        }

        var formats = SDL.GPUShaderFormat.MSL | SDL.GPUShaderFormat.SPIRV;
        _device = SDL.CreateGPUDevice(formats, true, null);
        
        if (_device == IntPtr.Zero)
        {
            throw new Exception($"Failed to create SDL GPU Device: {SDL.GetError()}");
        }

        CreateWhiteTexture();
    }

    private unsafe void CreateWhiteTexture()
    {
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Type = SDL.GPUTextureType.TextureType2D,
            Format = SDL.GPUTextureFormat.B8G8R8A8Unorm,
            Width = 1, Height = 1, LayerCountOrDepth = 1, NumLevels = 1,
            Usage = SDL.GPUTextureUsageFlags.Sampler | (SDL.GPUTextureUsageFlags)512
        };
        _whiteTexture = SDL.CreateGPUTexture(_device, createInfo);

        IntPtr cmdbuf = SDL.AcquireGPUCommandBuffer(_device);
        var tbInfo = new SDL.GPUTransferBufferCreateInfo { Usage = SDL.GPUTransferBufferUsage.Upload, Size = 4 };
        IntPtr uploadBuffer = SDL.CreateGPUTransferBuffer(_device, tbInfo);
        
        byte* mapped = (byte*)SDL.MapGPUTransferBuffer(_device, uploadBuffer, true);
        mapped[0] = 255; mapped[1] = 255; mapped[2] = 255; mapped[3] = 255;
        SDL.UnmapGPUTransferBuffer(_device, uploadBuffer);

        IntPtr copyPass = SDL.BeginGPUCopyPass(cmdbuf);
        var src = new SDL.GPUTextureTransferInfo { TransferBuffer = uploadBuffer, Offset = 0 };
        var dst = new SDL.GPUTextureRegion { Texture = _whiteTexture, W = 1, H = 1, D = 1 };
        
        // 传递变量本身，不加 (IntPtr)(&)
        SDL.UploadToGPUTexture(copyPass, src, dst, false);
        SDL.EndGPUCopyPass(copyPass);

        SDL.SubmitGPUCommandBuffer(cmdbuf);
        SDL.ReleaseGPUTransferBuffer(_device, uploadBuffer);
    }

    public IntPtr CreateTargetTexture(uint width, uint height)
    {
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Type = SDL.GPUTextureType.TextureType2D, 
            Format = SDL.GPUTextureFormat.B8G8R8A8Unorm, 
            Width = width, Height = height, LayerCountOrDepth = 1, NumLevels = 1,
            Usage = SDL.GPUTextureUsageFlags.ColorTarget | (SDL.GPUTextureUsageFlags)256
        };
        return SDL.CreateGPUTexture(_device, createInfo);
    }

    public unsafe void DrawBlocks(IntPtr target, IEnumerable<SDL.FRect> rects, float r, float g, float b, uint width, uint height, IntPtr destination)
    {
        IntPtr cmdbuf = SDL.AcquireGPUCommandBuffer(_device);
        if (cmdbuf == IntPtr.Zero) return;

        var colorTarget = new SDL.GPUColorTargetInfo
        {
            Texture = target,
            ClearColor = new SDL.FColor { R = 0.05f, G = 0.05f, B = 0.15f, A = 1.0f },
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };
        
        // BeginGPURenderPass 在这个库里很奇怪，又是 IntPtr。我会试着兼容它。
        IntPtr renderPass = SDL.BeginGPURenderPass(cmdbuf, (IntPtr)(&colorTarget), 1, IntPtr.Zero);
        SDL.EndGPURenderPass(renderPass);

        uint size = width * height * 4;
        var tbInfo = new SDL.GPUTransferBufferCreateInfo { Usage = SDL.GPUTransferBufferUsage.Download, Size = size };
        IntPtr transferBuffer = SDL.CreateGPUTransferBuffer(_device, tbInfo);

        IntPtr copyPass = SDL.BeginGPUCopyPass(cmdbuf);
        var downloadRegion = new SDL.GPUTextureRegion { Texture = target, W = width, H = height, D = 1 };
        var copyInfo = new SDL.GPUTextureTransferInfo { TransferBuffer = transferBuffer };
        
        // 这里不加取地址
        SDL.DownloadFromGPUTexture(copyPass, downloadRegion, copyInfo);
        SDL.EndGPUCopyPass(copyPass);

        IntPtr fence = SDL.SubmitGPUCommandBufferAndAcquireFence(cmdbuf);
        SDL.WaitForGPUFences(_device, true, new[] { fence }, 1);
        SDL.ReleaseGPUFence(_device, fence);

        IntPtr mapped = SDL.MapGPUTransferBuffer(_device, transferBuffer, false);
        if (mapped != IntPtr.Zero)
        {
            Buffer.MemoryCopy((void*)mapped, (void*)destination, size, size);
            SDL.UnmapGPUTransferBuffer(_device, transferBuffer);
        }
        SDL.ReleaseGPUTransferBuffer(_device, transferBuffer);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_whiteTexture != IntPtr.Zero) SDL.ReleaseGPUTexture(_device, _whiteTexture);
            if (_device != IntPtr.Zero) SDL.DestroyGPUDevice(_device);
            SDL.Quit();
            _disposed = true;
        }
    }
}