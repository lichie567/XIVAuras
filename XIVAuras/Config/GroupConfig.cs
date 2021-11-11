using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class GroupConfig : IConfigPage
    {
        public string Name => "Group";

        public Vector2 Position = new Vector2(0, 0);

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.DragFloat2("Group Position", ref this.Position);
                ImGui.EndChild();
            }
        }
    }
}
