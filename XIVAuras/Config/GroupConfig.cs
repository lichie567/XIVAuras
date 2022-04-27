using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Auras;

namespace XIVAuras.Config
{
    public class GroupConfig : IConfigPage
    {
        [JsonIgnore] private static Vector2 _screenSize = ImGui.GetMainViewport().Size;

        public string Name => "Group";

        public Vector2 Position = new Vector2(0, 0);

        [JsonIgnore] private Vector2 _iconSize = new Vector2(40, 40);
        [JsonIgnore] private Vector2 _screenSize1 = _screenSize;
        [JsonIgnore] private Vector2 _screenSize2 = _screenSize;
        [JsonIgnore] private bool _recusiveResize = false;
        [JsonIgnore] private bool _recusiveScale = false;
        [JsonIgnore] private bool _conditionsResize = false;

        public IConfigPage GetDefault() => new GroupConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.DragFloat2("Group Position", ref this.Position);

                ImGui.NewLine();
                ImGui.Text("Resize Icons");
                ImGui.DragFloat2("Icon Size##Size", ref _iconSize, 1, 0, _screenSize.Y);
                if (ImGui.Button("Resize", new Vector2(60, 0)))
                {
                    if (parent is AuraGroup group)
                    {
                        group.ResizeIcons(_iconSize, _recusiveResize, _conditionsResize);
                    }
                }

                ImGui.SameLine();
                ImGui.Checkbox("Recursive##Size", ref _recusiveResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to recursively resize icons in sub-groups");
                }
                ImGui.SameLine();
                ImGui.Checkbox("Conditions##Size", ref _conditionsResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to resize conditions");
                }

                ImGui.NewLine();
                ImGui.Text("Scale Resolution");
                ImGui.DragFloat2("Start Resolution", ref _screenSize1, 1, 0, _screenSize.Y);
                ImGui.DragFloat2("Target Resolution", ref _screenSize2, 1, 0, _screenSize.Y);
                if (ImGui.Button("Scale", new Vector2(60, 0)))
                {
                    if (parent is AuraGroup group)
                    {
                        group.ScaleResolution(_screenSize2 / _screenSize1, _recusiveScale);
                    }
                }

                ImGui.SameLine();
                ImGui.Checkbox("Recursive##Scale", ref _recusiveScale);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to recursively scale sub-groups");
                }
                

                ImGui.EndChild();
            }
        }
    }
}
