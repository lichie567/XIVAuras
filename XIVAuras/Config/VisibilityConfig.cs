using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class VisibilityConfig : IConfigPage
    {
        public string Name => "Visibility";

        public void DrawConfig(Vector2 size)
        {
            if (ImGui.BeginChild("##VisibilityConfig", new Vector2(size.X - 16, size.Y - 67), true))
            {
                ImGui.Text("TODO");

                ImGui.EndChild();
            }
        }
    }
}