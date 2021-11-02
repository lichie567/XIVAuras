namespace XIVAuras.Config
{
    public interface IConfigPage
    {
        string Name { get; }

        void DrawConfig();
    }
}
