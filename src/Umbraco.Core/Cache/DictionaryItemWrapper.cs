namespace Umbraco.Core.Cache
{
    internal class DictionaryItemWrapper
    {
        public DictionaryItemWrapper(dynamic item)
        {
            Key = item.Key;
            Value = item.Value;
        }
        
        public object Key { get; private set; }
        public object Value { get; private set; }
    }
}