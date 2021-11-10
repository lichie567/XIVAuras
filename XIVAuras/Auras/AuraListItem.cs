using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    public abstract class AuraListItem : IConfigurable
    {
        [JsonIgnore] public readonly string ID;
        [JsonIgnore] protected bool LastFrameWasPreview = false;
        [JsonIgnore] protected bool LastFrameWasDragging = false;
        [JsonIgnore] public bool Preview = false;
        [JsonIgnore] protected DataSource? StartData = null;
        [JsonIgnore] protected DateTime? StartTime = null;

        public string Name { get; set; }

        public AuraListItem(string name)
        {
            this.Name = name;
            this.ID = $"XIVAuras_{GetType().Name}_{Guid.NewGuid()}";
        }

        public abstract AuraType Type { get; }

        public abstract void Draw(Vector2 pos, Vector2? parentSize = null);

        public abstract IEnumerator<IConfigPage> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string? ToString() => $"{this.Type} [{this.Name}]";

        protected DataSource UpdatePreviewData(DataSource data)
        {
            if (this.StartTime.HasValue && this.StartData is not null)
            {
                float secondSinceStart = (float)(DateTime.UtcNow - this.StartTime.Value).TotalSeconds;
                float resetValue = Math.Min(this.StartData.Value, this.StartData.Value);
                float newValue = resetValue - secondSinceStart;

                if (newValue < 0)
                {
                    this.StartTime = DateTime.UtcNow;
                    newValue = resetValue;
                }

                return new DataSource()
                {
                    Value = newValue,
                    ChargeTime = data.ChargeTime,
                    Stacks = data.Stacks
                };
            }

            return data;
        }

        protected void UpdateStartData(DataSource data, TriggerType type)
        {
            if (this.StartData is not null)
            {
                float startValue = type == TriggerType.Cooldown
                    ? this.StartData.ChargeTime
                    : this.StartData.Value;

                float value = type == TriggerType.Cooldown
                    ? data.ChargeTime
                    : data.Value;

                if (value > startValue)
                {
                    this.StartData = data;
                    this.StartTime = DateTime.UtcNow;
                }
            }

            if (this.StartData is null || !this.StartTime.HasValue)
            {
                this.StartData = data;
                this.StartTime = DateTime.UtcNow;
                return;
            }
        }
    }

    public enum AuraType
    {
        Group,
        Icon,
        Bar,
        Label
    }
}