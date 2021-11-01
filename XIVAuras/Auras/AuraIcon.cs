using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public class AuraIcon : IAuraListItem
    {
        public AuraType Type => AuraType.Icon;

        public string Name { get; init; }

        public IEnumerable<IConfigPage> ConfigPages { get; init; }

        public AuraIcon(string name)
        {
            this.Name = name;
            this.ConfigPages = new List<IConfigPage>
            { 
                new IconStyleOptions(),
                new TriggerOptions(),
                new VisibilityOptions() 
            };
        }

        public void Draw(Vector2 pos)
        {

        }
    }
}