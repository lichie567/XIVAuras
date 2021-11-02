using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraIcon : IAuraListItem
    {
        public AuraType Type => AuraType.Icon;

        public string Name { get; init; }

        public IconStyleConfig IconStyleConfig { get; init; }

        public TriggerConfig TriggerConfig { get; init; }

        public VisibilityConfig VisibilityConfig { get; init; }

        // Constructor for deserialization
        public AuraIcon() : this(string.Empty) { }

        public AuraIcon(string name)
        {
            this.Name = name;
            this.IconStyleConfig = new IconStyleConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.IconStyleConfig;
            yield return this.TriggerConfig;
            yield return this.VisibilityConfig;
        }

        public void Draw(Vector2 pos)
        {

        }
    }
}