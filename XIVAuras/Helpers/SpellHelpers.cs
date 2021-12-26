using System;
using System.Collections.Generic;
using System.Linq;
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

        public static DataSource? GetStatusData(TriggerSource source, IEnumerable<TriggerData> triggerData, bool onlyMine, bool preview)
        {
            if (preview)
            {
                return new DataSource()
                {
                    Value = 10,
                    Stacks = 2,
                    MaxStacks = 2
                };
            }

            PlayerCharacter? player = Singletons.Get<ClientState>().LocalPlayer;
            if (player is null)
            {
                return null;
            }

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

        public static DataSource? GetCooldownData(IEnumerable<TriggerData> triggerData, bool preview)
        {
            if (preview)
            {
                return new DataSource()
                {
                    Value = 10,
                    Stacks = 2,
                    MaxStacks = 2
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

            SpellHelpers helper = Singletons.Get<SpellHelpers>();
            TriggerData activeTrigger = triggerData.First();

            int maxCharges = helper.GetMaxCharges(activeTrigger.Id, player.Level);
            int stacks = helper.GetStackCount(maxCharges, activeTrigger.Id);
            float chargeTime = helper.GetRecastTime(activeTrigger.Id) / maxCharges;
            float cooldown = chargeTime != 0 
                ? helper.GetSpellCooldown(activeTrigger.Id) % chargeTime
                : chargeTime;

            return new DataSource()
            {
                Value = cooldown,
                Stacks = stacks,
                MaxStacks = maxCharges
            };
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
                actionList.AddRange(FindEntriesFromActionSheet(input));

                if (!actionList.Any())
                {
                    actionList.AddRange(FindEntriesFromActionIndirectionSheet(input));
                }

                if (!actionList.Any())
                {
                    actionList.AddRange(FindEntriesFromGeneralActionSheet(input));
                }
            }

            return actionList;
        }

        public static List<TriggerData> FindEntriesFromActionSheet(string input)
        {
            List<TriggerData> actionList = new List<TriggerData>();
            ExcelSheet<LuminaAction>? actionSheet = Singletons.Get<DataManager>().GetExcelSheet<LuminaAction>();

            if (actionSheet is null)
            {
                return actionList;
            }
            
            // Add by id
            if (uint.TryParse(input, out uint value))
            {
                if (value > 0)
                {
                    LuminaAction? action = actionSheet.GetRow(value);
                    if (action is not null && (action.IsPlayerAction || action.IsRoleAction))
                    {
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon));
                    }
                }
            }

            // Add by name
            if (!actionList.Any())
            {
                foreach(LuminaAction action in actionSheet)
                {
                    if (input.ToLower().Equals(action.Name.ToString().ToLower()) && (action.IsPlayerAction || action.IsRoleAction))
                    {
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon));
                    }
                }
            }
            
            return actionList;
        }

        public static List<TriggerData> FindEntriesFromActionIndirectionSheet(string input)
        {
            List<TriggerData> actionList = new List<TriggerData>();
            ExcelSheet<ActionIndirection>? actionIndirectionSheet = Singletons.Get<DataManager>().GetExcelSheet<ActionIndirection>();

            if (actionIndirectionSheet is null)
            {
                return actionList;
            }

            // Add by id
            if (uint.TryParse(input, out uint value))
            {
                foreach (ActionIndirection iAction in actionIndirectionSheet)
                {
                    LuminaAction? action = iAction.Name.Value;
                    if (action is not null && action.RowId == value)
                    {
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon));
                        break;
                    }
                }
            }
            
            // Add by name
            if (!actionList.Any())
            {
                foreach (ActionIndirection indirectAction in actionIndirectionSheet)
                {
                    LuminaAction? action = indirectAction.Name.Value;
                    if (action is not null && input.ToLower().Equals(action.Name.ToString().ToLower()))
                    {
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon));
                    }
                }
            }

            return actionList;
        }

        public static List<TriggerData> FindEntriesFromGeneralActionSheet(string input)
        {
            List<TriggerData> actionList = new List<TriggerData>();
            ExcelSheet<GeneralAction>? generalSheet = Singletons.Get<DataManager>().GetExcelSheet<GeneralAction>();

            if (generalSheet is null)
            {
                return actionList;
            }

            // Add by name (Add by id doesn't really work, these sheets are a mess)
            if (!actionList.Any())
            {
                foreach (GeneralAction generalAction in generalSheet)
                {
                    LuminaAction? action = generalAction.Action.Value;
                    if (action is not null && input.ToLower().Equals(generalAction.Name.ToString().ToLower()))
                    {
                        actionList.Add(new TriggerData(generalAction.Name, action.RowId, (ushort)generalAction.Icon));
                    }
                }
            }

            return actionList;
        }
    }

    public class DataSource
    {
        public float Value;
        public int Stacks;
        public int MaxStacks;

        public float GetDataForSourceType(TriggerDataSource source)
        {
            return source switch
            {
                TriggerDataSource.Value => this.Value,
                TriggerDataSource.Stacks => this.Stacks,
                TriggerDataSource.MaxStacks => this.MaxStacks,
                _ => 0
            };
        }
    }

    public struct TriggerData
    {
        public string Name;
        public uint Id;
        public ushort Icon;
        public byte MaxStacks;

        public TriggerData(string name, uint id, ushort icon, byte maxStacks = 0)
        {
            Name = name;
            Id = id;
            Icon = icon;
            MaxStacks = maxStacks;
        }
    }
}
