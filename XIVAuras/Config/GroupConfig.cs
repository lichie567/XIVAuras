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
        [JsonIgnore] private bool _conditionsResize = false;
        [JsonIgnore] private bool _scaleByHeight = false;

        public IConfigPage GetDefault() => new GroupConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.DragFloat2("Group Position", ref this.Position);

                ImGui.NewLine();
                ImGui.Text("Resize Icons");
                ImGui.DragFloat2("Icon Size##Size", ref _iconSize, 1, 0, _screenSize.Y);
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
                
                ImGui.SameLine();
                float padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                if (ImGui.Button("Resize", new Vector2(60, 0)))
                {
                    if (parent is AuraGroup g)
                    {
                        g.ResizeIcons(_iconSize, _recusiveResize, _conditionsResize);
                    }
                }

                if (parent is AuraGroup group)
                {
                    ImGui.NewLine();
                    ImGui.Text("Scale Resolution (Size & Position)");
                    ImGui.DragFloat2("Original Resolution", ref _screenSize1, 1, 0, _screenSize.Y);
                    ImGui.DragFloat2("Target Resolution", ref _screenSize2, 1, 0, _screenSize.Y);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Check to recursively scale sub-groups");
                    }

                    ImGui.Checkbox("Scale by Height##Scale", ref _scaleByHeight);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Check to scale by screen height, use this for ultra-wide aspect ratios.");
                    }

                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    if (ImGui.Button("Scale", new Vector2(60, 0)))
                    {
                        Vector2 start = _scaleByHeight ? _screenSize1.Y * Vector2.One : _screenSize1;
                        Vector2 target = _scaleByHeight ? _screenSize2.Y * Vector2.One : _screenSize2;
                        group.ScaleResolution(target / start);
                    }
                }
                

                ImGui.EndChild();
            }
        }
    }
}
