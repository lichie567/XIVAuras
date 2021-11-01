using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{

    public abstract class Aura : IAuraListItem
    {
        public AuraType Type { get; init; }

        public string Name { get; init; }

        public StyleOptions StyleOptions { get; init; }

        public TriggerOptions TriggerOptions { get; init; }

        public VisibilityOptions VisibilityOptions { get; init; }

        [JsonIgnore]
        protected GameObject? Actor;

        public Aura(string name, AuraType type)
        {
            this.Name = name;
            this.Type = type;

            this.StyleOptions = new StyleOptions();
            this.TriggerOptions = new TriggerOptions();
            this.VisibilityOptions = new VisibilityOptions();
        }

        public abstract void Draw(Vector2 pos);
    }
}