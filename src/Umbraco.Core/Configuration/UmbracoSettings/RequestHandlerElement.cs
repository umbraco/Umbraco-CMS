using System;
using System.Configuration;
using System.Globalization;
using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RequestHandlerElement : UmbracoConfigurationElement, IRequestHandlerSection
    {
        [ConfigurationProperty("useDomainPrefixes")]
        public InnerTextConfigurationElement<bool> UseDomainPrefixes
        {
            get { return GetOptionalTextElement("useDomainPrefixes", false); }
        }

        [ConfigurationProperty("addTrailingSlash")]
        public InnerTextConfigurationElement<bool> AddTrailingSlash
        {
            get { return GetOptionalTextElement("addTrailingSlash", true); }
        }

        private UrlReplacingElement _defaultUrlReplacing;
        [ConfigurationProperty("urlReplacing")]
        public UrlReplacingElement UrlReplacing
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
                    _defaultUrlReplacing = new UrlReplacingElement()
                        {
                            CharCollection = GetDefaultCharReplacements()
                        };

                    return _defaultUrlReplacing;
                }
                
                return (UrlReplacingElement)this["urlReplacing"];
            }
        }
        
        internal static CharCollection GetDefaultCharReplacements()
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

            return collection;
        }

        bool IRequestHandlerSection.UseDomainPrefixes
        {
            get { return UseDomainPrefixes; }
        }

        bool IRequestHandlerSection.AddTrailingSlash
        {
            get { return AddTrailingSlash; }
        }

        bool IRequestHandlerSection.RemoveDoubleDashes
        {
            get { return UrlReplacing.RemoveDoubleDashes; }
        }

        bool IRequestHandlerSection.ConvertUrlsToAscii
        {
            get { return UrlReplacing.ConvertUrlsToAscii.InvariantEquals("true"); }
        }

        bool IRequestHandlerSection.TryConvertUrlsToAscii
        {
            get { return UrlReplacing.ConvertUrlsToAscii.InvariantEquals("try"); }
        }

        IEnumerable<IChar> IRequestHandlerSection.CharCollection
        {
            get { return UrlReplacing.CharCollection; }
        }
    }
}