using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class AboutPage : IConfigPage
    {
        public string Name => "About";

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##IconStyleConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Text("TODO");

                ImGui.EndChild();
            }
        }
    }
}
