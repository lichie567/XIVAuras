using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    [JsonObject]
    public class XIVAurasConfig : IConfigurable, IXIVAurasDisposable
    {
        public string Name => "XIVAuras";

        public string Version => Plugin.Version;

        public AuraListConfig AuraList { get; set; }

        public XIVAurasConfig()
        {
            this.AuraList = new AuraListConfig();
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.AuraList;
        }
    }
}