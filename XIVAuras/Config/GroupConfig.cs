using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Auras;

namespace XIVAuras.Config
{
    public class GroupConfig : IConfigPage
    {
        [JsonIgnore] Vector2 _screenSize = ImGui.GetMainViewport().Size;

        public string Name => "Group";

        public Vector2 Position = new Vector2(0, 0);
        public Vector2 IconSize = new Vector2(40, 40);

        [JsonIgnore] public bool RecusiveResize = false;
        [JsonIgnore] public bool ConditionsResize = false;

        public IConfigPage GetDefault() => new GroupConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.DragFloat2("Group Position", ref this.Position);
                ImGui.NewLine();

                ImGui.Text("Resize Icons");
                ImGui.DragFloat2("Icon Size", ref this.IconSize, 1, 0, _screenSize.Y);
                if (ImGui.Button("Resize", new Vector2(60, 0)))
                {
                    if (parent is AuraGroup group)
                    {
                        group.ResizeIcons(this.IconSize, this.RecusiveResize, this.ConditionsResize);
                    }
                }

                ImGui.SameLine();
                ImGui.Checkbox("Recursive", ref this.RecusiveResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to recursively resize icons in sub-groups");
                }
                ImGui.SameLine();
                ImGui.Checkbox("Conditions", ref this.ConditionsResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to resize conditions");
                }
                

                ImGui.EndChild();
            }
        }
    }
}
