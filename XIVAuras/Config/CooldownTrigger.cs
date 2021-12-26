using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Dalamud.Interface;
using ImGuiNET;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class CooldownTrigger : TriggerOptions
    {
        public override TriggerType Type => TriggerType.Cooldown;
        public override TriggerSource Source => TriggerSource.Player;

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            
        }

        public static TriggerOptions GetDefault()
        {
            return new CooldownTrigger();
        }

        public override bool IsTriggered(bool preview, out DataSource? data)
        {
            data = null;
            return false;
        }
    }
}