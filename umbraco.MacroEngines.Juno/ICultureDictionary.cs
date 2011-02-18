using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines {

    public interface ICultureDictionary {
        string this[string key] { get; }
        Language Language { get; }
    }

}
