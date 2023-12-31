using ImGuiNET;

namespace Rin.Editor.Panes;

abstract class Pane {
    bool isOpened;

    public bool IsOpened => isOpened;
    public string Title { get; }
    public GuiRenderer Gui { get; init; }

    public Pane(string title) {
        Title = title;
    }

    public void Render() {
        if (IsOpened) {
            if (ImGui.Begin(Title, ref isOpened)) {
                OnRender();
                ImGui.End();
            }
        }
    }

    public void Open() {
        isOpened = true;
    }

    protected abstract void OnRender();
}
