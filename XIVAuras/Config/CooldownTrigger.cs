using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState;
using ImGuiNET;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class CooldownTrigger : TriggerOptions
    {
        [JsonIgnore] private static readonly string[] _comboOptions = new[] { "Ready", "Not Ready" };
        [JsonIgnore] private static readonly string[] _usableOptions = new[] { "Usable", "Not Usable" };
        [JsonIgnore] private static readonly string[] _rangeOptions = new[] { "In Range", "Not in Range" };
        [JsonIgnore] private static readonly string[] _losOptions = new[] { "In LoS", "Not in LoS" };
        
        [JsonIgnore] private string _triggerNameInput = string.Empty;
        [JsonIgnore] private string _cooldownValueInput = string.Empty;
        [JsonIgnore] private string _chargeCountValueInput = string.Empty;

        public string TriggerName = string.Empty;

        public bool Cooldown = true;
        public TriggerDataOp CooldownOp = TriggerDataOp.GreaterThan;
        public float CooldownValue;

        public bool ChargeCount = false;
        public TriggerDataOp ChargeCountOp = TriggerDataOp.GreaterThan;
        public float ChargeCountValue;

        public bool Combo = false;
        public int ComboValue;

        public bool Usable = false;
        public int UsableValue;

        public bool RangeCheck;
        public int RangeValue;

        public bool LosCheck;
        public int LosValue;

        public override TriggerType Type => TriggerType.Cooldown;
        public override TriggerSource Source => TriggerSource.Player;

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

            SpellHelpers helper = Singletons.Get<SpellHelpers>();
            TriggerData actionTrigger = this.TriggerData.First();
            uint actionId = actionTrigger.Id;
            helper.GetAdjustedRecastInfo(actionId, out RecastInfo recastInfo);

            int stacks = recastInfo.RecastTime == 0f
                ? recastInfo.MaxCharges
                : (int)(recastInfo.MaxCharges * (recastInfo.RecastTimeElapsed / recastInfo.RecastTime));

            float chargeTime = recastInfo.MaxCharges != 0
                ? recastInfo.RecastTime / recastInfo.MaxCharges
                : recastInfo.RecastTime;

            float cooldown = chargeTime != 0 
                ? Math.Abs(recastInfo.RecastTime - recastInfo.RecastTimeElapsed) % chargeTime
                : 0;

            bool comboActive = false;
            bool usable = false;
            bool inRange = false;
            bool inLos = false;

            if (this.Usable)
            {
                usable = helper.CanUseAction(actionId);
            }

            if (this.RangeCheck)
            {
                inRange = helper.GetActionInRange(actionId, Singletons.Get<ClientState>().LocalPlayer, Utils.FindTarget());
            }

            if (this.LosCheck)
            {
                inLos = helper.IsTargetInLos(Singletons.Get<ClientState>().LocalPlayer, Utils.FindTarget());
            }

            if (this.Combo && actionTrigger.ComboId.Length > 0)
            {
                uint lastAction = helper.GetLastUsedActionId();
                foreach (uint id in actionTrigger.ComboId)
                {
                    if (id == lastAction)
                    {
                        comboActive = true;
                        break;
                    }
                }
            }

            data.Id = actionId;
            data.Value = cooldown;
            data.Stacks = stacks;
            data.MaxStacks = recastInfo.MaxCharges;
            data.Icon = actionTrigger.Icon;

            return preview ||
                (!this.Combo || (this.ComboValue == 0 ? comboActive : !comboActive)) &&
                (!this.Usable || (this.UsableValue == 0 ? usable : !usable)) &&
                (!this.RangeCheck || (this.RangeValue == 0 ? inRange : !inRange)) &&
                (!this.LosCheck || (this.LosValue == 0 ? inLos : !inLos)) &&
                (!this.Cooldown || GetResult(data.Value, this.CooldownOp, this.CooldownValue)) &&
                (!this.ChargeCount || GetResult(data.Stacks, this.ChargeCountOp, this.ChargeCountValue));
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
            ImGui.Text("Trigger Conditions");
            string[] operatorOptions = TriggerOptions.OperatorOptions;
            float optionsWidth = 100 + padX;
            float opComboWidth = 55;
            float valueInputWidth = 45;
            float padWidth = 0;

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Cooldown", ref this.Cooldown);
            if (this.Cooldown)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
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
                if (ImGui.InputText("Seconds##CooldownValue", ref _cooldownValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (float.TryParse(_cooldownValueInput, out float value))
                    {
                        this.CooldownValue = value;
                    }

                    _cooldownValueInput = this.CooldownValue.ToString();
                }

                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Charge Count", ref this.ChargeCount);
            if (this.ChargeCount)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
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
                if (ImGui.InputText("Stacks##ChargeCountValue", ref _chargeCountValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (float.TryParse(_chargeCountValueInput, out float value))
                    {
                        this.ChargeCountValue = value;
                    }

                    _chargeCountValueInput = this.ChargeCountValue.ToString();
                }

                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Combo Ready", ref this.Combo);
            if (this.Combo)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(optionsWidth);
                ImGui.Combo("##ComboCombo", ref this.ComboValue, _comboOptions, _comboOptions.Length);
                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Action Usable", ref this.Usable);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Usable means resource/proc requirements to use the Action are met.");
            }

            if (this.Usable)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(optionsWidth);
                ImGui.Combo("##UsableCombo", ref this.UsableValue, _usableOptions, _usableOptions.Length);
                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Target Range Check", ref this.RangeCheck);
            if (this.RangeCheck)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(optionsWidth);
                ImGui.Combo("##RangeCombo", ref this.RangeValue, _rangeOptions, _rangeOptions.Length);
                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Target LoS Check", ref this.LosCheck);
            if (this.LosCheck)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(optionsWidth);
                ImGui.Combo("##LosCombo", ref this.LosValue, _losOptions, _losOptions.Length);
                ImGui.PopItemWidth();
            }
        }
        
        private void AddTriggerData(TriggerData triggerData)
        {
            this.TriggerName = triggerData.Name.ToString();
            _triggerNameInput = TriggerName;
            this.TriggerData.Add(triggerData);
        }
    }
}