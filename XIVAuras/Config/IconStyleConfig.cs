using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Auras;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class IconStyleConfig : IConfigPage
    {
        [JsonIgnore]
        public string Name => "Style";

        [JsonIgnore]
        private string _labelInput = string.Empty;

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

        public List<AuraLabel> AuraLabels { get; init; }

        public IconStyleConfig(params AuraLabel[] labels)
        {
            this.AuraLabels = new List<AuraLabel>(labels);
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            ImGuiWindowFlags flags = ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar;
            if (ImGui.BeginChild("##IconStyleConfig", new Vector2(size.X, size.Y), true, flags))
            {
                Vector2 screenSize = ImGui.GetMainViewport().Size;
                ImGui.DragFloat2("Position", ref this.Position, 1, -screenSize.X / 2, screenSize.X / 2);
                ImGui.DragFloat2("Icon Size", ref this.Size, 1, 0, screenSize.Y);

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

                DrawHelpers.DrawSpacing(1);
                ImGui.Text("Labels");

                ImGuiTableFlags tableFlags =
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.BordersOuter |
                    ImGuiTableFlags.BordersInner |
                    ImGuiTableFlags.ScrollY |
                    ImGuiTableFlags.NoSavedSettings;

                if (ImGui.BeginTable("##Label_Table", 2, tableFlags, new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2)))
                {
                    Vector2 buttonSize = new Vector2(30, 0);
                    float actionsWidth = buttonSize.X * 3 + padX * 2;

                    ImGui.TableSetupColumn("Label Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 1);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    int i = 0;
                    for (; i < this.AuraLabels.Count; i++)
                    {
                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                        AuraLabel label = this.AuraLabels[i];

                        if (ImGui.TableSetColumnIndex(0))
                        {
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                            ImGui.Text(label.Name);
                        }

                        if (ImGui.TableSetColumnIndex(1))
                        {
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditLabel(label), "Edit", buttonSize);

                            ImGui.SameLine();
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportLabel(label), "Export", buttonSize);

                            ImGui.SameLine();
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteLabel(label), "Delete", buttonSize);
                        }
                    }

                    ImGui.PushID((i + 1).ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        ImGui.PushItemWidth(ImGui.GetColumnWidth());
                        ImGui.InputTextWithHint("##LabelInput", "New Label Name/Import String", ref _labelInput, 10000);
                        ImGui.PopItemWidth();
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddLabel(_labelInput), "Create Label", buttonSize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Download, () => ImportLabel(_labelInput), "Import Label", buttonSize);
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }
        }

        private void AddLabel(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.AuraLabels.Add(new AuraLabel(name));
            }

            this._labelInput = string.Empty;
        }

        private void ImportLabel(string importString)
        {
            if (!string.IsNullOrEmpty(importString))
            {
                AuraListItem? newAura = ConfigHelpers.GetAuraFromImportString(importString);

                if (newAura is AuraLabel label)
                {
                    this.AuraLabels.Add(label);
                }
                else
                {
                    DrawHelpers.DrawNotification("Failed to Import Aura!", NotificationType.Error);
                }
            }

            this._labelInput = string.Empty;
        }

        private void EditLabel(AuraLabel label)
        {
            Singletons.Get<PluginManager>().Edit(label);
        }

        private void ExportLabel(AuraLabel label)
        {
            ConfigHelpers.ExportAuraToClipboard(label);
        }

        private void DeleteLabel(AuraLabel label)
        {
            this.AuraLabels.Remove(label);
        }
    }
}