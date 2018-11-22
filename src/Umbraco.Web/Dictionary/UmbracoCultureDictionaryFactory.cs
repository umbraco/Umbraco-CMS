namespace Umbraco.Web.Dictionary
{
    /// <summary>
    /// A culture dictionary factory used to create an Umbraco.Core.Dictionary.ICultureDictionary.
    /// </summary>
    /// <remarks>
    /// In the future this will allow use to potentially store dictionary items elsewhere and allows for maximum flexibility.
    /// </remarks>
    internal class DefaultCultureDictionaryFactory : Umbraco.Core.Dictionary.ICultureDictionaryFactory
    {
        public Umbraco.Core.Dictionary.ICultureDictionary CreateDictionary()
        {
            return new DefaultCultureDictionary();
        }
    }
}
