using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    public class AuraIcon : AuraListItem
    {
        public override AuraType Type => AuraType.Icon;

        public IconStyleConfig IconStyleConfig { get; set; }
        public LabelListConfig LabelListConfig { get; set; }
        public TriggerConfig TriggerConfig { get; set; }
        public StyleConditions<IconStyleConfig> StyleConditions { get; set; }
        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public AuraIcon() : this(string.Empty) { }

        public AuraIcon(string name) : base(name)
        {
            this.IconStyleConfig = new IconStyleConfig();
            this.LabelListConfig = new LabelListConfig();
            this.TriggerConfig = new TriggerConfig();
            this.StyleConditions = new StyleConditions<IconStyleConfig>();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.IconStyleConfig;
            yield return this.LabelListConfig;
            yield return this.TriggerConfig;
            yield return this.StyleConditions;
            yield return this.VisibilityConfig;
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case IconStyleConfig newPage:
                    this.IconStyleConfig = newPage;
                    break;
                case LabelListConfig newPage:
                    this.LabelListConfig = newPage;
                    break;
                case TriggerConfig newPage:
                    this.TriggerConfig = newPage;
                    break;
                case StyleConditions<IconStyleConfig> newPage:
                    this.StyleConditions = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null, bool parentVisible = true)
        {
            if (!this.TriggerConfig.TriggerOptions.Any())
            {
                return;
            }

            bool visible = this.VisibilityConfig.IsVisible(parentVisible);
            if (!visible && !this.Preview)
            {
                return;
            }

            bool triggered = this.TriggerConfig.IsTriggered(this.Preview, out DataSource data);
            IconStyleConfig style = this.StyleConditions.GetStyle(data) ?? this.IconStyleConfig;

            Vector2 localPos = pos + style.Position;
            Vector2 size = style.Size;

            if (triggered || this.Preview)
            {
                this.UpdateStartData(data);
                this.UpdateDragData(localPos, size);

                DrawHelpers.DrawInWindow($"##{this.ID}", localPos, size, this.Preview, this.SetPosition, (drawList) =>
                {
                    if (this.Preview)
                    {
                        data = this.UpdatePreviewData(data);
                        if (this.LastFrameWasDragging)
                        {
                            localPos = ImGui.GetWindowPos();
                            style.Position = localPos - pos;
                        }
                    }
                    
                    bool desaturate = style.DesaturateIcon;
                    float alpha = style.Opacity;

                    if (style.IconOption == 3)
                    {
                        drawList.AddRectFilled(localPos, localPos + size, style.IconColor.Base);
                    }
                    else
                    {
                        ushort icon = style.IconOption switch
                        {
                            0 => data.Icon,
                            1 => style.CustomIcon,
                            _ => 0
                        };

                        if (icon > 0)
                        {
                            DrawHelpers.DrawIcon(icon, localPos, size, style.CropIcon, 0, desaturate, alpha, drawList);
                        }
                    }

                    if (style.IconOption != 2)
                    {
                        if (style.ShowProgressSwipe)
                        {
                            this.DrawProgressSwipe(style, localPos, size, data.Value, data.MaxValue, alpha, drawList);
                        }

                        if (style.ShowBorder)
                        {
                            for (int i = 0; i < style.BorderThickness; i++)
                            {
                                Vector2 offset = new Vector2(i, i);
                                Vector4 color = style.BorderColor.Vector.AddTransparency(alpha);
                                drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color));
                            }
                        }

                        if (style.Glow)
                        {
                            this.DrawIconGlow(localPos, size, style.GlowThickness, style.GlowSegments, style.GlowColor, drawList);
                        }
                    }
                });
            }
            else
            {
                this.StartData = null;
                this.StartTime = null;
            }

            foreach (AuraLabel label in this.LabelListConfig.AuraLabels)
            {
                if (!this.Preview && this.LastFrameWasPreview)
                {
                    label.Preview = false;
                }
                else
                {
                    label.Preview |= this.Preview;
                }

                if (triggered || label.Preview)
                {
                    label.SetData(data);
                    label.Draw(localPos, size, visible);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }

        private void DrawProgressSwipe(
            IconStyleConfig style,
            Vector2 pos,
            Vector2 size,
            float triggeredValue,
            float startValue,
            float alpha,
            ImDrawListPtr drawList)
        {
            if (startValue > 0 && triggeredValue != 0)
            {
                bool invert = style.InvertSwipe;
                float percent = (invert ? 0 : 1) - (startValue - triggeredValue) / startValue;

                float radius = (float)Math.Sqrt(Math.Pow(Math.Max(size.X, size.Y), 2) * 2) / 2f;
                float startAngle = -(float)Math.PI / 2;
                float endAngle = startAngle - 2f * (float)Math.PI * percent;

                ImGui.PushClipRect(pos, pos + size, false);
                drawList.PathArcTo(pos + size / 2, radius / 2, startAngle, endAngle, (int)(100f * Math.Abs(percent)));
                uint progressAlpha = (uint)(style.ProgressSwipeOpacity * 255 * alpha) << 24;
                drawList.PathStroke(progressAlpha, ImDrawFlags.None, radius);
                if (style.ShowSwipeLines)
                {
                    Vector2 vec = new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle));
                    Vector2 start = pos + size / 2;
                    Vector2 end = start + vec * radius;
                    float thickness = style.ProgressLineThickness;
                    Vector4 swipeLineColor = style.ProgressLineColor.Vector.AddTransparency(alpha);
                    uint color = ImGui.ColorConvertFloat4ToU32(swipeLineColor);

                    drawList.AddLine(start, end, color, thickness);
                    drawList.AddLine(start, new(pos.X + size.X / 2, pos.Y), color, thickness);
                    drawList.AddCircleFilled(start + new Vector2(thickness / 4, thickness / 4), thickness / 2, color);
                }

                ImGui.PopClipRect();
            }
        }

        private void DrawIconGlow(Vector2 pos, Vector2 size, int thickness, int segments, ConfigColor glowColor, ImDrawListPtr drawList)
        {
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            float anim = (float)(time % 250) / 250f;
            float lengthX = size.X + thickness / 2f;
            float lengthY = size.Y + thickness / 2f;
            uint col1 = glowColor.Base;
            uint col2 = 0xFF000000;

            DrawHelpers.DrawSegmentedLine(drawList, pos, pos.AddX(lengthX), anim, segments, col1, col2, thickness);
            DrawHelpers.DrawSegmentedLine(drawList, pos.AddX(size.X), pos + new Vector2(size.X, lengthY), anim, segments, col1, col2, thickness);
            DrawHelpers.DrawSegmentedLine(drawList, pos + size, pos + new Vector2(-thickness / 2, size.Y), anim, segments, col1, col2, thickness);
            DrawHelpers.DrawSegmentedLine(drawList, pos.AddY(size.Y), pos.AddY(-thickness / 2), anim, segments, col1, col2, thickness);
        }

        public static AuraIcon GetDefaultAuraIcon(string name)
        {
            AuraIcon newIcon = new AuraIcon(name);
            newIcon.ImportPage(newIcon.LabelListConfig.GetDefault());
            return newIcon;
        }
    }
}