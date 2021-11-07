using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;

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
        FocusTarget,
    }

    public class TriggerConfig : IConfigPage
    {
        [JsonIgnore] public string Name => "Trigger";
        [JsonIgnore] private string[] _options = Enum.GetNames(typeof(TriggerType));
        [JsonIgnore] private string _triggerNameInput = string.Empty;
        [JsonIgnore] private TriggerCondition _inputTrigger = new TriggerCondition();
        [JsonIgnore] private string _triggerValueInput = string.Empty;
        [JsonIgnore] private string _iconIdInput = string.Empty;

        public TriggerType TriggerType = TriggerType.Buff;

        public TriggerSource TriggerSource = TriggerSource.Player;

        public List<TriggerCondition> TriggerConditions = new List<TriggerCondition>();

        public List<TriggerData> TriggerList = new List<TriggerData>();

        public string TriggerName = string.Empty;

        public bool ShowOnlyMine = true;

        public int IconPickerIndex = 0;

        public int IconOption = 0;

        public ushort CustomIcon = 0;

        public ushort GetIcon()
        {
            if (!this.TriggerList.Any())
            {
                return 0;
            }

            if (this.IconOption == 0)
            {
                return this.TriggerList[this.IconPickerIndex].Icon;
            }
            else
            {
                return this.CustomIcon;
            }
        }

        public bool IsTriggered(DataSource data)
        {
            if (!this.TriggerConditions.Any())
            {
                return false;
            }

            bool triggered = this.TriggerConditions[0].GetResult(data);
            for (int i = 1; i < this.TriggerConditions.Count; i++)
            {
                TriggerCondition current = this.TriggerConditions[i];

                triggered = current.Cond switch
                {
                    TriggerCond.And => triggered && current.GetResult(data),
                    TriggerCond.Or => triggered || current.GetResult(data),
                    TriggerCond.Xor => triggered ^ current.GetResult(data),
                    _ => false
                };
            }

            return triggered;
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##TriggerConfig", new Vector2(size.X, size.Y), true))
            {
                if(ImGui.Combo("Trigger Type", ref Unsafe.As<TriggerType, int>(ref this.TriggerType), _options, _options.Length))
                {
                    this.ResetTriggers();
                }

                string[] sourceOptions = this.TriggerType switch
                {
                    TriggerType.Cooldown => new[] { TriggerSource.Player.ToString() },
                    _ => Enum.GetNames(typeof(TriggerSource))
                };

                ImGui.Combo("Trigger Source", ref Unsafe.As<TriggerSource, int>(ref this.TriggerSource), sourceOptions, sourceOptions.Length);

                if (string.IsNullOrEmpty(this._triggerNameInput))
                {
                    this._triggerNameInput = this.TriggerName;
                }

                if (ImGui.InputTextWithHint("Trigger", "Ability Name or ID", ref _triggerNameInput, 32, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    this.ResetTriggers();
                    if (!string.IsNullOrEmpty(_triggerNameInput))
                    {
                        if (this.TriggerType == TriggerType.Cooldown)
                        {
                            SpellHelpers.FindActionEntries(this._triggerNameInput).ForEach(t => AddTriggerData(t));
                        }
                        else
                        {
                            SpellHelpers.FindStatusEntries(this._triggerNameInput).ForEach(t => AddTriggerData(t));
                        }
                    }

                    this._triggerNameInput = this.TriggerName;
                }

                if (this.TriggerType == TriggerType.Buff || this.TriggerType == TriggerType.Debuff)
                {
                    ImGui.Checkbox("Only Show My Effects", ref this.ShowOnlyMine);
                }

                if (this.TriggerList.Any())
                {
                    ImGui.RadioButton("Icon Picker", ref this.IconOption, 0);
                    ImGui.SameLine();
                    ImGui.RadioButton("Custom Icon", ref this.IconOption, 1);

                    if (this.IconOption == 0)
                    {
                        if (ImGui.BeginChild("##IconPicker", new Vector2(size.X - padX * 2, 60), true))
                        {
                            List<ushort> icons = this.TriggerList.Select(t => t.Icon).Distinct().ToList();
                            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                            for (int i = 0; i < icons.Count; i++)
                            {
                                Vector2 iconSize = new Vector2(40, 40);
                                Vector2 iconPos = ImGui.GetWindowPos().AddX(10) + new Vector2(i * (40 + padX), padY);
                                DrawHelpers.DrawIcon(icons[i], iconPos, iconSize, true, 0, drawList);
                                string iconText = icons[i].ToString();
                                Vector2 iconTextPos = iconPos + new Vector2(20 - ImGui.CalcTextSize(iconText).X / 2, 38);
                                drawList.AddText(iconTextPos, 0xFFFFFFFF, iconText);

                                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + iconSize))
                                {
                                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                                    {
                                        this.IconPickerIndex = i;
                                    }
                                }
                            }

                            Vector2 selectionPos = ImGui.GetWindowPos().AddX(10) + new Vector2(this.IconPickerIndex * (40 + padX), padY);
                            drawList.AddRect(selectionPos, selectionPos + new Vector2(40, 40), 0xFF00FF00);

                            ImGui.EndChild();
                        }
                    }
                    else if (this.IconOption == 1)
                    {
                        if (string.IsNullOrEmpty(this._iconIdInput) && this.CustomIcon != 0)
                        {
                            this._iconIdInput = this.CustomIcon.ToString();
                        }

                        if (ImGui.InputTextWithHint("Custom Icon Id", "Custom Icon Id", ref _iconIdInput, 10, ImGuiInputTextFlags.EnterReturnsTrue))
                        {
                            if (ushort.TryParse(this._iconIdInput, out ushort value))
                            {
                                this.CustomIcon = value;
                            }
                        }

                        if (this.CustomIcon != 0 && ImGui.BeginChild("##IconPicker", new Vector2(size.X - padX * 2, 60), true))
                        {
                            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                            Vector2 iconSize = new Vector2(40, 40);
                            Vector2 iconPos = ImGui.GetWindowPos() + new Vector2(0, padY);
                            DrawHelpers.DrawIcon(this.CustomIcon, iconPos, iconSize, true, 0, drawList);
                            string iconText = this.CustomIcon.ToString();
                            Vector2 iconTextPos = iconPos + new Vector2(20 - ImGui.CalcTextSize(iconText).X / 2, 38);
                            drawList.AddText(iconTextPos, 0xFFFFFFFF, iconText);
                            ImGui.EndChild();
                        }
                    }
                }

                DrawHelpers.DrawSpacing(1);
                ImGui.Text("Trigger Conditions");

                ImGuiTableFlags tableFlags =
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.BordersOuter |
                    ImGuiTableFlags.BordersInner |
                    ImGuiTableFlags.ScrollY |
                    ImGuiTableFlags.NoSavedSettings;

                if (ImGui.BeginTable("##Trigger_Table", 5, tableFlags, new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2)))
                {
                    ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthFixed, 60, 0);
                    ImGui.TableSetupColumn("Data Source", ImGuiTableColumnFlags.WidthFixed, 90, 1);
                    ImGui.TableSetupColumn("Operator", ImGuiTableColumnFlags.WidthFixed, 80, 2);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 0, 3);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 45, 4);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < this.TriggerConditions.Count + 1; i++)
                    {
                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                        this.DrawTriggerConditionRow(i);
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }
        }

        private void DrawTriggerConditionRow(int i)
        {
            TriggerCondition trigger = i < this.TriggerConditions.Count ? this.TriggerConditions[i] : _inputTrigger;

            if (ImGui.TableSetColumnIndex(0))
            {
                if (i == 0)
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                    ImGui.Text("IF");
                }
                else
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                    ImGui.PushItemWidth(ImGui.GetColumnWidth());
                    string[] options = TriggerCondition.CondOptions;
                    ImGui.Combo("##CondCombo", ref Unsafe.As<TriggerCond, int>(ref trigger.Cond), options, options.Length);
                    ImGui.PopItemWidth();
                }
            }

            if (ImGui.TableSetColumnIndex(1))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                string[] options = TriggerCondition.SourceOptions;
                ImGui.Combo("##SourceCombo", ref Unsafe.As<TriggerDataSource, int>(ref trigger.Source), options, options.Length);
                ImGui.PopItemWidth();
            }


            if (ImGui.TableSetColumnIndex(2))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                string[] options = TriggerCondition.OperatorOptions;
                ImGui.Combo("##OpCombo", ref Unsafe.As<TriggerDataOp, int>(ref trigger.Op), options, options.Length);
                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(3))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());

                _triggerValueInput = trigger.Value.ToString();
                ImGui.InputText("##InputFloat", ref _triggerValueInput, 10);
                if (float.TryParse(_triggerValueInput, out float value))
                {
                    trigger.Value = value;
                }

                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(4))
            {
                Vector2 buttonSize = new Vector2(45, 0);

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                if (i < this.TriggerConditions.Count)
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => RemoveTriggerCondition(trigger), "Remove Trigger", buttonSize);
                }
                else
                {
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => AddTriggerCondition(), "New Trigger", buttonSize);
                }
            }
        }

        private void AddTriggerData(TriggerData triggerData)
        {
            this.TriggerName = triggerData.Name.ToString();
            this._triggerNameInput = triggerData.Name.ToString();
            this.TriggerList.Add(triggerData);
        }

        private void ResetTriggers()
        {
            this.TriggerName = string.Empty;
            this.TriggerList = new List<TriggerData>();
            this.CustomIcon = 0;
            this.IconOption = 0;
            this.IconPickerIndex = 0;
        }

        private void AddTriggerCondition()
        {
            if (!this.TriggerConditions.Any() ||
                (_inputTrigger.Cond != TriggerCond.None &&
                _inputTrigger.Op != TriggerDataOp.None &&
                _inputTrigger.Source != TriggerDataSource.None))
            {
                this.TriggerConditions.Add(_inputTrigger);
                _inputTrigger = new TriggerCondition();
            }
        }

        private void RemoveTriggerCondition(TriggerCondition trigger)
        {
            this.TriggerConditions.Remove(trigger);
        }
    }
}