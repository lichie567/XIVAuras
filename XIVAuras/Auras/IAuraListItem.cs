using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public interface IAuraListItem : IConfigurable
    {
        AuraType Type { get; }

        void Draw(Vector2 pos);
    }

    public enum AuraType
    {
        Group,
        Icon,
        Bar
    }
}