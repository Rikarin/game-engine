using ImGuiNET;
using Rin.Core.General;
using Rin.Editor.Panes;
using Serilog;
using System.Numerics;

namespace Rin.Editor;

sealed class GuiRenderer : IDisposable {
    readonly Application application;
    readonly Dictionary<Type, Pane> panes = new();
    readonly bool fullScreenDock = true;
    readonly string settingsPath;
    bool firstTime = true;
    bool dockSpaceOpen = true;

    public Project Project { get; private set; }

    string DefaultPath => Path.Combine(settingsPath, "Default.ini");

    public GuiRenderer(Application application, Project project) {
        this.application = application;
        Project = project;
        settingsPath = Path.Combine(project.RootDirectory, "ProjectSettings", "Editor");
        Directory.CreateDirectory(settingsPath);

        // var mainWindow = this.application.MainWindow;
        // mainWindow.Load += OnStart;
        // mainWindow.Closing += OnClosing;
        // mainWindow.Render += OnRender;

        AddPane<StatsPane>();
        AddPane<HierarchyPane>();
        AddPane<ProjectPane>();
        AddPane<ConsolePane>();
        AddPane<LightningPane>();
        AddPane<InspectorPane>();
        AddPane<ScenePane>();
        AddPane<DebugTransformViewMatrixPane>();
        AddPane<TagPane>();
        AddPane<GamePane>();

        // TODO: Load/Save of opened windows

        // This is for testing purpose only; till load/save will be implemented
        OpenPane<HierarchyPane>();
        OpenPane<InspectorPane>();
        OpenPane<ProjectPane>();
        OpenPane<ConsolePane>();
        OpenPane<LightningPane>();
        OpenPane<ScenePane>();
        OpenPane<GamePane>();
    }

    public void Dispose() {
        var mainWindow = application.MainWindow;
        // mainWindow.Load -= OnStart;
        mainWindow.Closing -= OnClosing;
        // mainWindow.Render -= OnRender;
    }

    public void OpenPane<T>() => panes[typeof(T)].Open();

    void OnClosing() {
        ImGui.SaveIniSettingsToDisk(DefaultPath);
    }

    public void OnRender(float deltaTime) {
        OnUpdate();

        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();
    }

    void AddPane<T>() where T : Pane, new() {
        panes.Add(typeof(T), new T { Gui = this });
    }

    public void OnStart() {
        // guiHandle = Application.Current.MainWindow.Handle.CreateGuiRenderer();

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable
            | ImGuiConfigFlags.ViewportsEnable
            | ImGuiConfigFlags.NavEnableKeyboard;

        ImGui.StyleColorsDark();

        // io.BackendFlags |=
        //     ImGuiBackendFlags.HasMouseCursors
        //     | ImGuiBackendFlags.HasSetMousePos
        //     // | ImGuiBackendFlags.RendererHasViewports
        // | ImGuiBackendFlags.PlatformHasViewports;

        Log.Information("GUI Renderer Started");
    }

    public void OnUpdate() {
        const ImGuiDockNodeFlags dockNodeFlags = ImGuiDockNodeFlags.None | ImGuiDockNodeFlags.PassthruCentralNode;

        // We are using the ImGuiWindowFlags_NoDocking flag to make the parent window not dockable into,
        // because it would be confusing to have two docking targets within each others.
        var windowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
        if (fullScreenDock) {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            windowFlags |= ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoNavFocus;
        }

        if (dockNodeFlags.HasFlag(ImGuiDockNodeFlags.PassthruCentralNode)) {
            windowFlags |= ImGuiWindowFlags.NoBackground;
        }

        // Important: note that we proceed even if Begin() returns false (aka window is collapsed).
        // This is because we want to keep our DockSpace() active. If a DockSpace() is inactive, 
        // all active windows docked into it will lose their parent and become undocked.
        // We cannot preserve the docking relationship between an active window and an inactive docking, otherwise 
        // any change of dockspace/settings would lead to windows being stuck in limbo and never being visible.
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("Rin Editor", ref dockSpaceOpen, windowFlags);
        ImGui.PopStyleVar();

        if (fullScreenDock) {
            ImGui.PopStyleVar(2);
        }

        var io = ImGui.GetIO();
        var style = ImGui.GetStyle();
        var minWindowSizeX = style.WindowMinSize.X;
        style.WindowMinSize.X = 370;

        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable)) {
            var dockSpaceId = ImGui.GetID("DockSpace");
            ImGui.DockSpace(dockSpaceId, Vector2.Zero, dockNodeFlags);
        }

        style.WindowMinSize.X = minWindowSizeX;
        io.WantSaveIniSettings = false; // Doesn't work or what

        // Start pane renderings
        foreach (var pane in panes.Values) {
            pane.Render();
        }

        if (firstTime) {
            firstTime = false;
            ImGui.LoadIniSettingsFromDisk(DefaultPath);
        }

        if (ImGui.BeginMainMenuBar()) {
            // if (ImGui.BeginMenu("File")) {
            //     if (ImGui.MenuItem("New", "Ctrl+N")) {
            //         opened = true;
            //     }
            //     ImGui.EndMenu();
            // }

            if (ImGui.BeginMenu("File")) {
                if (ImGui.MenuItem("Load Layout")) {
                    ImGui.LoadIniSettingsFromDisk("imgui_layout.ini");
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Assets")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Game Object")) {
                if (ImGui.MenuItem("Create Empty")) {
                    // TODO: create empty object in the current scene
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Component")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window")) {
                if (ImGui.BeginMenu("Layouts")) {
                    if (ImGui.MenuItem("Basic")) {
                        ImGui.LoadIniSettingsFromDisk(Path.Combine(settingsPath, "Layouts", "Basic.ini"));
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();
                if (ImGui.BeginMenu("General")) {
                    if (ImGui.MenuItem("Scene")) {
                        OpenPane<ScenePane>();
                    }

                    if (ImGui.MenuItem("Game")) {
                        OpenPane<GamePane>();
                    }

                    if (ImGui.MenuItem("Inspector", "Ctrl+I")) {
                        OpenPane<InspectorPane>();
                    }

                    if (ImGui.MenuItem("Hierarchy")) {
                        OpenPane<HierarchyPane>();
                    }

                    if (ImGui.MenuItem("Project")) {
                        OpenPane<ProjectPane>();
                    }

                    if (ImGui.MenuItem("Console")) {
                        OpenPane<ConsolePane>();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Debug")) {
                    if (ImGui.MenuItem("Debug Transform View Matrix")) {
                        OpenPane<DebugTransformViewMatrixPane>();
                    }

                    if (ImGui.MenuItem("Stats")) {
                        OpenPane<StatsPane>();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help")) {
                if (ImGui.MenuItem("Demo Window")) { }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        // ImGui.ShowDemoWindow();
        ImGui.End();
    }
}
