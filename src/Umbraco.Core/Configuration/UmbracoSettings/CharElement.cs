namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class CharElement : InnerTextConfigurationElement<string>, IChar
    {
        private string _char;
        private string _replacement;

        internal string Char
        {
            get => _char ?? (_char = (string)RawXml.Attribute("org"));
            set => _char = value;
        }

        internal string Replacement
        {
            get => _replacement ?? (_replacement = Value);
            set => _replacement = value;
        }

        string IChar.Char => Char;

        string IChar.Replacement => Replacement;
    }
}
