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

        public DataSource UpdatePreviewData(DataSource data)
        {
            if (this.StartTime.HasValue && this.StartData.HasValue)
            {
                float secondSinceStart = (float)(DateTime.UtcNow - this.StartTime.Value).TotalSeconds;
                float resetValue = Math.Min(this.StartData.Value.Duration, this.StartData.Value.Cooldown);
                float newValue = resetValue - secondSinceStart;

                if (newValue < 0)
                {
                    this.StartTime = DateTime.UtcNow;
                    newValue = resetValue;
                }

                return new DataSource()
                {
                    Cooldown = newValue,
                    Duration = newValue
                };
            }

            return data;
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