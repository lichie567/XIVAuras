using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public class AuraBar : AuraListItem
    {
        public override AuraType Type => AuraType.Bar;

        public BarStyleConfig BarStyleConfig { get; init; }

        public TriggerConfig TriggerConfig { get; init; }

        public VisibilityConfig VisibilityConfig { get; init; }

        // Constuctor for deserialization
        public AuraBar() : this(string.Empty) { }

        public AuraBar(string name) : base(name)
        {
            this.Name = name;
            this.BarStyleConfig = new BarStyleConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.BarStyleConfig;
            yield return this.TriggerConfig;
            yield return this.VisibilityConfig;
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {

        }
    }
}