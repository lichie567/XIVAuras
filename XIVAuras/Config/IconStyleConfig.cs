using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class IconStyleConfig : IConfigPage
    {
        [JsonIgnore]
        public string Name => "Icon";

        [JsonIgnore] private string _labelInput = string.Empty;
        [JsonIgnore] private string _iconSearchInput = string.Empty;
        [JsonIgnore] private List<TriggerData> _iconSearchResults = new List<TriggerData>();

        public Vector2 Position = Vector2.Zero;
        public Vector2 Size = new Vector2(40, 40);
        public bool ShowBorder = true;
        public int BorderThickness = 1;
        public ConfigColor BorderColor = new ConfigColor(0, 0, 0, 1);
        public bool ShowProgressSwipe = true;
        public float ProgressSwipeOpacity = 0.6f;
        public bool InvertSwipe = false;
        public bool ShowSwipeLines = false;
        public ConfigColor ProgressLineColor = new ConfigColor(1, 1, 1, 1);
        public int ProgressLineThickness = 2;
        public bool DesaturateIcon = false;
        public float Opacity = 1f;

        public int IconOption = 0;
        public ushort CustomIcon = 0;
        public bool CropIcon = false;

        public IConfigPage GetDefault() => new IconStyleConfig();

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##IconStyleConfig", new Vector2(size.X, size.Y), true))
            {
                float height = 50;
                if (this.IconOption > 0 && this.CustomIcon > 0)
                {
                    Vector2 iconPos = ImGui.GetWindowPos() + new Vector2(padX, padX);
                    Vector2 iconSize = new Vector2(height, height);
                    this.DrawIconPreview(iconPos, iconSize, this.CustomIcon, this.CropIcon, this.DesaturateIcon, false);
                    ImGui.GetWindowDrawList().AddRect(
                        iconPos,
                        iconPos + iconSize,
                        ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.Border]));

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + height + padX);
                }

                ImGui.RadioButton("Automatic Icon", ref this.IconOption, 0);
                ImGui.SameLine();
                ImGui.RadioButton("Custom Icon", ref this.IconOption, 1);
                
                if (this.IconOption == 1)
                {
                    float width = ImGui.CalcItemWidth();
                    if (this.CustomIcon > 0)
                    {
                        width -= height + padX;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + height + padX);
                    }

                    ImGui.PushItemWidth(width);
                    if (ImGui.InputTextWithHint("Search", "Search Icons by Name or ID", ref _iconSearchInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        _iconSearchResults.Clear();
                        if (ushort.TryParse(_iconSearchInput, out ushort iconId))
                        {
                            _iconSearchResults.Add(new TriggerData("", 0, iconId));
                        }
                        else if (!string.IsNullOrEmpty(_iconSearchInput))
                        {
                            _iconSearchResults.AddRange(SpellHelpers.FindActionEntries(_iconSearchInput));
                            _iconSearchResults.AddRange(SpellHelpers.FindStatusEntries(_iconSearchInput));
                        }
                    }
                    ImGui.PopItemWidth();

                    if (_iconSearchResults.Any() && ImGui.BeginChild("##IconPicker", new Vector2(size.X - padX * 2, 60), true))
                    {
                        List<ushort> icons = _iconSearchResults.Select(t => t.Icon).Distinct().ToList();
                        for (int i = 0; i < icons.Count; i++)
                        {
                            Vector2 iconPos = ImGui.GetWindowPos().AddX(10) + new Vector2(i * (40 + padX), padY);
                            Vector2 iconSize = new Vector2(40, 40);
                            this.DrawIconPreview(iconPos, iconSize, icons[i], this.CropIcon, false, true);

                            if (ImGui.IsMouseHoveringRect(iconPos, iconPos + iconSize))
                            {
                                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                                {
                                    this.CustomIcon = icons[i];
                                    _iconSearchResults.Clear();
                                    _iconSearchInput = string.Empty;
                                }
                            }
                        }

                        ImGui.EndChild();
                    }
                }

                ImGui.Checkbox("Crop Icon", ref this.CropIcon);
                DrawHelpers.DrawSpacing(1);

                Vector2 screenSize = ImGui.GetMainViewport().Size;
                ImGui.DragFloat2("Position", ref this.Position, 1, -screenSize.X / 2, screenSize.X / 2);
                ImGui.DragFloat2("Icon Size", ref this.Size, 1, 0, screenSize.Y);
                ImGui.DragFloat("Icon Opacity", ref this.Opacity, .01f, 0, 1);
                ImGui.Checkbox("Desaturate Icon", ref this.DesaturateIcon);

                ImGui.Checkbox("Show Border", ref this.ShowBorder);
                if (this.ShowBorder)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("Border Thickness", ref this.BorderThickness, 1, 1, 100);

                    DrawHelpers.DrawNestIndicator(1);
                    Vector4 vector = this.BorderColor.Vector;
                    ImGui.ColorEdit4("Border Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                    this.BorderColor.Vector = vector;
                }

                ImGui.Checkbox("Show Progress Swipe", ref this.ShowProgressSwipe);
                if (this.ShowProgressSwipe)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragFloat("Swipe Opacity", ref this.ProgressSwipeOpacity, .01f, 0, 1);
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("Invert Swipe", ref this.InvertSwipe);
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("Show Swipe Lines", ref this.ShowSwipeLines);
                    if (this.ShowSwipeLines)
                    {
                        DrawHelpers.DrawNestIndicator(2);
                        Vector4 vector = this.ProgressLineColor.Vector;
                        ImGui.ColorEdit4("Line Color", ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                        this.ProgressLineColor.Vector = vector;
                        DrawHelpers.DrawNestIndicator(2);
                        ImGui.DragInt("Thickness", ref this.ProgressLineThickness, 1, 1, 5);
                    }
                }
            }

            ImGui.EndChild();
        }

        private void DrawIconPreview(Vector2 iconPos, Vector2 iconSize, ushort icon, bool crop, bool desaturate, bool text)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            DrawHelpers.DrawIcon(icon, iconPos, iconSize, crop, 0, desaturate, 1f, drawList);
            if (text)
            {
                string iconText = icon.ToString();
                Vector2 iconTextPos = iconPos + new Vector2(20 - ImGui.CalcTextSize(iconText).X / 2, 38);
                drawList.AddText(iconTextPos, 0xFFFFFFFF, iconText);
            }
        }
    }
}