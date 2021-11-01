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

    public class TriggerOptions : IConfigPage
    {

        public string Name => "Trigger";

        public ActorType ActorType { get; set; }

        public uint StatusId;

        public bool IsTriggered()
        {
            return false;
        }

        public void DrawOptions()
        {
            ImGui.Text("TODO");
        }
    }
}