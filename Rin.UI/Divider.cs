using ImGuiNET;

namespace Rin.UI;

public class Divider : View {
    public override void Render() {
        ImGui.Separator();
        base.Render();
    }
}