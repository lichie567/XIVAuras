using System.Numerics;

namespace XIVAuras.Auras
{
    public interface IAuraListItem
    {
        AuraType Type { get; }

        string Name { get; }

        void Draw(Vector2 pos);
    }

    public enum AuraType
    {
        Group,
        Icon,
        Bar
    }
}