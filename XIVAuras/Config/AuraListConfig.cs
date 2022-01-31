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

        [JsonIgnore] private AuraType _selectedType = AuraType.Icon;
        [JsonIgnore] private string _input = string.Empty;
        [JsonIgnore] private string[] _options = new string[] { "Icon", "Bar", "Group" };
        [JsonIgnore] private int _swapX = -1;
        [JsonIgnore] private int _swapY = -1;

        public string Name => "Auras";

        public List<AuraListItem> Auras { get; init; }

        public AuraListConfig()
        {
            this.Auras = new List<AuraListItem>();
        }

        public IConfigPage GetDefault() => new AuraListConfig();

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            this.DrawCreateMenu(size, padX);
            this.DrawAuraTable(size.AddY(-padY), padX);
        }

        private void DrawCreateMenu(Vector2 size, float padX)
        {
            Vector2 buttonSize = new Vector2(40, 0);
            float comboWidth = 100;
            float textInputWidth = size.X - buttonSize.X * 2 - comboWidth - padX * 5;

            if (ImGui.BeginChild("##Buttons", new Vector2(size.X, MenuBarHeight), true))
            {
                ImGui.PushItemWidth(textInputWidth);
                ImGui.InputTextWithHint("##Input", "New Aura Name", ref _input, 100);
                ImGui.PopItemWidth();

                ImGui.SameLine();
                ImGui.PushItemWidth(comboWidth);
                ImGui.Combo("##Type", ref Unsafe.As<AuraType, int>(ref _selectedType), _options, _options.Length);

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => CreateAura(_selectedType, _input), "Create new Aura or Group", buttonSize);

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Download, () => ImportAura(), "Import new Aura or Group from Clipboard", buttonSize);
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

            if (ImGui.BeginTable("##Auras_Table", 4, flags, new Vector2(size.X, size.Y - MenuBarHeight)))
            {
                Vector2 buttonSize = new Vector2(30, 0);
                int buttonCount = this.Auras.Count > 1 ? 5 : 3;
                float actionsWidth = buttonSize.X * buttonCount + padX * (buttonCount - 1);
                float typeWidth = 75;

                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, typeWidth, 1);
                ImGui.TableSetupColumn("Pre.", ImGuiTableColumnFlags.WidthFixed, 23, 2);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 3);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < this.Auras.Count; i++)
                {
                    AuraListItem aura = this.Auras[i];

                    if (!string.IsNullOrEmpty(_input) &&
                        !aura.Name.Contains(_input, StringComparison.OrdinalIgnoreCase))
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
                        ImGui.Checkbox("##Preview", ref aura.Preview);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Preview");
                        }
                    }

                    if (ImGui.TableSetColumnIndex(3))
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditAura(aura), "Edit", buttonSize);

                        if (this.Auras.Count > 1)
                        {
                            ImGui.SameLine();
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowUp, () => Swap(i, i - 1), "Move Up", buttonSize);

                            ImGui.SameLine();
                            DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowDown, () => Swap(i, i + 1), "Move Down", buttonSize);
                        }

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportAura(aura), "Export", buttonSize);

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteAura(aura), "Delete", buttonSize);
                    }
                }

                ImGui.EndTable();
            }

            if (_swapX < this.Auras.Count && _swapX >= 0 &&
                _swapY < this.Auras.Count && _swapY >= 0)
            {
                AuraListItem temp = this.Auras[_swapX];
                this.Auras[_swapX] = this.Auras[_swapY];
                this.Auras[_swapY] = temp;

                _swapX = -1;
                _swapY = -1;
            }
        }

        private void Swap(int x, int y)
        {
            _swapX = x;
            _swapY = y;
        }

        private void CreateAura(AuraType type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                AuraListItem? newAura = type switch
                {
                    AuraType.Group => new AuraGroup(name),
                    AuraType.Icon => AuraIcon.GetDefaultAuraIcon(name),
                    AuraType.Bar => new AuraBar(name),
                    _ => null
                };

                if (newAura is not null)
                {
                    this.Auras.Add(newAura);
                }
            }

            _input = string.Empty;
        }

        private void EditAura(AuraListItem aura)
        {
            Singletons.Get<PluginManager>().Edit(aura);
        }

        private void DeleteAura(AuraListItem aura)
        {
            this.Auras.Remove(aura);
        }

        private void ImportAura()
        {
            string importString = string.Empty;
            try
            {
                importString = ImGui.GetClipboardText();
            }
            catch
            {
                DrawHelpers.DrawNotification("Failed to read from clipboard!", NotificationType.Error);
                return;
            }

            AuraListItem? newAura = ConfigHelpers.GetFromImportString<AuraListItem>(importString);
            if (newAura is not null)
            {
                this.Auras.Add(newAura);
            }
            else
            {
                DrawHelpers.DrawNotification("Failed to Import Aura!", NotificationType.Error);
            }

            _input = string.Empty;
        }

        private void ExportAura(AuraListItem aura)
        {
            ConfigHelpers.ExportToClipboard<AuraListItem>(aura);
        }
    }
}
