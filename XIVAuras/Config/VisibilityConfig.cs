using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class VisibilityConfig : IConfigPage
    {
        public string Name => "Visibility";

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##VisibilityConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Text("TODO");

                ImGui.EndChild();
            }
        }
    }
}