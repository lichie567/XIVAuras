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

            // ugly hack
            this.StyleConditions.UpdateTriggerCount(this.TriggerConfig.TriggerOptions.Count);
            this.StyleConditions.UpdateDefaultStyle(this.IconStyleConfig);

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

            bool triggered = this.TriggerConfig.IsTriggered(this.Preview, out DataSource[] datas, out int triggeredIndex);
            DataSource data = datas[triggeredIndex];
            IconStyleConfig style = this.StyleConditions.GetStyle(datas, triggeredIndex) ?? this.IconStyleConfig;

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

                    if (style.IconOption == 2)
                    {
                        return;
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

                    if (style.ShowProgressSwipe)
                    {
                        if (style.GcdSwipe && (data.Value == 0 || data.MaxValue == 0 || style.GcdSwipeOnly))
                        {
                            SpellHelpers.GetGCDInfo(out var recastInfo);
                            DrawProgressSwipe(style, localPos, size, recastInfo.RecastTime - recastInfo.RecastTimeElapsed, recastInfo.RecastTime, alpha, drawList);
                        }
                        else
                        {
                            DrawProgressSwipe(style, localPos, size, data.Value, data.MaxValue, alpha, drawList);
                        }
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
                        this.DrawIconGlow(localPos, size, style.GlowThickness, style.GlowSegments, style.GlowSpeed, style.GlowColor, style.GlowColor2, drawList);
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
                    label.SetData(datas, triggeredIndex);
                    label.Draw(localPos, size, visible);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }

        private static void DrawProgressSwipe(
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

        private void DrawIconGlow(Vector2 pos, Vector2 size, int thickness, int segments, float speed, ConfigColor col1, ConfigColor col2, ImDrawListPtr drawList)
        {
            speed = Math.Abs(speed);
            int mod = speed == 0 ? 1 : (int)(250 / speed);
            float prog = (float)(DateTimeOffset.Now.ToUnixTimeMilliseconds() % mod) / mod;

            float offset = thickness / 2 + thickness % 2;
            Vector2 pad = new Vector2(offset);
            Vector2 c1 = new Vector2(pos.X, pos.Y);
            Vector2 c2 = new Vector2(pos.X + size.X, pos.Y);
            Vector2 c3 = new Vector2(pos.X + size.X, pos.Y + size.Y);
            Vector2 c4 = new Vector2(pos.X, pos.Y + size.Y);

            DrawHelpers.DrawSegmentedLineHorizontal(drawList, c1, size.X, thickness, prog, segments, col1, col2);
            DrawHelpers.DrawSegmentedLineVertical(drawList, c2.AddX(-thickness), thickness, size.Y, prog, segments, col1, col2);
            DrawHelpers.DrawSegmentedLineHorizontal(drawList, c3.AddY(-thickness), -size.X, thickness, prog, segments, col1, col2);
            DrawHelpers.DrawSegmentedLineVertical(drawList, c4, thickness, -size.Y, prog, segments, col1, col2);
        }

        public void Resize(Vector2 size, bool conditions)
        {
            this.IconStyleConfig.Size = size;

            if (conditions)
            {
                foreach (var condition in this.StyleConditions.Conditions)
                {
                    condition.Style.Size = size;
                }
            }
        }

        public static AuraIcon GetDefaultAuraIcon(string name)
        {
            AuraIcon newIcon = new AuraIcon(name);
            newIcon.ImportPage(newIcon.LabelListConfig.GetDefault());
            return newIcon;
        }
    }
}