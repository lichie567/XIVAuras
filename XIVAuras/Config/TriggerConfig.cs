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
        Buff,
        Debuff,
        Cooldown
    }

    public enum TriggerSource
    {
        Player,
        Target,
        TargetOfTarget,
        FocusTarget,
    }

    public struct DataSource
    {
        public float Duration;
        public uint Stacks;
        public float Cooldown;
    }

    public class TriggerConfig : IConfigPage
    {
        [JsonIgnore] public string Name => "Trigger";

        [JsonIgnore] private string[] _options = Enum.GetNames(typeof(TriggerType));

        [JsonIgnore] private int _statusIdInput = -1;

        [JsonIgnore] private TriggerCondition _inputTrigger = new TriggerCondition();

        [JsonIgnore] private string _triggerValueInput = string.Empty;

        public TriggerType TriggerType = TriggerType.Buff;

        public TriggerSource TriggerSource = TriggerSource.Player;

        public List<TriggerCondition> TriggerConditions = new List<TriggerCondition>();

        public uint StatusId = 0;

        public bool IsTriggered(DataSource data)
        {
            if (!this.TriggerConditions.Any())
            {
                return false;
            }

            bool triggered = this.TriggerConditions[0].GetResult(data);
            for (int i = 1; i < this.TriggerConditions.Count; i++)
            {
                TriggerCondition current = this.TriggerConditions[i];

                triggered = current.Cond switch
                {
                    TriggerCond.And => triggered && current.GetResult(data),
                    TriggerCond.Or => triggered || current.GetResult(data),
                    TriggerCond.Xor => triggered ^ current.GetResult(data),
                    _ => false
                };
            }

            return triggered;
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##TriggerConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Combo("Trigger Type", ref Unsafe.As<TriggerType, int>(ref this.TriggerType), _options, _options.Length);

                string[] sourceOptions = this.TriggerType switch
                {
                    TriggerType.Cooldown => new[] { TriggerSource.Player.ToString() },
                    _ => Enum.GetNames(typeof(TriggerSource))
                };

                ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), sourceOptions, sourceOptions.Length);

                if (_statusIdInput == -1)
                {
                    _statusIdInput = (int)this.StatusId;
                }

                if (ImGui.InputInt("Ability Id", ref _statusIdInput, 0, 0, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    this.StatusId = (uint)_statusIdInput;
                }

                DrawHelpers.DrawSpacing(1);
                ImGui.Text("Trigger Conditions");

                ImGuiTableFlags tableFlags =
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.BordersOuter |
                    ImGuiTableFlags.BordersInner |
                    ImGuiTableFlags.ScrollY |
                    ImGuiTableFlags.NoSavedSettings;

                if (ImGui.BeginTable("##Trigger_Table", 5, tableFlags, new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2)))
                {
                    ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthFixed, 60, 0);
                    ImGui.TableSetupColumn("Data Source", ImGuiTableColumnFlags.WidthFixed, 90, 1);
                    ImGui.TableSetupColumn("Operator", ImGuiTableColumnFlags.WidthFixed, 80, 2);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 0, 3);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 45, 4);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < this.TriggerConditions.Count + 1; i++)
                    {
                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                        this.DrawTriggerRow(i);
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }
        }

        private void DrawTriggerRow(int i)
        {
            TriggerCondition trigger = i < this.TriggerConditions.Count ? this.TriggerConditions[i] : _inputTrigger;

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
                    string[] options = TriggerCondition.CondOptions;
                    ImGui.Combo("##CondCombo", ref Unsafe.As<TriggerCond, int>(ref trigger.Cond), options, options.Length);
                    ImGui.PopItemWidth();
                }
            }

            if (ImGui.TableSetColumnIndex(1))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                string[] options = TriggerCondition.SourceOptions;
                ImGui.Combo("##SourceCombo", ref Unsafe.As<TriggerDataSource, int>(ref trigger.Source), options, options.Length);
                ImGui.PopItemWidth();
            }


            if (ImGui.TableSetColumnIndex(2))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                string[] options = TriggerCondition.OperatorOptions;
                ImGui.Combo("##OpCombo", ref Unsafe.As<TriggerDataOp, int>(ref trigger.Op), options, options.Length);
                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(3))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());

                _triggerValueInput = trigger.Value.ToString();
                ImGui.InputText("##InputFloat", ref _triggerValueInput, 10);
                if (float.TryParse(_triggerValueInput, out float value))
                {
                    trigger.Value = value;
                }

                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(4))
            {
                Vector2 buttonSize = new Vector2(45, 0);

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                if (i < this.TriggerConditions.Count)
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => RemoveTrigger(trigger), "Remove Trigger", buttonSize);
                }
                else
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddTrigger(), "New Trigger", buttonSize);
                }
            }
        }

        private void AddTrigger()
        {
            if (!this.TriggerConditions.Any() ||
                (_inputTrigger.Cond != TriggerCond.None &&
                _inputTrigger.Op != TriggerDataOp.None &&
                _inputTrigger.Source != TriggerDataSource.None))
            {
                this.TriggerConditions.Add(_inputTrigger);
                _inputTrigger = new TriggerCondition();
            }
        }

        private void RemoveTrigger(TriggerCondition trigger)
        {
            this.TriggerConditions.Remove(trigger);
        }
    }
}