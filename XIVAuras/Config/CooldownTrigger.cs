using System.Xml.Linq;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using ImGuiNET;
using XIVAuras.Helpers;
using System.Linq;

namespace XIVAuras.Config
{
    public class CooldownTrigger : TriggerOptions
    {
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerSource>();
        
        [JsonIgnore] private string _triggerNameInput = string.Empty;
        [JsonIgnore] private string _cooldownValueInput = string.Empty;
        [JsonIgnore] private string _chargeCountValueInput = string.Empty;

        public string TriggerName = string.Empty;

        public bool Cooldown = false;
        public TriggerDataOp CooldownOp = TriggerDataOp.GreaterThan;
        public float CooldownValue;

        public bool ChargeCount = false;
        public TriggerDataOp ChargeCountOp = TriggerDataOp.GreaterThan;
        public float ChargeCountValue;

        public bool ActionUsable = false;
        public bool InRange;

        public override TriggerType Type => TriggerType.Cooldown;
        public override TriggerSource Source => TriggerSource.Player;

        public override bool IsTriggered(bool preview, out DataSource data)
        {
            if (!this.TriggerData.Any())
            {
                data = new DataSource();
                return false;
            }

            data = SpellHelpers.GetCooldownData(this.TriggerData, this.ActionUsable, this.InRange, preview);

            return (!this.ActionUsable || data.Active) &&
                (!this.InRange || data.InRange) &&
                (!this.Cooldown || GetResult(data, TriggerDataSource.Value, this.CooldownOp, this.CooldownValue)) &&
                (!this.ChargeCount || GetResult(data, TriggerDataSource.Stacks, this.ChargeCountOp, this.ChargeCountValue));
        }

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            if (string.IsNullOrEmpty(_triggerNameInput))
            {
                _triggerNameInput = TriggerName;
            }

            if (ImGui.InputTextWithHint("Action", "Action Name or ID", ref _triggerNameInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.TriggerData.Clear();
                if (!string.IsNullOrEmpty(_triggerNameInput))
                {
                    SpellHelpers.FindActionEntries(_triggerNameInput).ForEach(t => AddTriggerData(t));
                }

                _triggerNameInput = TriggerName;
            }

            DrawHelpers.DrawSpacing(1);
            string[] operatorOptions = TriggerOptions.OperatorOptions;
            float opComboWidth = 50;
            float valueInputWidth = 40;

            ImGui.Checkbox("Trigger when Cooldown", ref this.Cooldown);
            ImGui.SameLine();

            float padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - opComboWidth - valueInputWidth;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);

            ImGui.PushItemWidth(opComboWidth);
            ImGui.Combo("##CooldownOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.CooldownOp), operatorOptions, operatorOptions.Length);
            ImGui.PopItemWidth();
            ImGui.SameLine();

            if (string.IsNullOrEmpty(_cooldownValueInput))
            {
                _cooldownValueInput = this.CooldownValue.ToString();
            }

            ImGui.PushItemWidth(valueInputWidth);
            if (ImGui.InputText("Seconds##CooldownValue", ref _cooldownValueInput, 10, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (float.TryParse(_cooldownValueInput, out float value))
                {
                    this.CooldownValue = value;
                }
            }
            ImGui.PopItemWidth();

            ImGui.Checkbox("Trigger when Charge Count", ref this.ChargeCount);
            ImGui.SameLine();

            padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - opComboWidth - valueInputWidth;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);

            ImGui.PushItemWidth(opComboWidth);
            ImGui.Combo("##ChargeCountOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.ChargeCountOp), operatorOptions, operatorOptions.Length);
            ImGui.PopItemWidth();
            ImGui.SameLine();

            if (string.IsNullOrEmpty(_chargeCountValueInput))
            {
                _chargeCountValueInput = this.ChargeCountValue.ToString();
            }

            ImGui.PushItemWidth(valueInputWidth);
            if (ImGui.InputText("Stacks##ChargeCountValue", ref _chargeCountValueInput, 10, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (float.TryParse(_chargeCountValueInput, out float value))
                {
                    this.ChargeCountValue = value;
                }
            }
            ImGui.PopItemWidth();

            ImGui.Checkbox("Trigger only if Action is usable", ref this.ActionUsable);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Enable to only trigger when action is usable.\nUsable means Action is not on cooldown and resource/proc/range requirements to use the Action are met.");
            }

            ImGui.Checkbox("Trigger only if target is in range", ref this.InRange);
        }
        
        private void AddTriggerData(TriggerData triggerData)
        {
            this.TriggerName = triggerData.Name.ToString();
            _triggerNameInput = TriggerName;
            this.TriggerData.Add(triggerData);
        }
    }
}