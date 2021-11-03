using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public abstract class AuraListItem : IConfigurable
    {
        public string Name { get; set; }

        public AuraListItem(string name)
        {
            this.Name = name;
        }

        public abstract AuraType Type { get; }

        public abstract void Draw(Vector2 pos);

        public abstract IEnumerator<IConfigPage> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string? ToString() => $"{this.Type} [{this.Name}]";
    }

    public enum AuraType
    {
        Group,
        Icon,
        Bar
    }
}