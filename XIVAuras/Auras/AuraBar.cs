using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraBar : IAuraListItem
    {
        public AuraType Type => AuraType.Bar;

        public string Name { get; init; }

        public BarStyleConfig BarStyleConfig { get; init; }

        public TriggerConfig TriggerConfig { get; init; }

        public VisibilityConfig VisibilityConfig { get; init; }

        // Constuctor for deserialization
        public AuraBar() : this(string.Empty) { }

        public AuraBar(string name)
        {
            this.Name = name;
            this.BarStyleConfig = new BarStyleConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.BarStyleConfig;
            yield return this.TriggerConfig;
            yield return this.VisibilityConfig;
        }

        public void Draw(Vector2 pos)
        {

        }
    }
}