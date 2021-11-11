using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Config;

namespace XIVAuras.Auras
{
    public class AuraGroup : AuraListItem, IAuraGroup
    {
        public override AuraType Type => AuraType.Group;

        public AuraListConfig AuraList { get; set; }

        public GroupConfig GroupConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public AuraGroup() : this(string.Empty) { }

        public AuraGroup(string name) : base(name)
        {
            this.AuraList = new AuraListConfig();
            this.GroupConfig = new GroupConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.AuraList;
            yield return this.GroupConfig;
            yield return this.VisibilityConfig;
        }

        public override void StopPreview()
        {
            base.StopPreview();

            foreach (AuraListItem aura in this.AuraList.Auras)
            {
                aura.StopPreview();
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {
            foreach (AuraListItem aura in this.AuraList.Auras)
            {
                if (!this.Preview && this.LastFrameWasPreview)
                {
                    aura.Preview = false;
                }
                else
                {
                    aura.Preview |= this.Preview;
                }

                aura.Draw(pos + this.GroupConfig.Position);
            }

            this.LastFrameWasPreview = this.Preview;
        }
    }
}