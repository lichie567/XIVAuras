using System.Numerics;

namespace XIVAuras.Config
{
    public interface IConfigPage
    {
        string Name { get; }

        IConfigPage GetDefault();
        void DrawConfig(Vector2 size, float padX, float padY);
    }
}
