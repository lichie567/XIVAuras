using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class ItemCooldownTrigger : TriggerOptions
    {
        
        [JsonIgnore] private string _triggerNameInput = string.Empty;
        [JsonIgnore] private string _cooldownValueInput = string.Empty;

        public string TriggerName = string.Empty;

        public bool Cooldown = false;
        public TriggerDataOp CooldownOp = TriggerDataOp.GreaterThan;
        public float CooldownValue;

        public override TriggerType Type => TriggerType.ItemCooldown;
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
                data.Stacks = 1;
                data.MaxStacks = 1;
                data.Icon = this.TriggerData.FirstOrDefault().Icon;
                return true;
            }

            SpellHelpers helper = Singletons.Get<SpellHelpers>();
            TriggerData actionTrigger = this.TriggerData.First();
            helper.GetItemRecastInfo(actionTrigger.Id, out RecastInfo recastInfo);

            data.MaxValue = recastInfo.RecastTime;
            data.Value = recastInfo.RecastTime - recastInfo.RecastTimeElapsed;
            data.Icon = actionTrigger.Icon;
            data.Id = actionTrigger.Id;

            data.Stacks = GetQuantity(actionTrigger.Id);
            data.MaxStacks = data.Stacks;
            
            return !this.Cooldown || Utils.GetResult(data.Value, this.CooldownOp, this.CooldownValue);
        }

        private unsafe int GetQuantity(uint itemId)
        {
            InventoryManager* manager = InventoryManager.Instance();
            InventoryType[] inventoryTypes = new InventoryType[]
            {
                InventoryType.Inventory1,
                InventoryType.Inventory2,
                InventoryType.Inventory3,
                InventoryType.Inventory4
            };

            foreach (InventoryType inventoryType in inventoryTypes)
            {
                InventoryContainer* container = manager->GetInventoryContainer(inventoryType);
                if (container is not null)
                {
                    for (int i = 0; i < container->Size; i++)
                    {
                        InventoryItem* item = container->GetInventorySlot(i);

                        if (item is not null && item->ItemID == itemId)
                        {
                            return (int)item->Quantity;
                        }
                    }
                }
            }

            return 0;
        }

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            if (string.IsNullOrEmpty(_triggerNameInput))
            {
                _triggerNameInput = this.TriggerName;
            }

            if (ImGui.InputTextWithHint("Item", "Item Name or ID", ref _triggerNameInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.TriggerData.Clear();
                if (!string.IsNullOrEmpty(_triggerNameInput))
                {
                    SpellHelpers.FindItemEntries(_triggerNameInput).ForEach(t => AddTriggerData(t));
                }

                _triggerNameInput = this.TriggerName;
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
        }

        private void ResetTrigger()
        {
            this.TriggerData.Clear();
            this.TriggerName = string.Empty;
            this._triggerNameInput = string.Empty;
        }
        
        private void AddTriggerData(TriggerData triggerData)
        {
            this.TriggerName = triggerData.Name.ToString();
            _triggerNameInput = this.TriggerName;
            this.TriggerData.Add(triggerData);
            Dalamud.Logging.PluginLog.Information($"{triggerData.Name}: {triggerData.Icon}");
        }
    }
}