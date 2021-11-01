using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVAuras.Config
{
    public enum ActorType
    {
        Player,
        Target,
        TargetOfTarget,
        Focus,
    }

    public enum TriggerType
    {
        Buff,
        Debuff,
        Cooldown
    }

    public class TriggerOptions
    {
        public ActorType ActorType { get; set; }

        public uint StatusId;

        public bool IsTriggered()
        {
            return false;
        }

        public void DrawTab()
        {

        }
    }
}