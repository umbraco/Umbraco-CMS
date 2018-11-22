using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentErrorsElement : RawXmlConfigurationElement
    {
        
        public IEnumerable<IContentErrorPage> Error404Collection
        {
            get
            {
                var result = new ContentError404Collection();
                if (RawXml != null)
                {
                    var e404 = RawXml.Elements("error404").First();
                    var ePages = e404.Elements("errorPage").ToArray();                    
                    if (ePages.Any())
                    {
                        //there are multiple
                        foreach (var e in ePages)
                        {
                            result.Add(new ContentErrorPageElement(e)
                            {
                                Culture = (string)e.Attribute("culture"),
                                RawValue = e.Value
                            });
                        }
                    }
                    else
                    {
                        //there's only one defined
                        result.Add(new ContentErrorPageElement(e404)
                        {
                            RawValue = e404.Value
                        });
                    }                    
                }
                return result;
            }
        }

    }
}