using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public class AuraBar : IAuraListItem
    {
        public AuraType Type => AuraType.Bar;

        public string Name { get; init; }

        public IEnumerable<IConfigPage> ConfigPages { get; init; }

        public AuraBar(string name)
        {
            this.Name = name;
            this.ConfigPages = new List<IConfigPage>();
        }

        public void Draw(Vector2 pos)
        {

        }
    }
}