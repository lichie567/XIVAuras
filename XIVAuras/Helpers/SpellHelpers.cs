using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVAuras.Config;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;
using LuminaStatus = Lumina.Excel.GeneratedSheets.Status;

namespace XIVAuras.Helpers
{
    public class SpellHelpers
    {
        private readonly unsafe ActionManager* _actionManager;

        public unsafe SpellHelpers()
        {
            _actionManager = ActionManager.Instance();
        }

        public unsafe uint GetSpellActionId(uint actionId)
        {
            return _actionManager->GetAdjustedActionId(actionId);
        }

        public unsafe float GetRecastTimeElapsed(uint actionId)
        {
            return _actionManager->GetRecastTimeElapsed(ActionType.Spell, GetSpellActionId(actionId));
        }

        public unsafe float GetRecastTime(uint actionId)
        {
            return _actionManager->GetRecastTime(ActionType.Spell, GetSpellActionId(actionId));
        }

        public float GetSpellCooldown(uint actionId)
        {
            return Math.Abs(GetRecastTime(GetSpellActionId(actionId)) - GetRecastTimeElapsed(GetSpellActionId(actionId)));
        }

        public int GetStackCount(int maxStacks, uint actionId)
        {
            float cooldown = this.GetSpellCooldown(actionId);
            if (cooldown <= 0)
            {
                return maxStacks;
            }

            return maxStacks - (int)Math.Ceiling(cooldown / (this.GetRecastTime(actionId) / maxStacks));
        }

        public unsafe ushort GetMaxCharges(uint actionId, uint level)
        {
            return ActionManager.GetMaxCharges(actionId, level);
        }

        public static DataSource? GetData(TriggerSource source, TriggerType type, IEnumerable<TriggerData> triggerData, bool onlyMine, bool preview)
        {
            if (preview)
            {
                return new DataSource()
                {
                    Value = 10,
                    ChargeTime = 10,
                    Stacks = 2
                };
            }

            if (!triggerData.Any())
            {
                return null;
            }

            PlayerCharacter? player = Singletons.Get<ClientState>().LocalPlayer;
            if (player is null)
            {
                return null;
            }

            if (type == TriggerType.Cooldown)
            {
                SpellHelpers helper = Singletons.Get<SpellHelpers>();
                TriggerData activeTrigger = triggerData.First();

                int maxCharges = helper.GetMaxCharges(activeTrigger.Id, player.Level);
                int stacks = helper.GetStackCount(maxCharges, activeTrigger.Id);
                float cooldown = helper.GetSpellCooldown(activeTrigger.Id);
                float chargeTime = maxCharges == stacks
                    ? cooldown
                    : cooldown / (maxCharges - stacks);

                return new DataSource()
                {
                    Value = cooldown,
                    ChargeTime = chargeTime,
                    Stacks = stacks,
                    MaxStacks = maxCharges
                };
            }
            else
            {
                TargetManager targetManager = Singletons.Get<TargetManager>();
                GameObject? target = targetManager.SoftTarget ?? targetManager.Target;
                GameObject? actor = source switch
                {
                    TriggerSource.Player => player,
                    TriggerSource.Target => target,
                    TriggerSource.TargetOfTarget => Utils.FindTargetOfTarget(player, target),
                    TriggerSource.FocusTarget => targetManager.FocusTarget,
                    _ => null
                };

                if (actor is not BattleChara chara)
                {
                    return null;
                }

                foreach (TriggerData trigger in triggerData)
                {
                    foreach (var status in chara.StatusList)
                    {
                        if (status is not null &&
                            status.StatusId == trigger.Id &&
                            (status.SourceID == player.ObjectId || !onlyMine))
                        {
                            return new DataSource()
                            {
                                Value = Math.Abs(status.RemainingTime),
                                Stacks = status.StackCount,
                                MaxStacks = trigger.MaxStacks
                            };
                        }
                    }
                }

                return null;
            }
        }

        public static List<TriggerData> FindStatusEntries(string input)
        {
            ExcelSheet<LuminaStatus>? sheet = Singletons.Get<DataManager>().GetExcelSheet<LuminaStatus>();

            if (!string.IsNullOrEmpty(input) && sheet is not null)
            {
                List<TriggerData> statusList = new List<TriggerData>();

                // Add by id
                if (uint.TryParse(input, out uint value))
                {
                    if (value > 0)
                    {
                        LuminaStatus? status = sheet.GetRow(value);
                        if (status is not null)
                        {
                            statusList.Add(new TriggerData(status.Name, status.RowId, status.Icon, status.MaxStacks));
                        }
                    }
                }

                // Add by name
                if (statusList.Count == 0)
                {
                    statusList.AddRange(
                        sheet.Where(status => input.ToLower().Equals(status.Name.ToString().ToLower()))
                            .Select(status => new TriggerData(status.Name, status.RowId, status.Icon, status.MaxStacks)));
                }

                return statusList;
            }

            return new List<TriggerData>();
        }

        public static List<TriggerData> FindActionEntries(string input)
        {
            List<TriggerData> actionList = new List<TriggerData>();

            if (!string.IsNullOrEmpty(input))
            {
                ExcelSheet<LuminaAction>? actionSheet = Singletons.Get<DataManager>().GetExcelSheet<LuminaAction>();
                if (actionSheet is not null)
                {
                    // Add by id
                    if (uint.TryParse(input, out uint value))
                    {
                        if (value > 0)
                        {
                            LuminaAction? action = actionSheet.GetRow(value);
                            if (action is not null && (action.IsPlayerAction || action.IsRoleAction))
                            {
                                actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon, 0));
                            }
                        }
                    }

                    // Add by name
                    if (actionList.Count == 0)
                    {
                        actionList.AddRange(
                            actionSheet.Where(action => input.ToLower().Equals(action.Name.ToString().ToLower()) && (action.IsPlayerAction || action.IsRoleAction))
                                .Select(action => new TriggerData(action.Name, action.RowId, action.Icon, 0)));
                    }
                }

                ExcelSheet<GeneralAction>? generalSheet = Singletons.Get<DataManager>().GetExcelSheet<GeneralAction>();
                if (generalSheet is not null && actionList.Count == 0)
                {
                    actionList.AddRange(
                        generalSheet.Where(action => input.ToLower().Equals(action.Name.ToString().ToLower()))
                            .Select(action => new TriggerData(action.Name, action.Action.Value?.RowId ?? 0, (ushort)action.Icon, 0)));
                }
            }

            return actionList;
        }
    }

    public class DataSource
    {
        public float Value;
        public float ChargeTime;
        public int Stacks;
        public int MaxStacks;
    }

    public struct TriggerData
    {
        public string Name;
        public uint Id;
        public ushort Icon;
        public byte MaxStacks;

        public TriggerData(string name, uint id, ushort icon, byte maxStacks)
        {
            Name = name;
            Id = id;
            Icon = icon;
            MaxStacks = maxStacks;
        }
    }
}
