using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    [JsonObject]
    public class XIVAurasConfig : IAuraGroup, IConfigurable, IPluginDisposable
    {
        public string Name
        {
            get => "XIVAuras";
            set { }
        }

        public string Version => Plugin.Version;

        public AuraListConfig AuraList { get; set; }

        public GroupConfig GroupConfig { get; set; }

        public FontConfig FontConfig { get; set; }

        [JsonIgnore]
        private AboutPage AboutPage { get; } = new AboutPage();

        public XIVAurasConfig()
        {
            this.AuraList = new AuraListConfig();
            this.GroupConfig = new GroupConfig();
            this.FontConfig = new FontConfig();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ConfigHelpers.SaveConfig(this);
            }
        }

        public override string ToString() => this.Name;

        public IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.AuraList;
            yield return this.GroupConfig;
            yield return this.FontConfig;
            yield return this.AboutPage;
        }

        public void ImportPage(IConfigPage page)
        {
        }
    }
}