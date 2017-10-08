using System;
using System.ComponentModel;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed in future versions")]
    internal class LinkElement : ConfigurationElement, ILink
    {
        [ConfigurationProperty("application")]
        internal string Application
        {
            get { return (string)base["application"]; }
        }

        [ConfigurationProperty("applicationUrl")]
        internal string ApplicationUrl
        {
            get { return (string)base["applicationUrl"]; }
        }

        [ConfigurationProperty("language")]
        internal string Language
        {
            get { return (string)base["language"]; }
        }

        [ConfigurationProperty("userType")]
        internal string UserType
        {
            get { return (string)base["userType"]; }
        }

        [ConfigurationProperty("helpUrl")]
        internal string HelpUrl
        {
            get { return (string)base["helpUrl"]; }
        }

        string ILink.Application
        {
            get { return Application; }
        }

        string ILink.ApplicationUrl
        {
            get { return ApplicationUrl; }
        }

        string ILink.Language
        {
            get { return Language; }
        }

        string ILink.UserType
        {
            get { return UserType; }
        }

        string ILink.HelpUrl
        {
            get { return HelpUrl; }
        }
    }
}