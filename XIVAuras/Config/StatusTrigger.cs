using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using ImGuiNET;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class StatusTrigger : TriggerOptions
    {
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerSource>();
        [JsonIgnore] private static readonly string[] _triggerConditions = new string[] { "Status Active", "Status Not Active" };
        
        [JsonIgnore] private string _triggerNameInput = string.Empty;
        [JsonIgnore] private string _triggerConditionValueInput = string.Empty;
        [JsonIgnore] private string _remainingTimeValueInput = string.Empty;
        [JsonIgnore] private string _stackCountValueInput = string.Empty;

        public TriggerSource TriggerSource = TriggerSource.Player;
        public string TriggerName = string.Empty;
        public bool ShowOnlyMine = true;
        public int TriggerCondition = 0;

        public bool RemainingTime = false;
        public TriggerDataOp RemainingTimeOp = TriggerDataOp.GreaterThan;
        public float RemainingTimeValue;

        public bool StackCount = false;
        public TriggerDataOp StackCountOp = TriggerDataOp.GreaterThan;
        public float StackCountValue;

        public override TriggerType Type => TriggerType.Status;
        public override TriggerSource Source => this.TriggerSource;

        public override bool IsTriggered(bool preview, out DataSource data)
        {
            if (!this.TriggerData.Any())
            {
                data = new DataSource();
                return false;
            }
            
            data = SpellHelpers.GetStatusData(this.TriggerSource, this.TriggerData, this.ShowOnlyMine, preview);

            switch (this.TriggerCondition)
            {
                case 0:
                    return data.Active &&
                        (!this.RemainingTime || GetResult(data, TriggerDataSource.Value, this.RemainingTimeOp, this.RemainingTimeValue)) &&
                        (!this.StackCount || GetResult(data, TriggerDataSource.Stacks, this.StackCountOp, this.StackCountValue));
                case 1:
                    return !data.Active;
                default:
                    return false;
            }
        }

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), _sourceOptions, _sourceOptions.Length);

            if (string.IsNullOrEmpty(_triggerNameInput))
            {
                _triggerNameInput = this.TriggerName;
            }

            if (ImGui.InputTextWithHint("Status", "Status Name or ID", ref _triggerNameInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.TriggerData.Clear();
                if (!string.IsNullOrEmpty(_triggerNameInput))
                {
                    SpellHelpers.FindStatusEntries(_triggerNameInput).ForEach(t => AddTriggerData(t));
                }

                _triggerNameInput = this.TriggerName;
            }

            ImGui.Checkbox("Only Show My Effects", ref this.ShowOnlyMine);

            DrawHelpers.DrawSpacing(1);
            ImGui.Combo("Trigger Condition", ref this.TriggerCondition, _triggerConditions, _triggerConditions.Length);
            if (this.TriggerCondition == 0)
            {
                string[] operatorOptions = TriggerOptions.OperatorOptions;
                float opComboWidth = 50;
                float valueInputWidth = 40;

                ImGui.Checkbox("Only if Remaining Time", ref this.RemainingTime);
                ImGui.SameLine();

                float padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - opComboWidth - valueInputWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);

                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##RemainingTimeOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.RemainingTimeOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_remainingTimeValueInput))
                {
                    _remainingTimeValueInput = this.RemainingTimeValue.ToString();
                }

                ImGui.PushItemWidth(valueInputWidth);
                if (ImGui.InputText("Seconds##RemainingTimeValue", ref _remainingTimeValueInput, 10, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (float.TryParse(_remainingTimeValueInput, out float value))
                    {
                        this.RemainingTimeValue = value;
                    }
                }

                ImGui.PopItemWidth();

                ImGui.Checkbox("Only if Stack Count", ref this.StackCount);
                ImGui.SameLine();

                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - opComboWidth - valueInputWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);

                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##StackCountOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.StackCountOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_stackCountValueInput))
                {
                    _stackCountValueInput = this.StackCountValue.ToString();
                }

                ImGui.PushItemWidth(valueInputWidth);
                if (ImGui.InputText("Stacks##StackCountValue", ref _stackCountValueInput, 10, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (float.TryParse(_stackCountValueInput, out float value))
                    {
                        this.StackCountValue = value;
                    }
                }

                ImGui.PopItemWidth();
            }
        }
        
        private void AddTriggerData(TriggerData triggerData)
        {
            this.TriggerName = triggerData.Name.ToString();
            _triggerNameInput = this.TriggerName;
            this.TriggerData.Add(triggerData);
        }
    }
}