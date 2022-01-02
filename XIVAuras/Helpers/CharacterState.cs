using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using System.Linq;
using Dalamud.Game.ClientState;

namespace XIVAuras.Helpers
{

    public static class CharacterState
    {
        private static readonly uint[] _goldenSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        public static bool IsCharacterBusy()
        {
            Condition condition = Singletons.Get<Condition>();
            return condition[ConditionFlag.WatchingCutscene] ||
                condition[ConditionFlag.WatchingCutscene78] ||
                condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                condition[ConditionFlag.CreatingCharacter] ||
                condition[ConditionFlag.BetweenAreas] ||
                condition[ConditionFlag.BetweenAreas51] ||
                condition[ConditionFlag.OccupiedSummoningBell];
        }

        public static bool IsInCombat()
        {
            Condition condition = Singletons.Get<Condition>();
            return condition[ConditionFlag.InCombat];
        }

        public static bool IsInDuty()
        {
            Condition condition = Singletons.Get<Condition>();
            return condition[ConditionFlag.BoundByDuty];
        }

        public static bool IsPerforming()
        {
            Condition condition = Singletons.Get<Condition>();
            return condition[ConditionFlag.Performing];
        }

        public static bool IsInGoldenSaucer()
        {
            return _goldenSaucerIDs.Any(id => id == Singletons.Get<ClientState>().TerritoryType);
        }

        public static bool IsJob(IEnumerable<Job> jobs)
        {
            var player = Singletons.Get<ClientState>().LocalPlayer;
            if (player is null)
            {
                return false;
            }

            return jobs.Contains((Job)player.ClassJob.Id);
        }

        public static List<Job> GetJobsForJobType(JobType type)
        {
            switch (type)
            {
                case JobType.All:
                    return Enum.GetValues<Job>().ToList();
                case JobType.Tanks:
                    return new List<Job>() { Job.GLA, Job.MRD, Job.PLD, Job.WAR, Job.DRK, Job.GNB };
                case JobType.Casters:
                    return new List<Job>() { Job.THM, Job.ACN, Job.BLM, Job.SMN, Job.RDM, Job.BLU };
                case JobType.Melee:
                    return new List<Job>() { Job.PGL, Job.LNC, Job.ROG, Job.MNK, Job.DRG, Job.NIN, Job.SAM, Job.RPR };
                case JobType.Ranged:
                    return new List<Job>() { Job.ARC, Job.BRD, Job.MCH, Job.DNC };
                case JobType.Healers:
                    return new List<Job>() { Job.CNJ, Job.WHM, Job.SCH, Job.AST, Job.SGE };
                case JobType.DoH:
                    return new List<Job>() { Job.CRP, Job.BSM, Job.ARM, Job.GSM, Job.LTW, Job.WVR, Job.ALC, Job.CUL };
                case JobType.DoL:
                    return new List<Job>() { Job.MIN, Job.BOT, Job.FSH };
                case JobType.Combat:
                    List<Job> combatList = GetJobsForJobType(JobType.DoW);
                    combatList.AddRange(GetJobsForJobType(JobType.DoM));
                    return combatList;
                case JobType.DoW:
                    List<Job> dowList = GetJobsForJobType(JobType.Tanks);
                    dowList.AddRange(GetJobsForJobType(JobType.Melee));
                    dowList.AddRange(GetJobsForJobType(JobType.Ranged));
                    return dowList;
                case JobType.DoM:
                    List<Job> domList = GetJobsForJobType(JobType.Casters);
                    domList.AddRange(GetJobsForJobType(JobType.Healers));
                    return domList;
                case JobType.Crafters:
                    List<Job> crafterList = GetJobsForJobType(JobType.DoH);
                    crafterList.AddRange(GetJobsForJobType(JobType.DoL));
                    return crafterList;
                default:
                    return new List<Job>();
            }
        }
    }
}
