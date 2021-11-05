using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public enum JobTypes
    {
        All,
        Tanks,
        Casters,
        Melee,
        Ranged,
        DoW,
        DoM,
        Crafters,
        DoH,
        DoL,
        Custom
    }

    public class VisibilityConfig : IConfigPage
    {
        public string Name => "Visibility";

        public bool IsVisible()
        {
            return true;
        }

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