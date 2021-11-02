using System.Collections.Generic;

namespace XIVAuras.Config
{
    public interface IConfigurable : IEnumerable<IConfigPage>
    {
        string Name { get; }
    }
}
