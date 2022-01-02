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
        private const float NavBarHeight = 40;

        private bool _back = false;
        private bool _home = false;
        private string _name = string.Empty;
        private Vector2 _windowSize;
        private Stack<IConfigurable> _configStack;

        public ConfigWindow(string id, Vector2 position, Vector2 size) : base(id)
        {
            this.Flags =
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoSavedSettings;

            this.Position = position - size / 2;
            this.PositionCondition = ImGuiCond.Appearing;
            this.SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = new(size.X, 160),
                MaximumSize = ImGui.GetMainViewport().Size
            };

            _windowSize = size;
            _configStack = new Stack<IConfigurable>();
        }

        public void PushConfig(IConfigurable configItem)
        {
            _configStack.Push(configItem);
            _name = configItem.Name;
            this.IsOpen = true;
        }

        public override void PreDraw()
        {
            if (_configStack.Any())
            {
                this.WindowName = this.GetWindowTitle();
                ImGui.SetNextWindowSize(_windowSize);
            }
        }

        public override void Draw()
        {
            if (!_configStack.Any())
            {
                this.IsOpen = false;
                return;
            }

            IConfigurable configItem = _configStack.Peek();
            Vector2 spacing = ImGui.GetStyle().ItemSpacing;
            Vector2 size = _windowSize - spacing * 2;
            bool drawNavBar = _configStack.Count > 1;

            if (drawNavBar)
            {
                size -= new Vector2(0, NavBarHeight + spacing.Y);
            }

            IConfigPage? openPage = null;
            if (ImGui.BeginTabBar($"##{this.WindowName}"))
            {
                foreach (IConfigPage page in configItem.GetConfigPages())
                {
                    if (ImGui.BeginTabItem($"{page.Name}##{this.WindowName}"))
                    {
                        openPage = page;
                        page.DrawConfig(size.AddY(-ImGui.GetCursorPosY()), spacing.X, spacing.Y);
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }

            if (drawNavBar)
            {
                this.DrawNavBar(openPage, size, spacing.X);
            }

            this.Position = ImGui.GetWindowPos();
            _windowSize = ImGui.GetWindowSize();
        }

        private void DrawNavBar(IConfigPage? openPage, Vector2 size, float padX)
        {
            Vector2 buttonsize = new Vector2(40, 0);
            float textInputWidth = 150;

            if (ImGui.BeginChild($"##{this.WindowName}_NavBar", new Vector2(size.X, NavBarHeight), true))
            {
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.LongArrowAltLeft, () => _back = true, "Back", buttonsize);
                ImGui.SameLine();

                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Home, () => _home = true, "Home", buttonsize);
                ImGui.SameLine();

                // calculate empty horizontal space based on size of 5 buttons and text box
                float offset = size.X - buttonsize.X * 5 - textInputWidth - padX * 7;

                if (_configStack.Peek() is AuraListItem aura)
                {
                    offset -= 80;
                    ImGui.Checkbox("Preview", ref aura.Preview);
                    ImGui.SameLine();
                }

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.UndoAlt, () => Reset(openPage), $"Reset {openPage?.Name} Options to Defaults", buttonsize);
                ImGui.SameLine();

                ImGui.PushItemWidth(textInputWidth);
                if (ImGui.InputText("##Input", ref _name, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    Rename(_name);
                }
                ImGui.PopItemWidth();
                
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Rename");
                }

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => Export(openPage), $"Export {openPage?.Name} Options", buttonsize);

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Download, () => Import(), $"Import {openPage?.Name} Options", buttonsize);
            }

            ImGui.EndChild();
        }

        private string GetWindowTitle()
        {
            string title = string.Empty;
            title = string.Join("  >  ", _configStack.Reverse().Select(c => c.ToString()));
            return title;
        }

        private void Reset(IConfigPage? openPage)
        {
            if (openPage is not null)
            {
                _configStack.Peek().ImportPage(openPage.GetDefault());
            }
        }

        private void Export(IConfigPage? openPage)
        {
            if (openPage is not null)
            {
                ConfigHelpers.ExportToClipboard<IConfigPage>(openPage);
            }
        }

        private void Import()
        {
            string importString = ImGui.GetClipboardText();
            IConfigPage? page = ConfigHelpers.GetFromImportString<IConfigPage>(importString);

            if (page is not null)
            {
                _configStack.Peek().ImportPage(page);
            }
        }

        private void Rename(string name)
        {
            if (_configStack.Any())
            {
                _configStack.Peek().Name = name;
            }
        }

        public override void PostDraw()
        {
            if (_home)
            {
                while (_configStack.Count > 1)
                {
                    _configStack.Pop();
                }
            }
            else if (_back)
            {
                _configStack.Pop();
            }

            if ((_home || _back) && _configStack.Count > 1)
            {
                _name = _configStack.Peek().Name;
            }

            _home = false;
            _back = false;
        }

        public override void OnClose()
        {
            ConfigHelpers.SaveConfig();

            var config = Singletons.Get<XIVAurasConfig>();
            foreach (AuraListItem aura in config.AuraList.Auras)
            {
                aura.StopPreview();
            }

            _configStack.Clear();
        }
    }
}