using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraLabel : AuraListItem
    {
        public override AuraType Type => AuraType.Label;

        public LabelStyleConfig LabelStyleConfig { get; init; }

        // Constuctor for deserialization
        public AuraLabel() : this(string.Empty) { }

        public AuraLabel(string name) : base(name)
        {
            this.Name = name;
            this.LabelStyleConfig = new LabelStyleConfig();
        }

        public override IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.LabelStyleConfig;
        }

        public override void Draw(Vector2 pos)
        {

        }
    }
}