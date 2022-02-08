using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class TriggerConfig : IConfigPage
    {
        [JsonIgnore] public string Name => "Triggers";

        [JsonIgnore] private static readonly string[] _typeOptions = Enum.GetNames<TriggerType>();
        [JsonIgnore] private static readonly string[] _condOptions = new string[] { "AND", "OR", "XOR" };
        [JsonIgnore] private static readonly string[] _operatorOptions = new string[] { "==", "!=", "<", ">", "<=", ">=" };
        [JsonIgnore] private int _swapX = -1;
        [JsonIgnore] private int _swapY = -1;

        [JsonIgnore] private int _selectedIndex = 0;
        [JsonIgnore] private TriggerType _selectedType = 0;

        public List<TriggerOptions> TriggerOptions { get; init; }
        public int DataTrigger = 0;

        public TriggerConfig()
        {
            this.TriggerOptions = new List<TriggerOptions>() { new StatusTrigger() };
        }

        public IConfigPage GetDefault() => new TriggerConfig();

        public bool IsTriggered(bool preview, out DataSource[] data, out int triggeredIndex)
        {
            data = new DataSource[this.TriggerOptions.Count];
            triggeredIndex = this.DataTrigger == 0 ? 0 : this.DataTrigger - 1;
            if (!this.TriggerOptions.Any())
            {
                return false;
            }

            bool triggered = this.TriggerOptions[0].IsTriggered(preview, out data[0]);
            bool anyTriggered = triggered;
            for (int i = 1; i < data.Length; i++)
            {
                var trigger = this.TriggerOptions[i];
                bool currentTriggered = this.TriggerOptions[i].IsTriggered(preview, out data[i]);
                triggered = trigger.Condition switch
                {
                    TriggerCond.And => triggered && currentTriggered,
                    TriggerCond.Or => triggered || currentTriggered,
                    TriggerCond.Xor => triggered ^ currentTriggered,
                    _ => false
                };

                if (!anyTriggered && currentTriggered && this.DataTrigger == 0)
                {
                    triggeredIndex = i;
                }

                anyTriggered |= currentTriggered;
            }

            return triggered;
        }

        private string[] GetTriggerDataOptions()
        {
            string[] options = new string[this.TriggerOptions.Count + 1];
            options[0] = "Dynamic data from first active Trigger";
            for (int i = 1; i < options.Length; i++)
            {
                options[i] = $"Data from Trigger {i}";
            }

            return options;
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (!this.TriggerOptions.Any())
            {
                return;
            }

            if (ImGui.BeginChild("##TriggerConfig", new Vector2(size.X, size.Y), true))
            {
                if (this.TriggerOptions.Count > 1)
                {
                    string[] dataTriggerOptions = this.GetTriggerDataOptions();
                    ImGui.Combo("Use data from Trigger", ref this.DataTrigger, dataTriggerOptions, dataTriggerOptions.Length);
                }

                ImGui.Text("Trigger List");
                ImGuiTableFlags tableFlags =
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.BordersOuter |
                    ImGuiTableFlags.BordersInner |
                    ImGuiTableFlags.ScrollY |
                    ImGuiTableFlags.NoSavedSettings;

                if (ImGui.BeginTable("##Trigger_Table", 4, tableFlags, new Vector2(size.X - padX * 2, (size.Y - ImGui.GetCursorPosY() - padY * 2) / 4)))
                {
                    Vector2 buttonSize = new(30, 0);
                    int buttonCount = this.TriggerOptions.Count > 1 ? 5 : 3;
                    float actionsWidth = buttonSize.X * buttonCount + padX * (buttonCount - 1);
                    ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthFixed, 60, 0);
                    ImGui.TableSetupColumn("Trigger Name", ImGuiTableColumnFlags.WidthStretch, 0, 1);
                    ImGui.TableSetupColumn("Trigger Type", ImGuiTableColumnFlags.WidthStretch, 0, 2);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 3);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < this.TriggerOptions.Count; i++)
                    {
                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                        this.DrawTriggerRow(i);
                    }
                
                    ImGui.PushID(this.TriggerOptions.Count.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                    ImGui.TableSetColumnIndex(3);
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddTrigger(), "New Trigger", buttonSize);
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Download, () => ImportTrigger(), "Import Trigger", buttonSize);

                    ImGui.EndTable();

                    if (_swapX < this.TriggerOptions.Count && _swapX >= 0 &&
                        _swapY < this.TriggerOptions.Count && _swapY >= 0)
                    {
                        var temp = this.TriggerOptions[_swapX];
                        this.TriggerOptions[_swapX] = this.TriggerOptions[_swapY];
                        this.TriggerOptions[_swapY] = temp;

                        _swapX = -1;
                        _swapY = -1;
                    }
                }

                ImGui.Text($"Edit Trigger {_selectedIndex + 1}");
                if (ImGui.BeginChild("##TriggerEdit", new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2), true))
                {
                    TriggerOptions selectedTrigger = this.TriggerOptions[_selectedIndex];
                    _selectedType = selectedTrigger.Type;
                    if (ImGui.Combo("Trigger Type", ref Unsafe.As<TriggerType, int>(ref _selectedType), _typeOptions, _typeOptions.Length) &&
                        _selectedType != this.TriggerOptions[_selectedIndex].Type)
                    {
                        TriggerCond oldCond = this.TriggerOptions[_selectedIndex].Condition;
                        this.TriggerOptions[_selectedIndex] = _selectedType switch
                        {
                            TriggerType.Status          => new StatusTrigger(),
                            TriggerType.Cooldown        => new CooldownTrigger(),
                            TriggerType.CharacterState  => new CharacterStateTrigger(),
                        _                               => new StatusTrigger()
                        };
                        this.TriggerOptions[_selectedIndex].Condition = oldCond;
                    }
                    
                    selectedTrigger.DrawTriggerOptions(ImGui.GetWindowSize(), padX, padX);
                }

                ImGui.EndChild();
            }

            ImGui.EndChild();
        }

        private void DrawTriggerRow(int i)
        {
            if (i >= TriggerOptions.Count)
            {
                return;
            }

            TriggerOptions trigger = TriggerOptions[i];

            if (ImGui.TableSetColumnIndex(0))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (i == 0 ? 3f : 1f));
                if (i == 0)
                {
                    ImGui.Text("IF");
                }
                else
                {
                    ImGui.PushItemWidth(ImGui.GetColumnWidth());
                    ImGui.Combo("##CondCombo", ref Unsafe.As<TriggerCond, int>(ref trigger.Condition), _condOptions, _condOptions.Length);
                    ImGui.PopItemWidth();
                }
            }

            if (ImGui.TableSetColumnIndex(1))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (i == 0 ? 3f : 0f));
                ImGui.Text($"Trigger {i + 1}");
            }

            if (ImGui.TableSetColumnIndex(2))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (i == 0 ? 3f : 0f));
                ImGui.Text($"{trigger.Type}");
            }

            if (ImGui.TableSetColumnIndex(3))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                Vector2 buttonSize = new Vector2(30, 0);
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => SelectTrigger(i), "Edit Trigger", buttonSize);
       
                if (this.TriggerOptions.Count > 1)
                {
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowUp, () => Swap(i, i - 1), "Move Up", buttonSize);

                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowDown, () => Swap(i, i + 1), "Move Down", buttonSize);
                }

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportTrigger(i), "Export Trigger", buttonSize);
                if (this.TriggerOptions.Count > 1)
                {
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => RemoveTrigger(i), "Remove Trigger", buttonSize);
                }
            }
        }

        private void SelectTrigger(int i)
        {
            _selectedIndex = i;
        }

        private void AddTrigger(TriggerOptions? newTrigger = null)
        {
            this.TriggerOptions.Add(newTrigger ?? new StatusTrigger());
            this.SelectTrigger(this.TriggerOptions.Count - 1);
        }

        private void ExportTrigger(int i)
        {
            if (i < this.TriggerOptions.Count && i >= 0)
            {
                ConfigHelpers.ExportToClipboard<TriggerOptions>(this.TriggerOptions[i]);
            }
        }

        private void ImportTrigger()
        {
            string importString = ImGui.GetClipboardText();
            if (!string.IsNullOrEmpty(importString))
            {
                TriggerOptions? newTrigger = ConfigHelpers.GetFromImportString<TriggerOptions>(importString);
                if (newTrigger is not null)
                {
                    this.AddTrigger(newTrigger);
                }
            }
        }

        private void RemoveTrigger(int i)
        {
            if (i < this.TriggerOptions.Count && i >= 0)
            {
                this.TriggerOptions.RemoveAt(i);
                _selectedIndex = Math.Clamp(_selectedIndex, 0, this.TriggerOptions.Count - 1);
                if (this.TriggerOptions.Count <= 1 || this.DataTrigger >= i + 1)
                {
                    this.DataTrigger = 0;
                }
            }
        }

        private void Swap(int x, int y)
        {
            _swapX = x;
            _swapY = y;
        }
    }
}