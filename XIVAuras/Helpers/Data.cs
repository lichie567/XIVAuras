using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace XIVAuras.Helpers
{
    [StructLayout(LayoutKind.Explicit, Size = 0x8)]
    public struct Combo {
        [FieldOffset(0x00)] public float Timer;
        [FieldOffset(0x04)] public uint Action;
    }

    public struct TriggerData
    {
        public string Name;
        public uint Id;
        public ushort Icon;
        public byte MaxStacks;
        
        [JsonConverter(typeof(ComboIdConverter))]
        public uint[] ComboId;

        public TriggerData(string name, uint id, ushort icon, byte maxStacks = 0, uint[]? comboId = null)
        {
            Name = name;
            Id = id;
            Icon = icon;
            MaxStacks = maxStacks;
            ComboId = comboId ?? new uint[0];
        }
    }

    public struct RecastInfo
    {
        public float RecastTime;
        public float RecastTimeElapsed;
        public ushort MaxCharges;

        public RecastInfo(float recastTime, float recastTimeElapsed, ushort maxCharges)
        {
            RecastTime = recastTime;
            RecastTimeElapsed = recastTimeElapsed;
            MaxCharges = maxCharges;
        }
    }
    
    public class DataSource
    {
        [JsonIgnore]
        private static readonly Dictionary<string, FieldInfo> _fields = 
            typeof(DataSource).GetFields().ToDictionary((x) => x.Name.ToLower());

        public string GetFormattedString(string format, string numberFormat)
        {
            return TextTagFormatter.TextTagRegex.Replace(
                format,
                new TextTagFormatter(this, numberFormat, _fields).Evaluate);
        }

        public uint Id;
        public float Value;
        public int Stacks;
        public int MaxStacks;
        public ushort Icon;
        
        public uint Level;
        public uint Hp;
        public uint MaxHp;
        public uint Mp;
        public uint MaxMp;
        public uint Gp;
        public uint MaxGp;
        public uint Cp;
        public uint MaxCp;
        public bool HasPet;
    }
}
