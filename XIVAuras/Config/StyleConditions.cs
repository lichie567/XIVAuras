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
    public class StyleCondition<T> : IConfigurable where T : class?, IConfigPage, new()
    {
        public int TriggerDataSourceIndex = 0;
        public TriggerDataSource Source = TriggerDataSource.Value;
        public TriggerDataOp Op = TriggerDataOp.GreaterThan;
        public float Value = 0;

        public T Style { get; set; } = new T();

        public string Name
        {
            get => this.Style.Name;
            set { }
        }

        public override string ToString() => $"Condition [{this.Name}]";

        public StyleCondition() {}

        public StyleCondition(T? defaultStyle)
        {
            this.Style = ConfigHelpers.SerializedClone<T>(defaultStyle) ?? new T();
        }

        public IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.Style;
        }

        public void ImportPage(IConfigPage page)
        {
            if (page is T t)
            {
                this.Style = t;
            }
        }

        public bool GetResult(DataSource data)
        {
            float value = data.GetDataForSourceType(this.Source);

            return this.Op switch
            {
                TriggerDataOp.Equals => value == this.Value,
                TriggerDataOp.NotEquals => value != this.Value,
                TriggerDataOp.LessThan => value < this.Value,
                TriggerDataOp.GreaterThan => value > this.Value,
                TriggerDataOp.LessThanEq => value <= this.Value,
                TriggerDataOp.GreaterThanEq => value >= this.Value,
                _ => false
            } || Singletons.Get<PluginManager>().IsConfigurableOpen(this);
        }
    }

    public class StyleConditions<T> : IConfigPage where T : class?, IConfigPage, new()
    {
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerDataSource>();
        [JsonIgnore] private static readonly string[] _operatorOptions = new string[] { "==", "!=", "<", ">", "<=", ">=" };
        [JsonIgnore] private static readonly string _text = $"Add Conditions below to specify alternate appearance configurations under certain conditions.";
        [JsonIgnore] private static readonly float _yOffset = ImGui.CalcTextSize(_text).Y;
        [JsonIgnore] private static string[] _triggerOptions = new string[0];
        [JsonIgnore] private string _styleConditionValueInput = string.Empty;
        [JsonIgnore] private int _swapX = -1;
        [JsonIgnore] private int _swapY = -1;
        [JsonIgnore] private int _triggerCount = 0;
        [JsonIgnore] private T? _defaultStyle;

        public string Name => "Conditions";
        public IConfigPage GetDefault() => new StyleConditions<T>();
      
        public List<StyleCondition<T>> Conditions { get; set; } = new List<StyleCondition<T>>();

        public T? GetStyle(DataSource[]? data, int triggeredIndex)
        {
            if (!this.Conditions.Any() || data is null)
            {
                return null;
            }

            foreach (var condition in this.Conditions)
            {
                // int index = Math.Clamp(condition.TriggerDataSourceIndex, 0, data.Length - 1);
                int index = condition.TriggerDataSourceIndex == 0
                    ? triggeredIndex
                    : condition.TriggerDataSourceIndex - 1;

                if (condition.GetResult(data[index]))
                {
                    return condition.Style;
                }
            }

            return null;
        }

        public void UpdateTriggerCount(int count)
        {
            if (count < _triggerCount)
            {
                foreach (var condition in this.Conditions)
                {
                    condition.TriggerDataSourceIndex = Math.Clamp(condition.TriggerDataSourceIndex, 0, count);
                }
            }

            if (count > _triggerOptions.Length)
            {
                _triggerOptions = new string[count + 1];
                _triggerOptions[0] = "Dynamic";
                for (int i = 1; i < _triggerOptions.Length; i++)
                {
                    _triggerOptions[i] = $"Trigger {i}";
                }
            }

            _triggerCount = count;
        }

        public void UpdateDefaultStyle(T style)
        {
            _defaultStyle = style;
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            ImGui.Text(_text);
            size = size.AddY(-(_yOffset + padY));
            if (ImGui.BeginChild("##StyleConditions", new Vector2(size.X, size.Y), true))
            {
                ImGuiTableFlags tableFlags =
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.BordersOuter |
                    ImGuiTableFlags.BordersInner |
                    ImGuiTableFlags.ScrollY |
                    ImGuiTableFlags.NoSavedSettings;

                if (ImGui.BeginTable("##Conditions_Table", 6, tableFlags, new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2)))
                {
                    Vector2 buttonSize = new(30, 0);
                    int buttonCount = this.Conditions.Count > 1 ? 4 : 2;
                    float actionsWidth = buttonSize.X * buttonCount + padX * (buttonCount - 1);
                    ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthFixed, 55, 0);
                    ImGui.TableSetupColumn("Data Source", ImGuiTableColumnFlags.WidthFixed, 90, 1);
                    ImGui.TableSetupColumn("Data", ImGuiTableColumnFlags.WidthFixed, 90, 2);
                    ImGui.TableSetupColumn("Operator", ImGuiTableColumnFlags.WidthFixed, 55, 3);
                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch, 0, 4);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 5);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < this.Conditions.Count; i++)
                    {
                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                        this.DrawStyleConditionRow(i);
                    }
                    
                    ImGui.PushID(this.Conditions.Count.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                    ImGui.TableSetColumnIndex(5);
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Plus, () => this.Conditions.Add(new StyleCondition<T>(_defaultStyle)), "New Condition", buttonSize);
                }

                ImGui.EndTable();

                if (_swapX < this.Conditions.Count && _swapX >= 0 &&
                    _swapY < this.Conditions.Count && _swapY >= 0)
                {
                    var temp = this.Conditions[_swapX];
                    this.Conditions[_swapX] = this.Conditions[_swapY];
                    this.Conditions[_swapY] = temp;

                    _swapX = -1;
                    _swapY = -1;
                }
            }
            

            ImGui.EndChild();
        }

        private void DrawStyleConditionRow(int i)
        {
            StyleCondition<T> condition = this.Conditions[i];

            if (ImGui.TableSetColumnIndex(0))
            {
                if (i == 0)
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                    ImGui.Text("IF");
                }
                else
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                    ImGui.Text("ELSE IF");
                }
            }

            if (ImGui.TableSetColumnIndex(1))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                ImGui.Combo("##TriggerCombo", ref condition.TriggerDataSourceIndex, _triggerOptions, _triggerCount + 1);
                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(2))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                ImGui.Combo("##SourceCombo", ref Unsafe.As<TriggerDataSource, int>(ref condition.Source), _sourceOptions, _sourceOptions.Length);
                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(3))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());
                ImGui.Combo("##OpCombo", ref Unsafe.As<TriggerDataOp, int>(ref condition.Op), _operatorOptions, _operatorOptions.Length);
                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(4))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                ImGui.PushItemWidth(ImGui.GetColumnWidth());

                _styleConditionValueInput = condition.Value.ToString();
                ImGui.InputText("##InputFloat", ref _styleConditionValueInput, 10);
                if (float.TryParse(_styleConditionValueInput, out float value))
                {
                    condition.Value = value;
                }

                ImGui.PopItemWidth();
            }

            if (ImGui.TableSetColumnIndex(5))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                Vector2 buttonSize = new(30, 0);
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Pen, () => Singletons.Get<PluginManager>().Edit(condition), "Edit Style", buttonSize);
       
                if (this.Conditions.Count > 1)
                {
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowUp, () => Swap(i, i - 1), "Move Up", buttonSize);

                    ImGui.SameLine();
                    DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.ArrowDown, () => Swap(i, i + 1), "Move Down", buttonSize);
                }

                ImGui.SameLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => this.Conditions.Remove(condition), "Remove Condition", buttonSize);
            }
        }

        private void Swap(int x, int y)
        {
            _swapX = x;
            _swapY = y;
        }
    }
}