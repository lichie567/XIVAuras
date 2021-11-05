using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Linq;
using XIVAuras.Config;

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

        public static DataSource? GetData(TriggerSource source, uint statusId)
        {
            ClientState clientState = Singletons.Get<ClientState>();
            TargetManager targetManager = Singletons.Get<TargetManager>();

            GameObject? player = clientState.LocalPlayer;
            GameObject? target = targetManager.SoftTarget ?? targetManager.Target;

            GameObject? actor = source switch
            {
                TriggerSource.Player => player,
                TriggerSource.Target => target,
                TriggerSource.TargetOfTarget => FindTargetOfTarget(player, target),
                TriggerSource.FocusTarget => targetManager.FocusTarget,
                _ => null
            };

            if (actor is not BattleChara chara)
            {
                return null;
            }

            return new DataSource()
            {
                Duration = Math.Abs(chara.StatusList.FirstOrDefault(o => o.StatusId == statusId)?.RemainingTime ?? 0f),
            };
        }

        // TODO: move this
        public static GameObject? FindTargetOfTarget(GameObject? player, GameObject? target)
        {
            if (target == null)
            {
                return null;
            }

            if (target.TargetObjectId == 0 && player != null && player.TargetObjectId == 0)
            {
                return player;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            ObjectTable objectTable = Singletons.Get<ObjectTable>();
            for (int i = 0; i < 200; i += 2)
            {
                GameObject? actor = objectTable[i];
                if (actor?.ObjectId == target.TargetObjectId)
                {
                    return actor;
                }
            }

            return null;
        }
    }
}
