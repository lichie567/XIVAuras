using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Excel;

namespace XIVAuras.Helpers
{
    public class TexturesCache : IXIVAurasDisposable
    {
        private Dictionary<uint, TextureWrap> Cache { get; init; }

        public TexturesCache()
        {
            this.Cache = new Dictionary<uint, TextureWrap>();
        }

        public TextureWrap? GetTexture<T>(uint rowId, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            var sheet = Singletons.Get<DataManager>().GetExcelSheet<T>();

            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), stackCount, hdIcon);
        }

        public TextureWrap? GetTexture<T>(dynamic? row, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;
            return GetTextureFromIconId(iconId, stackCount, hdIcon);
        }

        public TextureWrap? GetTextureFromIconId(uint iconId, uint stackCount = 0, bool hdIcon = true)
        {
            if (this.Cache.TryGetValue(iconId + stackCount, out TextureWrap? texture))
            {
                return texture;
            }

            var newTexture = LoadTexture(iconId + stackCount, hdIcon);
            if (newTexture == null)
            {
                return null;
            }

            this.Cache.Add(iconId + stackCount, newTexture);
            return newTexture;
        }

        private unsafe TextureWrap? LoadTexture(uint id, bool hdIcon)
        {
            var hdString = hdIcon ? "_hr1" : "";
            var path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}{hdString}.tex";

            return TexturesCache.LoadTexture(path);
        }

        public static TextureWrap? LoadTexture(string path)
        {
            try
            {
                TexFile? iconFile = Singletons.Get<DataManager>().GetFile<TexFile>(path);
                if (iconFile != null)
                {
                    return Singletons.Get<UiBuilder>().LoadImageRaw(iconFile.GetRgbaImageData(), iconFile.Header.Width, iconFile.Header.Height, 4);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex.ToString());
            }

            return null;
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
                foreach (TextureWrap tex in this.Cache.Values)
                {
                    tex.Dispose();
                }

                this.Cache.Clear();
            }
        }
    }
}