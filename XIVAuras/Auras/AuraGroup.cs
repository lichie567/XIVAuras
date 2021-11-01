using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XIVAuras.Auras
{
    public class AuraGroup : IAuraListItem
    {

        public AuraType Type => AuraType.Group;

        public string Name { get; private set; }

        public List<IAuraListItem> Auras { get; set; }

        public AuraGroup(string name)
        {
            this.Auras = new List<IAuraListItem>();
            this.Name = name;
        }

        public void Draw(Vector2 pos)
        {
            foreach (IAuraListItem aura in this.Auras)
            {
                aura.Draw(pos);
            }
        }
    }
}