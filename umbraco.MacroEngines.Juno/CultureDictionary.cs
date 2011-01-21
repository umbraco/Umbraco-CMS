namespace umbraco.MacroEngines {
    public class CultureDictionary : DynamicLambdaDictionary<string> {
        public CultureDictionary() : base(library.GetDictionaryItem) {}
    }
}
