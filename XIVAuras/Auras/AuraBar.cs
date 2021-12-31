using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public class AuraBar : AuraListItem
    {
        public override AuraType Type => AuraType.Bar;

        public BarStyleConfig BarStyleConfig { get; set; }

        public TriggerConfig TriggerConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

        // Constuctor for deserialization
        public AuraBar() : this(string.Empty) { }

        public AuraBar(string name) : base(name)
        {
            this.Name = name;
            this.BarStyleConfig = new BarStyleConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case BarStyleConfig newPage:
                    this.BarStyleConfig = newPage;
                    break;
                case TriggerConfig newPage:
                    this.TriggerConfig = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
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