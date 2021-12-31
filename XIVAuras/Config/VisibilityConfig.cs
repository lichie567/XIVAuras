using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;
using System.Linq;
using System.Collections.Generic;

namespace XIVAuras.Config
{

    public class VisibilityConfig : IConfigPage
    {
        public string Name => "Visibility";

        [JsonIgnore] private static readonly string[] _sourceOptions = new string[] { "Value", "Stacks", "MaxStacks" };

        [JsonIgnore] private string _customJobInput = string.Empty;
        [JsonIgnore] private string _hideIfValueInput = string.Empty;

        public bool AlwaysHide = false;
        public bool HideInCombat = false;
        public bool HideOutsideCombat = false;
        public bool HideOutsideDuty = false;
        public bool HideWhilePerforming = false;
        public bool HideInGoldenSaucer = false;

        public bool HideIf = false;
        public TriggerDataSource HideIfDataSource = TriggerDataSource.Value;
        public TriggerDataOp HideIfOp = TriggerDataOp.Equals;
        public float HideIfValue = 0;

        public JobType ShowForJobTypes = JobType.All;
        public string CustomJobString = string.Empty;
        public List<Job> CustomJobList = new List<Job>();

        public IConfigPage GetDefault() => new VisibilityConfig();

        public bool IsVisible(DataSource? data = null)
        {
            if (this.AlwaysHide)
            {
                return false;
            }

            if (this.HideInCombat && CharacterState.IsInCombat())
            {
                return false;
            }

            if (this.HideOutsideDuty && !CharacterState.IsInDuty())
            {
                return false;
            }

            if (this.HideOutsideCombat && !CharacterState.IsInCombat())
            {
                return false;
            }

            if (this.HideWhilePerforming && CharacterState.IsPerforming())
            {
                return false;
            }

            if (this.HideInGoldenSaucer && CharacterState.IsInGoldenSaucer())
            {
                return false;
            }

            if (this.HideIf)
            {
                if (data is not null ||
                    (this.HideIfDataSource != TriggerDataSource.Value &&
                    this.HideIfDataSource != TriggerDataSource.Stacks &&
                    this.HideIfDataSource != TriggerDataSource.MaxStacks))
                {
                    float value = data?.GetDataForSourceType(this.HideIfDataSource) ?? 0;
                    bool result = this.HideIfOp switch
                    {
                        TriggerDataOp.Equals => value == this.HideIfValue,
                        TriggerDataOp.NotEquals => value != this.HideIfValue,
                        TriggerDataOp.LessThan => value < this.HideIfValue,
                        TriggerDataOp.GreaterThan => value > this.HideIfValue,
                        TriggerDataOp.LessThanEq => value <= this.HideIfValue,
                        TriggerDataOp.GreaterThanEq => value >= this.HideIfValue,
                        _ => false
                    };

                    if (result)
                    {
                        return false;
                    }
                }
            }

            if (this.ShowForJobTypes == JobType.All)
            {
                return true;
            }

            if (this.ShowForJobTypes == JobType.Custom)
            {
                return CharacterState.IsJob(this.CustomJobList);
            }

            return CharacterState.IsJob(CharacterState.GetJobsForJobType(this.ShowForJobTypes));
        }

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##VisibilityConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Checkbox("Always Hide", ref this.AlwaysHide);
                ImGui.Checkbox("Hide In Combat", ref this.HideInCombat);
                ImGui.Checkbox("Hide Outside Combat", ref this.HideOutsideCombat);
                ImGui.Checkbox("Hide Outside Duty", ref this.HideOutsideDuty);
                ImGui.Checkbox("Hide While Performing", ref this.HideWhilePerforming);
                ImGui.Checkbox("Hide In Golden Saucer", ref this.HideInGoldenSaucer);

                DrawHelpers.DrawSpacing(1);
                ImGui.Checkbox("Hide If:", ref this.HideIf);
                ImGui.SameLine();
                ImGui.PushItemWidth(90);
                ImGui.Combo("##HideIfSourceCombo", ref Unsafe.As<TriggerDataSource, int>(ref this.HideIfDataSource), _sourceOptions, _sourceOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();
                string[] operatorOptions = TriggerOptions.OperatorOptions;
                ImGui.PushItemWidth(80);
                ImGui.Combo("##HideIfOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.HideIfOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_hideIfValueInput))
                {
                    _hideIfValueInput = this.HideIfValue.ToString();
                }

                float width = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() + padX;
                ImGui.PushItemWidth(width);
                if (ImGui.InputText("##HideIfValue", ref _hideIfValueInput, 10, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (float.TryParse(_hideIfValueInput, out float value))
                    {
                        this.HideIfValue = value;
                    }
                }

                ImGui.PopItemWidth();
                
                DrawHelpers.DrawSpacing(1);
                string[] jobTypeOptions = Enum.GetNames(typeof(JobType));
                ImGui.Combo("Show for Jobs", ref Unsafe.As<JobType, int>(ref this.ShowForJobTypes), jobTypeOptions, jobTypeOptions.Length);

                if (this.ShowForJobTypes == JobType.Custom)
                {
                    if (string.IsNullOrEmpty(_customJobInput))
                    {
                        _customJobInput = this.CustomJobString.ToUpper();
                    }

                    if (ImGui.InputTextWithHint("Custom Job List", "Comma Separated List (ex: WAR, SAM, BLM)", ref _customJobInput, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        IEnumerable<string> jobStrings = _customJobInput.Split(',').Select(j => j.Trim());
                        List<Job> jobList = new List<Job>();
                        foreach (string j in jobStrings)
                        {
                            if (Enum.TryParse(j, true, out Job parsed))
                            {
                                jobList.Add(parsed);
                            }
                            else
                            {
                                jobList.Clear();
                                _customJobInput = string.Empty;
                                break;
                            }
                        }

                        _customJobInput = _customJobInput.ToUpper();
                        this.CustomJobString = _customJobInput;
                        this.CustomJobList = jobList;
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}