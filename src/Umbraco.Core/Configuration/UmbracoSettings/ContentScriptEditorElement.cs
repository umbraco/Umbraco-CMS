using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentScriptEditorElement : ConfigurationElement, IContentScriptEditor
    {
        [ConfigurationProperty("scriptFolderPath")]
        internal InnerTextConfigurationElement<string> ScriptFolderPath
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                       (InnerTextConfigurationElement<string>)this["scriptFolderPath"],
                    //set the default
                       "/scripts");
            }
        }

        [ConfigurationProperty("scriptFileTypes")]
        internal OptionalCommaDelimitedConfigurationElement ScriptFileTypes
        {
            get
            {
                return new OptionalCommaDelimitedConfigurationElement(
                       (OptionalCommaDelimitedConfigurationElement)this["scriptFileTypes"],
                    //set the default
                       new[] { "js", "xml" });                
            }
        }

        [ConfigurationProperty("scriptDisableEditor")]
        internal InnerTextConfigurationElement<bool> DisableScriptEditor
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>) this["scriptDisableEditor"],
                    //set the default
                    false);
            }
        }

        string IContentScriptEditor.ScriptFolderPath
        {
            get { return ScriptFolderPath; }
        }

        IEnumerable<string> IContentScriptEditor.ScriptFileTypes
        {
            get { return ScriptFileTypes; }
        }

        bool IContentScriptEditor.DisableScriptEditor
        {
            get { return DisableScriptEditor; }
        }
    }
}