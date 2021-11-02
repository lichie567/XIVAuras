using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraGroup : IAuraListItem
    {
        public AuraType Type => AuraType.Group;

        public string Name { get; init; }

        public AuraListConfig AuraList { get; set; }

        // Constructor for deserialization
        public AuraGroup() : this(string.Empty) { }

        public AuraGroup(string name)
        {
            this.Name = name;
            this.AuraList = new AuraListConfig();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.AuraList;
        }

        public void Draw(Vector2 pos)
        {
            foreach (IAuraListItem aura in this.AuraList.Auras)
            {
                aura.Draw(pos);
            }
        }
    }
}