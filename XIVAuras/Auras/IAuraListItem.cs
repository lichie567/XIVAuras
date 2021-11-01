using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public interface IAuraListItem
    {
        AuraType Type { get; }

        string Name { get; }

        IEnumerable<IConfigPage> ConfigPages { get; }

        void Draw(Vector2 pos);
    }

    public enum AuraType
    {
        Group,
        Icon,
        Bar
    }
}