namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LogTypeElement : InnerTextConfigurationElement<string>, ILogType
    {

        string ILogType.LogTypeAlias
        {
            get { return Value; }
        }
    }
}