using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
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
        [JsonIgnore] private string _durationValueInput = string.Empty;
        [JsonIgnore] private string _stackCountValueInput = string.Empty;

        public TriggerSource TriggerSource = TriggerSource.Player;
        public string TriggerName = string.Empty;
        public int TriggerCondition = 0;

        public bool OnlyMine = true;

        public bool Duration = false;
        public TriggerDataOp DurationOp = TriggerDataOp.GreaterThan;
        public float DurationValue;

        public bool StackCount = false;
        public TriggerDataOp StackCountOp = TriggerDataOp.GreaterThan;
        public float StackCountValue;


        public override TriggerType Type => TriggerType.Status;
        public override TriggerSource Source => this.TriggerSource;

        public override bool IsTriggered(bool preview, out DataSource data)
        {
            data = new DataSource();
            if (!this.TriggerData.Any())
            {
                return false;
            }
            
            if (preview)
            {
                data.Value = 10;
                data.Stacks = 2;
                data.MaxStacks = 2;
                data.Icon = this.TriggerData.FirstOrDefault().Icon;
                return true;
            }

            PlayerCharacter? player = Singletons.Get<ClientState>().LocalPlayer;
            if (player is null)
            {
                return false;
            }

            bool active = false;
            StatusHelpers helper = Singletons.Get<StatusHelpers>();
            foreach(TriggerData trigger in this.TriggerData)
            {
                foreach (var status in helper.GetStatus(this.Source, trigger.Id))
                {
                    if (status is not null &&
                        (status.SourceID == player.ObjectId || !this.OnlyMine))
                    {
                        active = true;
                        data.Id = status.StatusId;
                        data.Value = Math.Abs(status.RemainingTime);
                        data.Stacks = status.StackCount;
                        data.MaxStacks = trigger.MaxStacks;
                        data.Icon = trigger.Icon;
                        break;
                    }
                }
            }

            switch (this.TriggerCondition)
            {
                case 0:
                    return active &&
                        (!this.Duration || Utils.GetResult(data.Value, this.DurationOp, this.DurationValue)) &&
                        (!this.StackCount || Utils.GetResult(data.Stacks, this.StackCountOp, this.StackCountValue));
                case 1:
                    return !active;
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
                    StatusHelpers.FindStatusEntries(_triggerNameInput).ForEach(t => AddTriggerData(t));
                }

                _triggerNameInput = this.TriggerName;
            }

            ImGui.Checkbox("Only Mine", ref this.OnlyMine);
            DrawHelpers.DrawSpacing(1);
            ImGui.Combo("Trigger Condition", ref this.TriggerCondition, _triggerConditions, _triggerConditions.Length);
            if (this.TriggerCondition == 0)
            {
                string[] operatorOptions = TriggerOptions.OperatorOptions;
                float optionsWidth = 100 + padX;
                float opComboWidth = 55;
                float valueInputWidth = 45;
                float padWidth = 0;

                ImGui.Checkbox("Duration Remaining", ref this.Duration);
                if (this.Duration)
                {
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    ImGui.PushItemWidth(opComboWidth);
                    ImGui.Combo("##DurationOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.DurationOp), operatorOptions, operatorOptions.Length);
                    ImGui.PopItemWidth();
                    ImGui.SameLine();

                    if (string.IsNullOrEmpty(_durationValueInput))
                    {
                        _durationValueInput = this.DurationValue.ToString();
                    }

                    ImGui.PushItemWidth(valueInputWidth);
                    if (ImGui.InputText("Seconds##DurationValue", ref _durationValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_durationValueInput, out float value))
                        {
                            this.DurationValue = value;
                        }

                        _durationValueInput = this.DurationValue.ToString();
                    }

                    ImGui.PopItemWidth();
                }

                ImGui.Checkbox("Stack Count", ref this.StackCount);
                if (this.StackCount)
                {
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
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
                    if (ImGui.InputText("Stacks##StackCountValue", ref _stackCountValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (float.TryParse(_stackCountValueInput, out float value))
                        {
                            this.StackCountValue = value;
                        }

                        _stackCountValueInput = this.StackCountValue.ToString();
                    }
                    
                    ImGui.PopItemWidth();
                }
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