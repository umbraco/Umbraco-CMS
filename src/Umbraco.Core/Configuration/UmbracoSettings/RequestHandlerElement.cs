using System;
using System.Configuration;
using System.Globalization;
using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RequestHandlerElement : ConfigurationElement
    {
        [ConfigurationProperty("useDomainPrefixes")]
        internal InnerTextConfigurationElement<bool> UseDomainPrefixes
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["useDomainPrefixes"],
                    //set the default
                    false);  
            }
        }

        [ConfigurationProperty("addTrailingSlash")]
        internal InnerTextConfigurationElement<bool> AddTrailingSlash
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["addTrailingSlash"],
                    //set the default
                    true);  
            }
        }

        private UrlReplacingElement _defaultUrlReplacing;
        [ConfigurationProperty("urlReplacing")]
        internal UrlReplacingElement UrlReplacing
        {
            get
            {
                if (_defaultUrlReplacing != null)
                {
                    return _defaultUrlReplacing;
                }
                
                //here we need to check if this element is defined, if it is not then we'll setup the defaults
                var prop = Properties["urlReplacing"];
                var urls = this[prop] as ConfigurationElement;
                if (urls != null && urls.ElementInformation.IsPresent == false)
                {
                    var dictionary = new Dictionary<char, string>()
                        {
                            {' ',"-"},
                            {'\"',""},
                            {'\'',""},
                            {'%',""},
                            {'.',""},
                            {';',""},
                            {'/',""},
                            {'\\',""},
                            {':',""},
                            {'#',""},
                            {'+',"plus"},
                            {'*',"star"},
                            {'&',""},
                            {'?',""},
                            {'æ',"ae"},
                            {'ø',"oe"},
                            {'å',"aa"},
                            {'ä',"ae"},
                            {'ö',"oe"},
                            {'ü',"ue"},
                            {'ß',"ss"},
                            {'Ä',"ae"},
                            {'Ö',"oe"},
                            {'|',"-"},
                            {'<',""},
                            {'>',""}
                        };

                    //const string chars = @" ,"",',%,.,;,/,\,:,#,+,*,&,?,æ,ø,å,ä,ö,ü,ß,Ä,Ö,|,<,>";

                    var collection = new CharCollection();
                    foreach (var c in dictionary)
                    {
                        collection.Add(new CharElement
                        {
                            Char = c.Key.ToString(CultureInfo.InvariantCulture),
                            Replacement = c.Value.ToString(CultureInfo.InvariantCulture)
                        });
                    }

                    _defaultUrlReplacing = new UrlReplacingElement()
                        {
                            CharCollection = collection
                        };

                    return _defaultUrlReplacing;
                }
                
                return (UrlReplacingElement)this["urlReplacing"];
            }
        }
    }
}