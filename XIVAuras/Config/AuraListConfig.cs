using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Auras;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class AuraListConfig : IConfigPage
    {
        private const float MenuBarHeight = 40;

        [JsonIgnore] private AuraType _selectedType = AuraType.Group;
        [JsonIgnore] private string _input = string.Empty;
        [JsonIgnore] private string[] _options = Enum.GetNames(typeof(AuraType));

        public string Name => "Auras";

        public List<AuraListItem> Auras { get; init; }

        public AuraListConfig()
        {
            this.Auras = new List<AuraListItem>();
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            this.DrawCreateMenu(size, padX);
            this.DrawAuraTable(size.AddY(-padY), padX);
        }

        private void DrawCreateMenu(Vector2 size, float padX)
        {
            Vector2 buttonsize = new Vector2(40, 0);
            float comboWidth = 100;
            float textInputWidth = size.X - buttonsize.X * 2 - comboWidth - padX * 5;

            if (ImGui.BeginChild("##Buttons", new Vector2(size.X, MenuBarHeight), true))
            {
                ImGui.PushItemWidth(textInputWidth);
                ImGui.InputTextWithHint("##Input", "Aura Name/Import String", ref _input, 10000);
                ImGui.PopItemWidth();

                ImGui.SameLine();
                ImGui.PushItemWidth(comboWidth);
                ImGui.Combo("##Type", ref Unsafe.As<AuraType, int>(ref _selectedType), _options, _options.Length);

                ImGui.SameLine();
                DrawHelpers.DrawButton("", FontAwesomeIcon.Plus, () => CreateAura(_selectedType, _input), "Create new Aura or Group", buttonsize);

                ImGui.SameLine();
                DrawHelpers.DrawButton("", FontAwesomeIcon.Download, () => ImportAura(_input), "Import new Aura or Group", buttonsize);
                ImGui.PopItemWidth();

                ImGui.EndChild();
            }
        }

        private void DrawAuraTable(Vector2 size, float padX)
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.NoSavedSettings;

            if (ImGui.BeginTable("##Auras_Table", 3, flags, new Vector2(size.X, size.Y - MenuBarHeight)))
            {
                Vector2 buttonsize = new Vector2(30, 0);
                float actionsWidth = buttonsize.X * 3 + padX * 2;
                float typeWidth = 75;

                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, typeWidth, 1);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 2);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < this.Auras.Count; i++)
                {
                    AuraListItem aura = this.Auras[i];

                    if (!string.IsNullOrEmpty(this._input) &&
                        !aura.Name.Contains(this._input, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(aura.Name);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                        ImGui.Text(aura.Type.ToString());
                    }

                    if (ImGui.TableSetColumnIndex(2))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditAura(aura), "Edit", buttonsize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportAura(aura), "Export", buttonsize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteAura(aura), "Delete", buttonsize);
                    }
                }

                ImGui.EndTable();
            }
        }

        private void CreateAura(AuraType type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                AuraListItem? newAura = type switch
                {
                    AuraType.Group => new AuraGroup(name),
                    AuraType.Icon => new AuraIcon(name),
                    AuraType.Bar => new AuraBar(name),
                    _ => null
                };

                if (newAura is not null)
                {
                    this.Auras.Add(newAura);
                }
            }

            this._input = string.Empty;
        }

        private void EditAura(AuraListItem aura)
        {
            Singletons.Get<PluginManager>().Edit(aura);
        }

        private void DeleteAura(AuraListItem aura)
        {
            this.Auras.Remove(aura);
        }

        private void ImportAura(string importString)
        {
            if (!string.IsNullOrEmpty(importString))
            {
                AuraListItem? newAura = ConfigHelpers.GetAuraFromImportString(importString);

                if (newAura is not null)
                {
                    this.Auras.Add(newAura);
                }
                else
                {
                    DrawHelpers.DrawNotification("Failed to Import Aura!", NotificationType.Error);
                }
            }

            this._input = string.Empty;
        }

        private void ExportAura(AuraListItem aura)
        {
            ConfigHelpers.ExportAuraToClipboard(aura);
        }
    }
}
