using FFXIVClientStructs.FFXIV.Client.Game;
using System;

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
    }
}
