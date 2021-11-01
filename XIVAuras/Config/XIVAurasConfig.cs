using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Dalamud.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XIVAuras.Auras;

namespace XIVAuras.Config
{
    public class XIVAurasConfig
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = new XIVAurasSerializationBinder()
        };

        public string Version => Plugin.Version;

        public List<IAuraListItem> Auras { get; set; }

        public XIVAurasConfig()
        {
            this.Auras = new List<IAuraListItem>();
        }

        public void AddAura(IAuraListItem aura)
        {
            this.Auras.Add(aura);
            XIVAurasConfig.SaveConfig(this);
        }

        public void DeleteAura(IAuraListItem aura)
        {
            this.Auras.Remove(aura);
            XIVAurasConfig.SaveConfig(this);
        }

        public static string GetAuraExportString(IAuraListItem aura)
        {
            string jsonString = JsonConvert.SerializeObject(aura, Formatting.None, _serializerSettings);
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (DeflateStream compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal))
                {
                    using (StreamWriter writer = new StreamWriter(compressionStream, Encoding.UTF8))
                    {
                        writer.Write(jsonString);
                    }
                }

                return Convert.ToBase64String(outputStream.ToArray());
            }
        }

        public static IAuraListItem? GetAuraFromImportString(string importString)
        {
            if (string.IsNullOrEmpty(importString)) return null;

            byte[] bytes = Convert.FromBase64String(importString);

            string decodedJsonString;
            using (MemoryStream inputStream = new MemoryStream(bytes))
            {
                using (DeflateStream compressionStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(compressionStream, Encoding.UTF8))
                    {
                        decodedJsonString = reader.ReadToEnd();
                    }
                }
            }

            try
            {
                IAuraListItem? importedAura = JsonConvert.DeserializeObject<IAuraListItem>(decodedJsonString, _serializerSettings);
                return importedAura;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
            }

            return null;
        }

        public static XIVAurasConfig LoadConfig(string path)
        {
            XIVAurasConfig? config = null;

            try
            {
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    config = JsonConvert.DeserializeObject<XIVAurasConfig>(jsonString, _serializerSettings);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
            }

            return config ?? new XIVAurasConfig();
        }

        public static void SaveConfig(XIVAurasConfig config)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented, _serializerSettings);
                File.WriteAllText(Plugin.ConfigFilePath, jsonString);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Because the game blocks the json serializer from loading assemblies at runtime,
    /// we need to define a custom SerializationBinder to ignore the assembly name for the
    /// types defined by this plugin.
    /// </summary>
    public class XIVAurasSerializationBinder : ISerializationBinder
    {
        private static List<Type> _configTypes = new List<Type>()
        {
            typeof(AuraGroup),
            typeof(Aura),
            typeof(IconAura),
            typeof(BarAura),
            typeof(VisibilityOptions),
            typeof(TriggerOptions),
            typeof(StyleOptions)
        };

        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();

        public XIVAurasSerializationBinder()
        {
            foreach (Type type in _configTypes)
            {
                if (type.FullName is not null)
                {
                    this.typeToName.Add(type, type.FullName);
                    this.nameToType.Add(type.FullName, type);
                }
            }
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            if (this.typeToName.TryGetValue(serializedType, out string? name))
            {
                assemblyName = null;
                typeName = name;
            }
            else
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName;
            }
        }

        public Type BindToType(string? assemblyName, string? typeName)
        {
            if (typeName is not null &&
                this.nameToType.TryGetValue(typeName, out Type? type))
            {
                return type;
            }

            return Type.GetType($"{typeName}, {assemblyName}", true) ??
                throw new TypeLoadException($"Unable to load type '{typeName}' from assembly '{assemblyName}'");
        }
    }
}