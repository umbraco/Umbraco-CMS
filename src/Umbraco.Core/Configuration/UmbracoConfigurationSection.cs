using System.Configuration;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Represents an Umbraco configuration section which can be used to pass to UmbracoConfiguration.For{T}
    /// </summary>
    public interface IUmbracoConfigurationSection
    {
        
    }

    /// <summary>
    /// Represents an Umbraco section within the configuration file.
    /// </summary>
    /// <remarks>
    /// <para>The requirement for these sections is to be read-only.</para>
    /// <para>However for unit tests purposes it is internally possible to override some values, and
    /// then calling <c>>ResetSection</c> should cancel these changes and bring the section back to
    /// what it was originally.</para>
    /// <para>The <c>UmbracoSettings.For{T}</c> method will return a section, either one that
    /// is in the configuration file, or a section that was created with default values.</para>
    /// </remarks>
    public abstract class UmbracoConfigurationSection : ConfigurationSection, IUmbracoConfigurationSection
    {
        /// <summary>
        /// Gets a value indicating whether the section actually is in the configuration file.
        /// </summary>
        protected bool IsPresent { get { return ElementInformation.IsPresent; } }

    }
}
