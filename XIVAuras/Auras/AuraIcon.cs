using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraIcon : AuraListItem
    {
        public override AuraType Type => AuraType.Icon;

        public IconStyleConfig IconStyleConfig { get; init; }

        public TriggerConfig TriggerConfig { get; init; }

        public VisibilityConfig VisibilityConfig { get; init; }

        // Constructor for deserialization
        public AuraIcon() : this(string.Empty) { }

        public AuraIcon(string name) : base(name)
        {
            this.Name = name;
            this.IconStyleConfig = new IconStyleConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.IconStyleConfig;
            yield return this.TriggerConfig;
            yield return this.VisibilityConfig;
        }

        public override void Draw(Vector2 pos)
        {
            if (TriggerConfig.TriggerType == TriggerType.Buff &&
                TriggerConfig.TriggerSource == TriggerSource.Player)
            {
                PlayerCharacter? player = Singletons.Get<ClientState>().LocalPlayer;
                if (player is not null)
                {
                    uint statusId = TriggerConfig.StatusId;
                    float duration = player.StatusList.FirstOrDefault(o => o.StatusId == statusId && o.RemainingTime > 0f)?.RemainingTime ?? 0f;

                    if (duration > 0)
                    {
                        Vector2 position = pos + this.IconStyleConfig.Position;
                        Vector2 size = this.IconStyleConfig.Size;
                        DrawHelpers.DrawInWindow($"AuraIcon_{Name}", position, size, false, false, (drawList) =>
                        {
                            string text = $"{Math.Truncate(duration)}";
                            Vector2 textSize = ImGui.CalcTextSize(text);
                            drawList.AddRectFilled(position, position + size, IconStyleConfig.BorderColor.Base);
                            DrawHelpers.DrawOutlinedText(text, position + size / 2 - textSize / 2, 0xFFFFFFFF, 0xFF000000, drawList);
                        });
                    }
                }
            }
        }
    }
}