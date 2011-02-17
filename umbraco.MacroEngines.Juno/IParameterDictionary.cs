using System.Collections.Generic;

namespace umbraco.MacroEngines {
    public interface IParameterDictionary : IEnumerable<KeyValuePair<string, string>> {
        string this[string alias] { get; }
    }
}
