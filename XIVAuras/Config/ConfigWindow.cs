using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class ConfigWindow : Window
    {
        public XIVAurasConfig Config { get; private set; }

        private AuraType _selectedType = AuraType.Group;
        private string _nameInput = string.Empty;
        private string[] _options = Enum.GetNames(typeof(AuraType));

        public ConfigWindow(XIVAurasConfig config) : base("XIVAuras")
        {
            this.Flags = ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoScrollWithMouse;

            this.Size = new Vector2(600, 600);

            this.Config = config;
        }

        public override void Draw()
        {
            if (!ImGui.BeginTabBar("##XIVAuras_Settings"))
            {
                return;
            }

            if (ImGui.BeginTabItem("Auras##XIVAuras_Auras"))
            {
                this.DrawCreateMenu();
                this.DrawAuraTable();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        private void DrawCreateMenu()
        {
            if (ImGui.BeginChild("##Buttons", new Vector2(584, 40), true))
            {
                ImGui.PushItemWidth(200);
                ImGui.InputTextWithHint("##Name", "New Aura Name", ref _nameInput, 30);

                ImGui.SameLine();
                ImGui.Combo("##Type", ref Unsafe.As<AuraType, int>(ref _selectedType), _options, _options.Length);

                ImGui.SameLine();
                DrawHelpers.DrawButton("Create", FontAwesomeIcon.Plus, () => CreateAura(_selectedType, _nameInput), "Create new Aura or Group");

                ImGui.SameLine();
                DrawHelpers.DrawButton("Import", FontAwesomeIcon.Download, () => ImportAura(string.Empty), "Import new Aura or Group");

                ImGui.PopItemWidth();
                ImGui.EndChild();
            }

        }

        private void DrawAuraTable()
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            if (ImGui.BeginTable("##Auras_Table", 3, flags, new Vector2(584, 484)))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 40, 0);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 25, 1);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthStretch, 38, 2);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < Config.Auras.Count; i++)
                {
                    IAuraListItem aura = Config.Auras[i];

                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 30);

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.Text(aura.Name);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.Text(aura.Type.ToString());
                    }

                    if (ImGui.TableSetColumnIndex(2))
                    {
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => EditAura(aura), "Edit");

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Upload, () => ExportAura(aura), "Export");

                        ImGui.SameLine();
                        DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => DeleteAura(aura), "Delete");
                    }
                }

                ImGui.EndTable();
            }
        }
        
        private void CreateAura(AuraType type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                IAuraListItem? newAura = type switch
                {
                    AuraType.Group => new AuraGroup(name),
                    AuraType.Icon => new IconAura(name),
                    AuraType.Bar => new BarAura(name),
                    _ => null
                };

                if (newAura is not null)
                {
                    this.Config.AddAura(newAura);
                    this.EditAura(newAura);
                }
            }

            this._nameInput = string.Empty;
        }

        private void EditAura(IAuraListItem aura)
        {

        }

        private void ImportAura(string importString)
        {
            IAuraListItem? newAura = XIVAurasConfig.GetAuraFromImportString(importString);

            if (newAura is not null)
            {
                this.Config.AddAura(newAura);
            }
        }

        private void ExportAura(IAuraListItem aura)
        {
            string exportString = XIVAurasConfig.GetAuraExportString(aura);
            ImGui.SetClipboardText(exportString);
        }

        private void DeleteAura(IAuraListItem aura)
        {
            this.Config.DeleteAura(aura);
        }

        public override void OnClose()
        {
            XIVAurasConfig.SaveConfig(this.Config);
        }
    }
}