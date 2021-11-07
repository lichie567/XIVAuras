using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public enum TriggerCond
    {
        None,
        And,
        Or,
        Xor
    }

    public enum TriggerDataSource
    {
        None,
        Duration,
        Stacks,
        Cooldown
    }

    public enum TriggerDataOp
    {
        None,
        Equals,
        NotEquals,
        LessThan,
        GreaterThan,
        LessThanEq,
        GreaterThanEq
    }

    public class TriggerCondition
    {
        public static readonly string[] CondOptions = new string[] { "", "AND", "OR", "XOR" };
        public static readonly string[] SourceOptions = new string[] { "", "Duration", "Stacks", "Cooldown" };
        public static readonly string[] OperatorOptions = new string[] { "", "==", "!=", "<", ">", "<=", ">=" };

        public TriggerCond Cond = TriggerCond.None;
        public TriggerDataSource Source = TriggerDataSource.None;
        public TriggerDataOp Op = TriggerDataOp.None;
        public float Value = 0;

        public bool GetResult(DataSource data)
        {
            float value = this.Source switch
            {
                TriggerDataSource.Duration => data.Duration,
                TriggerDataSource.Stacks => data.Stacks,
                TriggerDataSource.Cooldown => data.Cooldown,
                _ => 0
            };

            return this.Op switch
            {
                TriggerDataOp.Equals => value == this.Value,
                TriggerDataOp.NotEquals => value != this.Value,
                TriggerDataOp.LessThan => value < this.Value,
                TriggerDataOp.GreaterThan => value > this.Value,
                TriggerDataOp.LessThanEq => value <= this.Value,
                TriggerDataOp.GreaterThanEq => value >= this.Value,
                _ => false
            };
        }
    }
}