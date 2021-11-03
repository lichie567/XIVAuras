using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Windows
{
    public class ConfigWindow : Window
    {
        private Stack<IConfigurable> ConfigStack { get; init; }

        private bool _back = false;
        private bool _home = false;
        private string _name = string.Empty;

        public ConfigWindow(string id, Vector2 position, Vector2 size) : base(id)
        {
            this.Flags =
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings;

            this.Size = size;
            this.Position = position - size / 2;
            this.PositionCondition = ImGuiCond.Appearing;
            this.ConfigStack = new Stack<IConfigurable>();
        }

        public void PushConfig(IConfigurable configItem)
        {
            this.ConfigStack.Push(configItem);
            this._name = configItem.Name;
            this.IsOpen = true;
        }

        public void Close()
        {
            this.IsOpen = false;
            this.ConfigStack.Clear();
        }

        public override void PreDraw()
        {
            if (this.ConfigStack.Any())
            {
                this.WindowName = this.ConfigStack.Peek().ToString() ?? "XIVAuras";
            }
        }

        public override void Draw()
        {
            if (!this.ConfigStack.Any())
            {
                this.IsOpen = false;
                return;
            }

            IConfigurable configItem = this.ConfigStack.Peek();
            Vector2 size = this.Size ?? Vector2.Zero;

            if (this.ConfigStack.Count > 1)
            {
                size -= new Vector2(0, 40);
            }

            if (ImGui.BeginTabBar($"##{this.WindowName}"))
            {
                foreach (IConfigPage page in configItem)
                {
                    if (ImGui.BeginTabItem($"{page.Name}##{this.WindowName}"))
                    {
                        page.DrawConfig(size);
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }

            if (this.ConfigStack.Count > 1)
            {
                this.DrawNavBar(size);
            }

            this.Position = ImGui.GetWindowPos();
        }

        private void DrawNavBar(Vector2 size)
        {
            if (ImGui.BeginChild($"##{this.WindowName}_NavBar", new Vector2(size.X - 16, 40), true))
            {
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.LongArrowAltLeft, () => _back = true, "Back", new Vector2(40, 0));

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Home, () => _home = true, "Home", new Vector2(40, 0));

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + size.X - 422);
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => Export(), "Export", new Vector2(40, 0));

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => Delete(), "Delete", new Vector2(40, 0));

                ImGui.SameLine();
                ImGui.PushItemWidth(150);
                ImGui.InputText("##Input", ref _name, 64);
                ImGui.PopItemWidth();

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Check, () => Rename(_name), "Rename", new Vector2(40, 0));

                ImGui.EndChild();
            }
        }

        private void Export()
        {
            if (this.ConfigStack.Any() &&
                this.ConfigStack.Peek() is AuraListItem aura)
            {
                ConfigHelpers.ExportAuraToClipboard(aura);
            }
        }

        private void Delete()
        {
            AuraListItem? aura = this.ConfigStack.Pop() as AuraListItem;
            if (aura is not null && this.ConfigStack.Any())
            {
                IAuraGroup? auraGroup = this.ConfigStack.Peek() as IAuraGroup;
                auraGroup?.AuraList.Auras.Remove(aura);
            }
        }

        private void Rename(string name)
        {
            if (this.ConfigStack.Any())
            {
                this.ConfigStack.Peek().Name = name;
            }
        }

        public override void PostDraw()
        {
            if (this._home)
            {
                while (this.ConfigStack.Count > 1)
                {
                    this.ConfigStack.Pop();
                }
            }
            else if (this._back)
            {
                this.ConfigStack.Pop();
            }

            this._home = false;
            this._back = false;
        }

        public override void OnClose()
        {
            ConfigHelpers.SaveConfig();
            this.Close();
        }
    }
}