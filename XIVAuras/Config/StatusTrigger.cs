using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Dalamud.Interface;
using ImGuiNET;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class StatusTrigger : TriggerOptions
    {
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerSource>();
        [JsonIgnore] private static readonly string[] _dataSourceOptions = new string[] { "Duration", "Stacks" };
        
        [JsonIgnore] private string _triggerNameInput = string.Empty;
        [JsonIgnore] private string _triggerConditionValueInput = string.Empty;

        public TriggerSource TriggerSource = TriggerSource.Player;
        public string TriggerName = string.Empty;
        public bool ShowOnlyMine = true;

        public List<TriggerCondition> TriggerConditions { get; private set; } = new List<TriggerCondition>();

        public override TriggerType Type => TriggerType.Status;
        public override TriggerSource Source => this.TriggerSource;

        public static TriggerOptions GetDefault()
        {
            StatusTrigger newTrigger = new StatusTrigger();
            newTrigger.TriggerConditions.Add(
                new TriggerCondition
                {
                    Cond = TriggerCond.And,
                    Source = TriggerDataSource.Value,
                    Op = TriggerDataOp.GreaterThan,
                    Value = 0
                });

            return newTrigger;
        }

        public override bool IsTriggered(bool preview, out DataSource? data)
        {
            data = null;
            if (!this.TriggerConditions.Any())
            {
                return false;
            }

            data = SpellHelpers.GetStatusData(this.TriggerSource, this.TriggerData, this.ShowOnlyMine, preview);

            if (data is null)
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

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), _sourceOptions, _sourceOptions.Length);

            if (string.IsNullOrEmpty(_triggerNameInput))
            {
                _triggerNameInput = TriggerName;
            }

            if (ImGui.InputTextWithHint("Trigger", "Status Name or ID", ref _triggerNameInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.TriggerData.Clear();
                if (!string.IsNullOrEmpty(_triggerNameInput))
                {
                    SpellHelpers.FindStatusEntries(_triggerNameInput).ForEach(t => AddTriggerData(t));
                }

                _triggerNameInput = TriggerName;
            }

            ImGui.Checkbox("Only Show My Effects", ref this.ShowOnlyMine);
            
            DrawHelpers.DrawSpacing(1);
            ImGui.Text("Trigger Conditions");

            ImGuiTableFlags tableFlags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.NoSavedSettings;

            if (ImGui.BeginTable("##TriggerConditions", 5, tableFlags, new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2)))
            {
                ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthFixed, 60, 0);
                ImGui.TableSetupColumn("Data Source", ImGuiTableColumnFlags.WidthFixed, 90, 1);
                ImGui.TableSetupColumn("Operator", ImGuiTableColumnFlags.WidthFixed, 80, 2);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 0, 3);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 45, 4);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < this.TriggerConditions.Count; i++)
                {
                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    this.DrawTriggerConditionRow(i);
                }
                
                ImGui.PushID(this.TriggerConditions.Count.ToString());
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                ImGui.TableSetColumnIndex(4);
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddTriggerCondition(), "New Trigger Condition", new Vector2(45, 0));

                ImGui.EndTable();
            }
        }
        
        private void AddTriggerData(TriggerData triggerData)
        {
            TriggerName = triggerData.Name.ToString();
            _triggerNameInput = TriggerName;
            this.TriggerData.Add(triggerData);
        }
        
        private void DrawTriggerConditionRow(int i)
        {
            TriggerCondition trigger = this.TriggerConditions[i];

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
                ImGui.Combo("##SourceCombo", ref Unsafe.As<TriggerDataSource, int>(ref trigger.Source), _dataSourceOptions, _dataSourceOptions.Length);
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

                _triggerConditionValueInput = trigger.Value.ToString();
                ImGui.InputText("##InputFloat", ref _triggerConditionValueInput, 10);
                if (float.TryParse(_triggerConditionValueInput, out float value))
                {
                    trigger.Value = value;
                }

                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(4))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                if (i < this.TriggerConditions.Count && i > 0)
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => RemoveTriggerCondition(trigger), "Remove Trigger", new Vector2(45, 0));
                }
            }
        }
        
        private void AddTriggerCondition()
        {
            this.TriggerConditions.Add(new TriggerCondition());
        }

        private void RemoveTriggerCondition(TriggerCondition trigger)
        {
            this.TriggerConditions.Remove(trigger);
        }
    }
}