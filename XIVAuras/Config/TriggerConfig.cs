using ImGuiNET;

namespace XIVAuras.Config
{
    public enum ActorType
    {
        Player,
        Target,
        TargetOfTarget,
        Focus,
    }

    public enum TriggerType
    {
        Buff,
        Debuff,
        Cooldown
    }

    public class TriggerConfig : IConfigPage
    {

        public string Name => "Trigger";

        public ActorType ActorType { get; set; }

        public uint StatusId;

        public bool IsTriggered()
        {
            return false;
        }

        public void DrawConfig()
        {
            ImGui.Text("TODO");
        }
    }
}