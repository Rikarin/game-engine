using Rin.Core.Abstractions;
using Rin.Platform.Internal;
using Rin.Platform.Rendering;
using Rin.Platform.Vulkan;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Key = Silk.NET.Input.Key;
using MouseButton = Silk.NET.Input.MouseButton;
using WindowOptions = Rin.Core.Abstractions.WindowOptions;

[assembly: InternalsVisibleTo("Rin.Core")]
[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Platform.Silk;

sealed class SilkWindow : IInternalWindow {
    internal static SilkWindow MainWindow;
    internal IWindow silkWindow = null!;
    internal IInputContext input;

    // TODO: this needs to be fixed
    // I think user can press multiple keys during one frame
    Key? keyDown;
    Key? keyUp;
    MouseButton? mouseButtonDown;
    MouseButton? mouseButtonUp;
    Vector2 mouseAxis = Vector2.Zero;

    readonly bool[] keyPressed = new bool[(int)Key.Menu + 1];
    readonly bool[] mouseButtonPressed = new bool[(int)MouseButton.Button12 + 1];

    public RendererContext RendererContext { get; private set; }
    public ISwapchain Swapchain { get; private set; }
    public Vector2 MousePosition { get; private set; } = Vector2.Zero;

    internal SilkWindow(WindowOptions options) {
        MainWindow = this;
        Initialize(options);
    }

    public IInternalGuiRenderer CreateGuiRenderer() => throw new NotImplementedException();
    // new SilkImGuiRenderer(new(Gl, silkWindow, input));

    public void Run() {
        Load?.Invoke();
        silkWindow.Run();
    }

    public bool GetKey(Core.Abstractions.Key key) => keyPressed[(int)key];
    public bool GetKeyDown(Core.Abstractions.Key key) => keyDown.HasValue && (int)keyDown.Value == (int)key;
    public bool GetKeyUp(Core.Abstractions.Key key) => keyUp.HasValue && (int)keyUp.Value == (int)key;

    public Vector2 GetMouseAxis() => mouseAxis;

    public bool GetMouseButtonDown(Core.Abstractions.MouseButton mouseButton) =>
        mouseButtonDown.HasValue && (int)mouseButtonDown.Value == (int)mouseButton;

    public bool GetMouseButtonUp(Core.Abstractions.MouseButton mouseButton) =>
        mouseButtonUp.HasValue && (int)mouseButtonUp.Value == (int)mouseButton;

    void Initialize(WindowOptions options) {
        silkWindow = Window.Create(
            global::Silk.NET.Windowing.WindowOptions.DefaultVulkan with {
                Title = options.Title, Size = new(options.Size.Width, options.Size.Height), VSync = options.VSync
            }
        );

        silkWindow.Load += OnLoad;
        silkWindow.Render += OnRender;
        silkWindow.Closing += OnClosing;
        // TODO
        // silkWindow.FramebufferResize += vector2D => Gl.Viewport(vector2D);


        silkWindow.Initialize();
        RendererContext = ObjectFactory.CreateRendererContext();
        var swapChain = new VulkanSwapChain();
        swapChain.InitializeSurface(silkWindow);

        var width = 800;
        var height = 600;

        swapChain.Create(ref width, ref height, false);
        Swapchain = swapChain;

        //     // swapChain.Dispose();
        //     swapChain.BeginFrame();
        //     swapChain.Present();
    }

    void OnClosing() {
        Closing?.Invoke();
    }

    void OnRender(double deltaTime) {
        Render?.Invoke((float)deltaTime);
        keyDown = null;
        keyUp = null;
        mouseAxis = Vector2.Zero;
        mouseButtonDown = null;
        mouseButtonUp = null;
    }

    void OnLoad() {
        input = silkWindow.CreateInput();

        foreach (var keyboard in input.Keyboards) {
            keyboard.KeyDown += KeyDown;
            keyboard.KeyUp += KeyUp;
        }

        foreach (var mouse in input.Mice) {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
            // click, scroll, double click??
        }

        Log.Information("Vulkan Context initialized");
    }

    void OnMouseMove(IMouse arg1, Vector2 arg2) {
        mouseAxis = arg2 - MousePosition;
        MousePosition = arg2;
    }

    void OnMouseUp(IMouse arg1, MouseButton arg2) {
        mouseButtonPressed[(int)arg2] = false;
        mouseButtonUp = arg2;
    }

    void OnMouseDown(IMouse arg1, MouseButton arg2) {
        mouseButtonPressed[(int)arg2] = true;
        mouseButtonDown = arg2;
    }

    void KeyDown(IKeyboard keyboard, Key key, int index) {
        keyPressed[(int)key] = true;
        keyDown = key;

        if (key == Key.Escape) {
            silkWindow.Close();
        }
    }

    void KeyUp(IKeyboard keyboard, Key key, int index) {
        keyPressed[(int)key] = false;
        keyUp = key;
    }

    public event Action? Load;
    public event Action? Closing;
    public event Action<float>? Render;
}
