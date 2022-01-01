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
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Newtonsoft.Json;

namespace XIVAuras.Helpers
{
    public class SpellHelpers
    {
        private const string CastRaySig = "48 83 EC 48 48 8B 05 ?? ?? ?? ?? 4D 8B D1";
        private const string ComboSig = "48 89 2D ?? ?? ?? ?? 85 C0";

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private unsafe delegate bool CastRayNative(float* origin, float* direction, float distance, float* worldPos, int* flags);

        private readonly CastRayNative _castRay;
        private readonly unsafe Combo* _combo;
        private readonly unsafe ActionManager* _actionManager;

        public unsafe SpellHelpers(SigScanner scanner)
        {
            _actionManager = ActionManager.Instance();
            _castRay = Marshal.GetDelegateForFunctionPointer<CastRayNative>(scanner.ScanText(CastRaySig));
            _combo = (Combo*) scanner.GetStaticAddressFromSig(ComboSig);
        }

        public unsafe uint GetAdjustedActionId(uint actionId)
        {
            return _actionManager->GetAdjustedActionId(actionId);
        }

        public unsafe void GetAdjustedRecastInfo(uint actionId, out RecastInfo recastInfo)
        {
            recastInfo = default;
            int recastGroup = _actionManager->GetRecastGroup((int)ActionType.Spell, actionId);
            RecastDetail* recastDetail = _actionManager->GetRecastGroupDetail(recastGroup);
            if (recastDetail == null)
            {
                return;
            }
            
            recastInfo.RecastTime = recastDetail->Total;
            recastInfo.RecastTimeElapsed = recastDetail->Elapsed;
            recastInfo.MaxCharges = ActionManager.GetMaxCharges(actionId, 90);
            if (recastInfo.MaxCharges == 1)
            {
                return;
            }

            ushort currentMaxCharges = ActionManager.GetMaxCharges(actionId, 0);
            if (currentMaxCharges == recastInfo.MaxCharges)
            {
                return;
            }

            recastInfo.RecastTime = (recastInfo.RecastTime * currentMaxCharges) / recastInfo.MaxCharges;
            recastInfo.MaxCharges = currentMaxCharges;
            if (recastInfo.RecastTimeElapsed > recastInfo.RecastTime)
            {
                recastInfo.RecastTime = 0;
                recastInfo.RecastTimeElapsed = 0;
            }

            return;
        }

        public unsafe bool CanUseAction(uint actionId, uint targetId = 0xE000_0000)
        {
            return _actionManager->GetActionStatus(ActionType.Spell, actionId, targetId, 0, 1) == 0;
        }

        public unsafe bool GetActionInRange(uint actionId, GameObject? player, GameObject? target)
        {
            if (player is null || target is null)
            {
                return false;
            }

            uint result = ActionManager.GetActionInRangeOrLoS(
                actionId,
                (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)player.Address,
                (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address);

            return result != 566; // 0 == in range, 565 == in range but not facing target, 566 == out of range, 562 == not in LoS
        }

        public unsafe bool IsTargetInLos(GameObject? player, GameObject? target)
        {
            if (target is null || player is null)
            {
                return false;
            }

            Vector3 delta = target.Position - player.Position;
            float distance = delta.Length();
            float* origin = stackalloc[] { player.Position.X, player.Position.Y + 2f, player.Position.Z };
            float* direction = stackalloc[] { delta.X / distance, delta.Y / distance, delta.Z / distance };
            float* worldPos = stackalloc float[32];
            int* flags = stackalloc int[3] { 0x4000, 0x4000, 0x0 };
            return !_castRay(origin, direction, distance, worldPos, flags);
        }

        public unsafe uint GetLastUsedActionId()
        {
            return _combo->Action;
        }

        public static DataSource GetStatusData(
            TriggerSource source,
            IEnumerable<TriggerData> triggerData,
            bool onlyMine,
            bool preview)
        {
            if (preview)
            {
                return new DataSource()
                {
                    Active = true,
                    Value = 10,
                    Stacks = 2,
                    MaxStacks = 2,
                    Icon = triggerData.FirstOrDefault().Icon
                };
            }

            PlayerCharacter? player = Singletons.Get<ClientState>().LocalPlayer;
            if (player is null)
            {
                return new DataSource();
            }

            GameObject? actor = source switch
            {
                TriggerSource.Player => player,
                TriggerSource.Target => Utils.FindTarget(),
                TriggerSource.TargetOfTarget => Utils.FindTargetOfTarget(player),
                TriggerSource.FocusTarget => Singletons.Get<TargetManager>().FocusTarget,
                _ => null
            };

            if (actor is not BattleChara chara)
            {
                return new DataSource();
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
                            Active = true,
                            TriggerId = trigger.Id,
                            Value = Math.Abs(status.RemainingTime),
                            Stacks = status.StackCount,
                            MaxStacks = trigger.MaxStacks,
                            Icon = trigger.Icon
                        };
                    }
                }
            }

            return new DataSource()
            {
                Icon = triggerData.FirstOrDefault().Icon
            };
        }

        public static DataSource GetCooldownData(
            IEnumerable<TriggerData> triggerData,
            bool usable,
            bool combo,
            bool inRange,
            bool inLos,
            bool preview)
        {
            if (preview)
            {
                return new DataSource()
                {
                    Active = true,
                    Value = 10,
                    Stacks = 2,
                    MaxStacks = 2,
                    Icon = triggerData.FirstOrDefault().Icon
                };
            }

            if (!triggerData.Any())
            {
                return new DataSource();
            }

            SpellHelpers helper = Singletons.Get<SpellHelpers>();
            TriggerData actionTrigger = triggerData.First();
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
            if (combo && actionTrigger.ComboId.Length > 0)
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

            return new DataSource()
            {
                Active = usable && helper.CanUseAction(actionId),
                InRange = inRange && helper.GetActionInRange(actionId, Singletons.Get<ClientState>().LocalPlayer, Utils.FindTarget()),
                InLos = inLos && helper.IsTargetInLos(Singletons.Get<ClientState>().LocalPlayer, Utils.FindTarget()),
                ComboActive = comboActive,
                TriggerId = actionId,
                Value = cooldown,
                Stacks = stacks,
                MaxStacks = recastInfo.MaxCharges,
                Icon = actionTrigger.Icon
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
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon, action.MaxCharges, GetComboIds(action)));
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
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon, action.MaxCharges, GetComboIds(action)));
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
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon, action.MaxCharges, GetComboIds(action)));
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
                        actionList.Add(new TriggerData(action.Name, action.RowId, action.Icon, action.MaxCharges, GetComboIds(action)));
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
                        actionList.Add(new TriggerData(generalAction.Name, action.RowId, (ushort)generalAction.Icon, action.MaxCharges));
                    }
                }
            }

            return actionList;
        }

        public static uint[] GetComboIds(LuminaAction? action)
        {
            if (action is null)
            {
                return Array.Empty<uint>();
            }

            return GetComboIds(action.ActionCombo.Value?.RowId ?? 0);
        }

        public static uint[] GetComboIds(uint baseComboId)
        {
            if (baseComboId == 0)
            {
                return Array.Empty<uint>();
            }
            
            List<uint> comboIds = new List<uint>() { baseComboId };
            ExcelSheet<ActionIndirection>? actionIndirectionSheet = Singletons.Get<DataManager>().GetExcelSheet<ActionIndirection>();

            if (actionIndirectionSheet is null)
            {
                return comboIds.ToArray();
            }

            foreach (ActionIndirection indirectAction in actionIndirectionSheet)
            {
                LuminaAction? upgradedAction = indirectAction.Name.Value;
                LuminaAction? prevAction = indirectAction.PreviousComboAction.Value;
                if (upgradedAction is not null && prevAction is not null && baseComboId == prevAction.RowId)
                {
                    comboIds.Add(upgradedAction.RowId);
                }
            }

            return comboIds.ToArray();
        }
    }

    public class DataSource
    {
        public uint TriggerId;
        public bool Active;
        public bool InRange;
        public bool InLos;
        public bool ComboActive;
        public float Value;
        public int Stacks;
        public int MaxStacks;
        public ushort Icon;

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

    [StructLayout(LayoutKind.Explicit, Size = 0x8)]
    public struct Combo {
        [FieldOffset(0x00)] public float Timer;
        [FieldOffset(0x04)] public uint Action;
    }

    public struct TriggerData
    {
        public string Name;
        public uint Id;
        public ushort Icon;
        public byte MaxStacks;
        
        [JsonConverter(typeof(ComboIdConverter))]
        public uint[] ComboId;

        public TriggerData(string name, uint id, ushort icon, byte maxStacks = 0, uint[]? comboId = null)
        {
            Name = name;
            Id = id;
            Icon = icon;
            MaxStacks = maxStacks;
            ComboId = comboId ?? new uint[0];
        }
    }

    public struct RecastInfo
    {
        public float RecastTime;
        public float RecastTimeElapsed;
        public ushort MaxCharges;

        public RecastInfo(float recastTime, float recastTimeElapsed, ushort maxCharges)
        {
            RecastTime = recastTime;
            RecastTimeElapsed = recastTimeElapsed;
            MaxCharges = maxCharges;
        }
    }
}
