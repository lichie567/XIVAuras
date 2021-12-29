using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public enum TriggerCond
    {
        And,
        Or,
        Xor
    }

    public enum TriggerDataSource
    {
        Value,
        Stacks,
        MaxStacks
    }

    public enum TriggerDataOp
    {
        Equals,
        NotEquals,
        LessThan,
        GreaterThan,
        LessThanEq,
        GreaterThanEq
    }

    public abstract class TriggerOptions
    {
        [JsonIgnore] public static readonly string[] OperatorOptions = new string[] { "==", "!=", "<", ">", "<=", ">=" };

        public TriggerCond Condition = TriggerCond.And;
        public List<TriggerData> TriggerData { get; protected set; } = new List<TriggerData>();

        public abstract TriggerType Type { get; }
        public abstract TriggerSource Source { get; }
        public abstract bool IsTriggered(bool preview, out DataSource data);
        public abstract void DrawTriggerOptions(Vector2 size, float padX, float padY);

        protected static bool GetResult(
            DataSource data,
            TriggerDataSource source,
            TriggerDataOp op,
            float value)
        {
                
            float dataValue = data.GetDataForSourceType(source);

            return op switch
            {
                TriggerDataOp.Equals => dataValue == value,
                TriggerDataOp.NotEquals => dataValue != value,
                TriggerDataOp.LessThan => dataValue < value,
                TriggerDataOp.GreaterThan => dataValue > value,
                TriggerDataOp.LessThanEq => dataValue <= value,
                TriggerDataOp.GreaterThanEq => dataValue >= value,
                _ => false
            };
        }
    }
}