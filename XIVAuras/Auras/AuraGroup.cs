using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraGroup : AuraListItem, IAuraGroup
    {
        public override AuraType Type => AuraType.Group;

        public AuraListConfig AuraList { get; set; }

        // Constructor for deserialization
        public AuraGroup() : this(string.Empty) { }

        public AuraGroup(string name) : base(name)
        {
            this.AuraList = new AuraListConfig();
        }

        public override IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.AuraList;
        }

        public override void Draw(Vector2 pos)
        {
            foreach (AuraListItem aura in this.AuraList.Auras)
            {
                aura.Draw(pos);
            }
        }
    }
}