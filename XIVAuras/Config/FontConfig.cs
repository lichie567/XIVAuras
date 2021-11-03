using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class FontConfig : IConfigPage
    {
        public string Name => "Fonts";

        public void DrawConfig(Vector2 size)
        {
            if (ImGui.BeginChild("##IconStyleConfig", new Vector2(size.X - 16, size.Y - 67), true))
            {
                ImGui.Text("TODO");

                ImGui.EndChild();
            }
        }
    }
}
