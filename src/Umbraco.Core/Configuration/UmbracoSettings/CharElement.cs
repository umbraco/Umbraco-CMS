namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class CharElement : InnerTextConfigurationElement<string>
    {
        private string _char;
        private string _replacement;

        public string Char
        {
            get { return _char ?? (_char = (string)RawXml.Attribute("org")); }
            set { _char = value; }
        }

        public string Replacement
        {
            get { return _replacement ?? (_replacement = Value); }
            set { _replacement = value; }
        }
    }
}