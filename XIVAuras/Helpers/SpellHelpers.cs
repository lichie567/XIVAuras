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

        public int GetSpellCooldownInt(uint actionId)
        {
            if ((int)Math.Ceiling(GetSpellCooldown(actionId) % GetRecastTime(actionId)) <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(GetSpellCooldown(actionId) % GetRecastTime(actionId));
        }

        public int GetStackCount(int maxStacks, uint actionId)
        {
            if (GetSpellCooldownInt(actionId) == 0 || GetSpellCooldownInt(actionId) < 0)
            {
                return maxStacks;
            }

            return maxStacks - (int)Math.Ceiling(GetSpellCooldownInt(actionId) / (GetRecastTime(actionId) / maxStacks));
        }

        public static DataSource? GetData(TriggerSource source, TriggerType type, IEnumerable<TriggerData> triggerData, bool onlyMine, bool preview)
        {
            if (preview)
            {
                return new DataSource()
                {
                    Duration = 15,
                    Cooldown = 15,
                    Stacks = 0, // needs to be 0 to preview icon correctly
                };
            }

            if (!triggerData.Any())
            {
                return null;
            }

            if (type == TriggerType.Cooldown)
            {
                SpellHelpers helper = Singletons.Get<SpellHelpers>();
                TriggerData activeTrigger = triggerData.FirstOrDefault(t => helper.GetSpellCooldown(t.Id) > 0);

                if (activeTrigger.Id == 0)
                {
                    return new DataSource();
                }

                return new DataSource()
                {
                    Cooldown = helper.GetSpellCooldown(activeTrigger.Id),
                    Stacks = helper.GetStackCount(activeTrigger.MaxStacks, activeTrigger.Id),
                };
            }
            else
            {
                ClientState clientState = Singletons.Get<ClientState>();
                TargetManager targetManager = Singletons.Get<TargetManager>();

                PlayerCharacter? player = clientState.LocalPlayer;
                if (player is null)
                {
                    return null;
                }

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

                IEnumerable<uint> ids = triggerData.Select(t => t.Id);
                var status = chara.StatusList.FirstOrDefault(o => ids.Contains(o.StatusId) && (o.SourceID == player.ObjectId || !onlyMine));
                return new DataSource()
                {
                    Duration = Math.Abs(status?.RemainingTime ?? 0f),
                    Stacks = status?.StackCount ?? 0,
                };
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
            ExcelSheet<LuminaAction>? sheet = Singletons.Get<DataManager>().GetExcelSheet<LuminaAction>();

            if (!string.IsNullOrEmpty(input) && sheet is not null)
            {
                List<TriggerData> actionList = new List<TriggerData>();

                // Add by id
                if (uint.TryParse(input, out uint value))
                {
                    if (value > 0)
                    {
                        LuminaAction? action = sheet.GetRow(value);
                        if (action is not null)
                        {
                            actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon, action.MaxCharges));
                        }
                    }
                }

                // Add by name
                if (actionList.Count == 0)
                {
                    actionList.AddRange(
                        sheet.Where(action => input.ToLower().Equals(action.Name.ToString().ToLower()))
                            .Select(action => new TriggerData(action.Name, action.RowId, action.Icon, action.MaxCharges)));
                }

                return actionList;
            }

            return new List<TriggerData>();
        }
    }

    public struct DataSource
    {
        public float Duration;
        public int Stacks;
        public float Cooldown;
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
