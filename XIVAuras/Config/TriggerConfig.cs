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
    public enum TriggerType
    {
        Status,
        Cooldown
    }

    public enum TriggerSource
    {
        Player,
        Target,
        TargetOfTarget,
        FocusTarget
    }

    public class TriggerConfig : IConfigPage
    {
        [JsonIgnore] public string Name => "Triggers";

        [JsonIgnore] private static readonly string[] _statusOptions = Enum.GetNames<TriggerStatus>();
        [JsonIgnore] private static readonly string[] _typeOptions = Enum.GetNames<TriggerType>();
        [JsonIgnore] private static readonly string[] _condOptions = new string[] { "AND", "OR", "XOR" };
        [JsonIgnore] private static readonly string[] _operatorOptions = new string[] { "==", "!=", "<", ">", "<=", ">=" };

        [JsonIgnore] private int _selectedIndex = 0;
        [JsonIgnore] private TriggerType _selectedType = 0;

        public List<TriggerOptions> TriggerOptions { get; private set; } = new List<TriggerOptions>();

        public bool IsTriggered(bool preview, out DataSource?[] data)
        {
            data = new DataSource?[this.TriggerOptions.Count];
            if (!this.TriggerOptions.Any())
            {
                return false;
            }

            if (data is null)
            {
                return false;
            }

            bool triggered = this.TriggerOptions[0].IsTriggered(preview, out data[0]);
            for (int i = 1; i < this.TriggerOptions.Count; i++)
            {
                TriggerOptions current = this.TriggerOptions[i];

                bool currentTriggered = current.IsTriggered(preview, out data[i]);
                triggered = current.Condition switch
                {
                    TriggerCond.And => triggered && currentTriggered,
                    TriggerCond.Or => triggered || currentTriggered,
                    TriggerCond.Xor => triggered ^ currentTriggered,
                    _ => false
                };
            }

            return triggered;
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##TriggerConfig", new Vector2(size.X, size.Y), true))
            {
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
                    ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthFixed, 60, 0);
                    ImGui.TableSetupColumn("Trigger Name", ImGuiTableColumnFlags.WidthFixed, 90, 1);
                    ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.WidthStretch, 0, 2);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 90, 3);

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
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddTrigger(), "New Trigger", new Vector2(40, 0));

                    ImGui.EndTable();
                }

                ImGui.Text($"Edit Trigger {_selectedIndex + 1}");
                if (ImGui.BeginChild("##TriggerEdit", new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2), true))
                {
                    TriggerOptions selectedTrigger = this.TriggerOptions[_selectedIndex];
                    _selectedType = selectedTrigger.Type;
                    if (ImGui.Combo("Trigger Type", ref Unsafe.As<TriggerType, int>(ref _selectedType), _typeOptions, _typeOptions.Length))
                    {
                        this.TriggerOptions[_selectedIndex] = _selectedType switch
                        {
                            TriggerType.Status   => StatusTrigger.GetDefault(),
                            TriggerType.Cooldown => CooldownTrigger.GetDefault(),
                            _                    => StatusTrigger.GetDefault()
                        };
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
                if (i == 0)
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                    ImGui.Text("IF");
                }
                else
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                    ImGui.PushItemWidth(ImGui.GetColumnWidth());
                    ImGui.Combo("##CondCombo", ref Unsafe.As<TriggerCond, int>(ref trigger.Condition), _condOptions, _condOptions.Length);
                    ImGui.PopItemWidth();
                }
            }

            if (ImGui.TableSetColumnIndex(1))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                ImGui.Text($"Trigger {i + 1}");
            }

            if (ImGui.TableSetColumnIndex(2))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                string[] options = TriggerCondition.OperatorOptions;
                ImGui.Combo("##StatusCombo", ref Unsafe.As<TriggerStatus, int>(ref trigger.Status), _statusOptions, _statusOptions.Length);
                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(3))
            {
                Vector2 buttonSize = new Vector2(40, 0);

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                if (i < this.TriggerOptions.Count)
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => SelectTrigger(i), "Edit Trigger", buttonSize);
                    if (i > 0)
                    {
                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => RemoveTrigger(trigger), "Remove Trigger", buttonSize);
                    }
                }
                else
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddTrigger(), "New Trigger", buttonSize);
                }
            }
        }

        private void SelectTrigger(int i)
        {
            _selectedIndex = i;
        }

        private void AddTrigger()
        {
            this.TriggerOptions.Add(StatusTrigger.GetDefault());
            this.SelectTrigger(this.TriggerOptions.Count - 1);
        }

        private void RemoveTrigger(TriggerOptions trigger)
        {
            this.TriggerOptions.Remove(trigger);
            _selectedIndex = Math.Clamp(_selectedIndex, 0, this.TriggerOptions.Count - 1);
        }
    }
}