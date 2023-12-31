using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Rin.Rendering;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Rin.Platform.Vulkan;

sealed class VulkanStorageBuffer : IStorageBuffer, IVulkanBuffer {
    readonly StorageBufferOptions options;

    int size;
    byte[] localBuffer;
    Allocation? allocation;
    Buffer vkBuffer;

    public DescriptorBufferInfo DescriptorBufferInfo { get; private set; }

    public VulkanStorageBuffer(StorageBufferOptions options, int size) {
        this.options = options;
        this.size = size;

        Renderer.Submit(Invalidate_RT);
    }

    public void SetData(ReadOnlySpan<byte> data) {
        data.CopyTo(localBuffer);
        Renderer.Submit(() => SetData_RT(localBuffer));
    }

    public unsafe void SetData_RT(ReadOnlySpan<byte> data) {
        var destData = new Span<byte>(allocation!.Map().ToPointer(), data.Length);
        data.CopyTo(destData);
        allocation.Unmap();
    }

    public void Resize(int newSize) {
        size = newSize;
        Renderer.Submit(Invalidate_RT);
    }

    public void Dispose() {
        Release();
    }

    void Release() {
        Renderer.SubmitDisposal(
            () => {
                VulkanAllocator.DestroyBuffer(vkBuffer, allocation!);

                allocation = null;
                localBuffer = null!;
            }
        );
    }

    unsafe void Invalidate_RT() {
        Release();
        localBuffer = new byte[size];

        var bufferCreateInfo = new BufferCreateInfo(StructureType.BufferCreateInfo) {
            Usage = BufferUsageFlags.TransferDstBit | BufferUsageFlags.StorageBufferBit, Size = (uint)size
        };

        var usage = options.GpuOnly ? MemoryUsage.GPU_Only : MemoryUsage.CPU_To_GPU;
        allocation = VulkanAllocator.AllocateBuffer(bufferCreateInfo, usage, out vkBuffer);
        DescriptorBufferInfo = new() { Buffer = vkBuffer, Range = (uint)size };
    }
}
