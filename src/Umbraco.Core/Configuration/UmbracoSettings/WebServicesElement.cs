using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class WebServicesElement : ConfigurationElement
    {
        [ConfigurationProperty("enabled")]
        internal bool Enabled
        {
            get { return (bool)base["enabled"]; }
        }

        [ConfigurationProperty("documentServiceUsers")]
        internal CommaDelimitedConfigurationElement DocumentServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement)this["documentServiceUsers"]; }
        }

        [ConfigurationProperty("fileServiceUsers")]
        internal CommaDelimitedConfigurationElement FileServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement)this["fileServiceUsers"]; }
        }

        [ConfigurationProperty("fileServiceFolders")]
        internal CommaDelimitedConfigurationElement FileServiceFolders
        {
            get { return (CommaDelimitedConfigurationElement)this["fileServiceFolders"]; }
        }

        [ConfigurationProperty("stylesheetServiceUsers")]
        internal CommaDelimitedConfigurationElement StylesheetServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement)this["stylesheetServiceUsers"]; }
        }

        [ConfigurationProperty("memberServiceUsers")]
        internal CommaDelimitedConfigurationElement MemberServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement)this["memberServiceUsers"]; }
        }

        [ConfigurationProperty("mediaServiceUsers")]
        internal CommaDelimitedConfigurationElement MediaServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement) this["mediaServiceUsers"]; }
        }

        [ConfigurationProperty("templateServiceUsers")]
        internal CommaDelimitedConfigurationElement TemplateServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement)this["templateServiceUsers"]; }
        }

        [ConfigurationProperty("maintenanceServiceUsers")]
        internal CommaDelimitedConfigurationElement MaintenanceServiceUsers
        {
            get { return (CommaDelimitedConfigurationElement)this["maintenanceServiceUsers"]; }
        }
    }
}