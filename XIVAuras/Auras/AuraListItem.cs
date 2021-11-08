using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public abstract class AuraListItem : IConfigurable
    {
        [JsonIgnore] protected bool LastFrameWasPreview = false;
        [JsonIgnore] protected bool LastFrameWasDragging = false;
        [JsonIgnore] public bool Preview = false;
        [JsonIgnore] public readonly string ID;

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
    }

    public enum AuraType
    {
        Group,
        Icon,
        Bar,
        Label
    }
}