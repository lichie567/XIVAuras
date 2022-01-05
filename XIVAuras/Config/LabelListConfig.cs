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
    public class LabelListConfig : IConfigPage
    {
        [JsonIgnore]
        public string Name => "Labels";

        [JsonIgnore]
        private string _labelInput = string.Empty;

        public List<AuraLabel> AuraLabels { get; init; }

        public LabelListConfig()
        {
            this.AuraLabels = new List<AuraLabel>();
        }

        public LabelListConfig(params AuraLabel[] labels)
        {
            this.AuraLabels = new List<AuraLabel>(labels);
        }

        public IConfigPage GetDefault()
        {
            AuraLabel valueLabel = new AuraLabel("Value", "[value:t]");
            valueLabel.LabelStyleConfig.FontKey = FontsManager.DefaultBigFontKey;
            valueLabel.LabelStyleConfig.FontID = Singletons.Get<FontsManager>().GetFontIndex(FontsManager.DefaultBigFontKey);
            valueLabel.StyleConditions.Conditions.Add(new StyleCondition<LabelStyleConfig>()
            {
                Source = TriggerDataSource.Value,
                Op = TriggerDataOp.Equals,
                Value = 0
            });

            AuraLabel stacksLabel = new AuraLabel("Stacks", "[stacks]");
            stacksLabel.LabelStyleConfig.FontKey = FontsManager.DefaultMediumFontKey;
            stacksLabel.LabelStyleConfig.FontID = Singletons.Get<FontsManager>().GetFontIndex(FontsManager.DefaultMediumFontKey);
            stacksLabel.LabelStyleConfig.Position = new Vector2(-1, 0);
            stacksLabel.LabelStyleConfig.ParentAnchor = DrawAnchor.BottomRight;
            stacksLabel.LabelStyleConfig.TextAlign = DrawAnchor.BottomRight;
            stacksLabel.LabelStyleConfig.TextColor = new ConfigColor(0, 0, 0, 1);
            stacksLabel.LabelStyleConfig.OutlineColor = new ConfigColor(1, 1, 1, 1);
            stacksLabel.StyleConditions.Conditions.Add(new StyleCondition<LabelStyleConfig>()
            {
                Source = TriggerDataSource.MaxStacks,
                Op = TriggerDataOp.LessThanEq,
                Value = 1
            });

            return new LabelListConfig(valueLabel, stacksLabel);
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            this.DrawLabelTable(size, padX);
        }

        private void DrawLabelTable(Vector2 size, float padX)
        {
            ImGuiTableFlags tableFlags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.NoSavedSettings;

            if (ImGui.BeginTable("##Label_Table", 2, tableFlags, size))
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
        }

        private void AddLabel(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.AuraLabels.Add(new AuraLabel(name));
            }

            this._labelInput = string.Empty;
        }

        private void ImportLabel(string input)
        {
            string importString = input;
            if (string.IsNullOrEmpty(importString))
            {
                importString = ImGui.GetClipboardText();
            }
            
            AuraListItem? newAura = ConfigHelpers.GetFromImportString<AuraListItem>(importString);

            if (newAura is AuraLabel label)
            {
                this.AuraLabels.Add(label);
            }
            else
            {
                DrawHelpers.DrawNotification("Failed to Import Aura!", NotificationType.Error);
            }

            this._labelInput = string.Empty;
        }

        private void EditLabel(AuraLabel label)
        {
            Singletons.Get<PluginManager>().Edit(label);
        }

        private void ExportLabel(AuraLabel label)
        {
            ConfigHelpers.ExportToClipboard<AuraLabel>(label);
        }

        private void DeleteLabel(AuraLabel label)
        {
            this.AuraLabels.Remove(label);
        }
    }
}
