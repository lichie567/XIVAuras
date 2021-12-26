using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public enum TriggerStatus
    {
        Active,
        NotActive
    }

    public abstract class TriggerOptions
    {
        public TriggerCond Condition = TriggerCond.And;
        public TriggerStatus Status = TriggerStatus.Active;
        public List<TriggerData> TriggerData { get; protected set; } = new List<TriggerData>();

        public abstract TriggerType Type { get; }
        public abstract TriggerSource Source { get; }
        public abstract bool IsTriggered(bool preview, out DataSource? data);
        public abstract void DrawTriggerOptions(Vector2 size, float padX, float padY);
    }
}