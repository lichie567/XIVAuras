using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Newtonsoft.Json;

namespace XIVAuras.Config
{
    public enum TriggerType
    {
        Buff,
        Debuff,
        Cooldown
    }

    public enum TriggerSource
    {
        Player,
        Target,
        TargetOfTarget,
        Focus,
    }

    public enum TriggerJobTypes
    {
        All,
        Tanks,
        Casters,
        Melee,
        Ranged,
        DoW,
        DoM,
        Crafters,
        DoH,
        DoL,
        Custom
    }

    public class TriggerConfig : IConfigPage
    {
        [JsonIgnore] public string Name => "Trigger";

        [JsonIgnore] private string[] _options = Enum.GetNames(typeof(TriggerType));

        [JsonIgnore] private int _statusIdInput = -1;

        public TriggerType TriggerType = TriggerType.Buff;

        public TriggerSource TriggerSource = TriggerSource.Player;

        public uint StatusId = 0;

        public bool IsTriggered()
        {
            return false;
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##TriggerConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Combo("Trigger Type", ref Unsafe.As<TriggerType, int>(ref this.TriggerType), _options, _options.Length);

                string[] sourceOptions = this.TriggerType switch
                {
                    TriggerType.Cooldown => new[] { TriggerSource.Player.ToString() },
                    _ => Enum.GetNames(typeof(TriggerSource))
                };

                ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), sourceOptions, sourceOptions.Length);

                if (_statusIdInput == -1)
                {
                    _statusIdInput = (int)this.StatusId;
                }

                if (ImGui.InputInt("Ability Id", ref _statusIdInput, 0, 0, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    this.StatusId = (uint)_statusIdInput;
                }

                ImGui.EndChild();
            }
        }
    }
}