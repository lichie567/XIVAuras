using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class BarStyleConfig : IConfigPage
    {
        public string Name => "Style";

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##BarStyleConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Text("Coming Soon");

                ImGui.EndChild();
            }
        }
    }
}