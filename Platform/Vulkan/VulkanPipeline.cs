using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using PrimitiveTopology = Rin.Platform.Rendering.PrimitiveTopology;

namespace Rin.Platform.Vulkan;

public sealed class VulkanPipeline : IPipeline, IDisposable {
    Pipeline pipeline;
    PipelineLayout layout;
    PipelineCache cache;

    public bool IsDynamicLineWidth =>
        Options.Topology is PrimitiveTopology.Lines or PrimitiveTopology.LineStrip || Options.WireFrame;

    public PipelineOptions Options { get; }

    public VulkanPipeline(PipelineOptions options) {
        Options = options;
        Invalidate();

        // TODO: Register Shader Dependency
    }

    public unsafe void Dispose() {
        Renderer.SubmitDisposal(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                vk.DestroyPipeline(device, pipeline, null);
                vk.DestroyPipelineCache(device, cache, null);
                vk.DestroyPipelineLayout(device, layout, null);
            }
        );
    }

    unsafe void Invalidate() {
        Renderer.Submit(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                var shader = Options.Shader as VulkanShader;
                var framebuffer = Options.TargetFramebuffer as VulkanFramebuffer;

                var descriptorSetLayouts = shader.DescriptorSetLayouts;
                var pushConstantRanges = shader.PushConstantRanges;

                // TODO: push constants

                fixed (DescriptorSetLayout* descriptorSetLayoutPtr = descriptorSetLayouts.Values.ToArray()) {
                    var pipelineLayoutCreateInfo =
                        new PipelineLayoutCreateInfo(StructureType.PipelineLayoutCreateInfo) {
                            SetLayoutCount = (uint)descriptorSetLayouts.Count,
                            PSetLayouts = descriptorSetLayoutPtr,
                            // TODO
                            PushConstantRangeCount = 0,
                            PPushConstantRanges = null
                        };

                    vk.CreatePipelineLayout(device, pipelineLayoutCreateInfo, null, out var pipelineLayout);
                    layout = pipelineLayout;
                }

                // Input Assembly
                var inputAssemblyState =
                    new PipelineInputAssemblyStateCreateInfo(StructureType.PipelineInputAssemblyStateCreateInfo) {
                        Topology = Options.Topology.ToVulkan()
                    };

                // Rasterization
                var rasterizationState =
                    new PipelineRasterizationStateCreateInfo(StructureType.PipelineRasterizationStateCreateInfo) {
                        PolygonMode = Options.WireFrame ? PolygonMode.Line : PolygonMode.Fill,
                        CullMode = Options.BackfaceCulling ? CullModeFlags.BackBit : CullModeFlags.None,
                        FrontFace = FrontFace.Clockwise,
                        LineWidth = Options.LineWidth
                    };

                // Blend State
                var colorAttachmentCount =
                    framebuffer!.Options.IsSwapChainTarget ? 1 : framebuffer.ColorAttachmentCount;
                var blendAttachmentStates = new List<PipelineColorBlendAttachmentState>();

                if (framebuffer.Options.IsSwapChainTarget) {
                    blendAttachmentStates.Add(
                        new() {
                            ColorWriteMask = (ColorComponentFlags)0xF,
                            BlendEnable = true,
                            SrcColorBlendFactor = BlendFactor.SrcAlpha,
                            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                            ColorBlendOp = BlendOp.Add,
                            AlphaBlendOp = BlendOp.Add,
                            SrcAlphaBlendFactor = BlendFactor.One,
                            DstAlphaBlendFactor = BlendFactor.Zero
                        }
                    );
                } else {
                    throw new NotImplementedException();
                }

                using var blendAttachmentStatesMemoryHandle =
                    new Memory<PipelineColorBlendAttachmentState>(blendAttachmentStates.ToArray()).Pin();
                var colorBlendState =
                    new PipelineColorBlendStateCreateInfo(StructureType.PipelineColorBlendStateCreateInfo) {
                        PAttachments = (PipelineColorBlendAttachmentState*)blendAttachmentStatesMemoryHandle.Pointer,
                        AttachmentCount = (uint)blendAttachmentStates.Count
                    };

                // Viewport
                var viewportState = new PipelineViewportStateCreateInfo(StructureType.PipelineViewportStateCreateInfo) {
                    ViewportCount = 1, ScissorCount = 1
                };

                // Dynamic states
                var enabledDynamicStates = new List<DynamicState> { DynamicState.Viewport, DynamicState.Scissor };
                if (IsDynamicLineWidth) {
                    enabledDynamicStates.Add(DynamicState.LineWidth);
                }

                using var enabledDynamicStatesMemoryHandle =
                    new Memory<DynamicState>(enabledDynamicStates.ToArray()).Pin();
                var dynamicState = new PipelineDynamicStateCreateInfo(StructureType.PipelineDynamicStateCreateInfo) {
                    PDynamicStates = (DynamicState*)enabledDynamicStatesMemoryHandle.Pointer,
                    DynamicStateCount = (uint)enabledDynamicStates.Count
                };

                // Depth Stencil
                var depthStencilState =
                    new PipelineDepthStencilStateCreateInfo(StructureType.PipelineDepthStencilStateCreateInfo) {
                        DepthTestEnable = Options.DepthTest,
                        DepthWriteEnable = Options.DepthWrite,
                        DepthCompareOp = Options.DepthOperator.ToVulkan(),
                        Back = new() {
                            FailOp = StencilOp.Keep, PassOp = StencilOp.Keep, CompareMask = (uint)CompareOp.Always
                        },
                        Front = new() {
                            FailOp = StencilOp.Keep, PassOp = StencilOp.Keep, CompareMask = (uint)CompareOp.Always
                        }
                    };

                // Multi sample
                var multisampleState =
                    new PipelineMultisampleStateCreateInfo(StructureType.PipelineMultisampleStateCreateInfo) {
                        RasterizationSamples = SampleCountFlags.Count1Bit
                    };

                // Vertex Input Descriptor
                var vertexInputBindings = new List<VertexInputBindingDescription> {
                    new() { Binding = 0, Stride = (uint)Options.Layout.Stride, InputRate = VertexInputRate.Vertex }
                };

                if (Options.InstanceLayout?.HasElements == true) {
                    vertexInputBindings.Add(
                        new() {
                            Binding = 1, Stride = (uint)Options.InstanceLayout.Stride, InputRate = VertexInputRate.Instance
                        }
                    );
                }

                if (Options.BoneInfluenceLayout?.HasElements == true) {
                    vertexInputBindings.Add(
                        new() {
                            Binding = 2, Stride = (uint)Options.BoneInfluenceLayout.Stride, InputRate = VertexInputRate.Vertex
                        }
                    );
                }

                // Input attribute bindings describe shader attribute locations and memory layouts
                var vertexInputAttributes = new List<VertexInputAttributeDescription>();

                var binding = 0;
                var location = 0;
                foreach (var layout in new[] { Options.Layout, Options.InstanceLayout, Options.BoneInfluenceLayout }) {
                    foreach (var element in layout?.Elements ?? ArraySegment<VertexBufferElement>.Empty) {
                        vertexInputAttributes.Add(
                            new() {
                                Binding = (uint)binding,
                                Location = (uint)location,
                                Format = element.Type.ToVulkan(),
                                Offset = (uint)element.Offset
                            }
                        );
                        location++;
                    }

                    binding++;
                }

                using var vertexInputBindingsMemoryHandle =
                    new Memory<VertexInputBindingDescription>(vertexInputBindings.ToArray()).Pin();

                using var vertexInputAttributesMemoryHandle =
                    new Memory<VertexInputAttributeDescription>(vertexInputAttributes.ToArray()).Pin();

                var vertexInputState =
                    new PipelineVertexInputStateCreateInfo(StructureType.PipelineVertexInputStateCreateInfo) {
                        VertexBindingDescriptionCount = (uint)vertexInputBindings.Count,
                        VertexAttributeDescriptionCount = (uint)vertexInputAttributes.Count,
                        PVertexBindingDescriptions =
                            (VertexInputBindingDescription*)vertexInputBindingsMemoryHandle.Pointer,
                        PVertexAttributeDescriptions =
                            (VertexInputAttributeDescription*)vertexInputAttributesMemoryHandle.Pointer
                    };

                var shaderStages =
                    new Memory<PipelineShaderStageCreateInfo>(shader.PipelineShaderStageCreateInfos.ToArray());
                using var shaderStagesMemoryHandle = shaderStages.Pin();

                var pipelineCreateInfo = new GraphicsPipelineCreateInfo(StructureType.GraphicsPipelineCreateInfo) {
                    Layout = layout,
                    RenderPass = framebuffer.RenderPass,
                    StageCount = (uint)shaderStages.Length,
                    PStages = (PipelineShaderStageCreateInfo*)shaderStagesMemoryHandle.Pointer,
                    PInputAssemblyState = &inputAssemblyState,
                    PRasterizationState = &rasterizationState,
                    PViewportState = &viewportState,
                    PMultisampleState = &multisampleState,
                    PDynamicState = &dynamicState,
                    PColorBlendState = &colorBlendState,
                    PDepthStencilState = &depthStencilState,
                    PVertexInputState = &vertexInputState
                };

                // Create cache
                var pipelineCacheCreateInfo = new PipelineCacheCreateInfo(StructureType.PipelineCacheCreateInfo);
                vk.CreatePipelineCache(device, pipelineCacheCreateInfo, null, out var pipelineCache);
                cache = pipelineCache;

                // Create rendering pipeline
                var pipelines = new[] { pipelineCreateInfo };
                var outPipeline = new Pipeline[1];
                vk.CreateGraphicsPipelines(device, cache, pipelines.AsSpan(), null, outPipeline);
                pipeline = outPipeline[0];
            }
        );
    }
}
