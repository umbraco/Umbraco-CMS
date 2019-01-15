using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Xml;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Web._Legacy.PackageActions
{
    public class AddProxyFeedHost : IPackageAction
    {
        #region IPackageAction Members

        public bool Execute(string packageName, XElement xmlData)
        {
            var hostname = xmlData.AttributeValue<string>("host");
            if (string.IsNullOrEmpty(hostname))
                return false;

            var xdoc = XDocument.Load(IOHelper.MapPath(SystemFiles.FeedProxyConfig));

            var insert = true;

            if (xdoc.Root.HasElements)
            {
                foreach (var node in xdoc.Root.Elements("allow"))
                {
                    if (node.AttributeValue<string>("host") != null && node.AttributeValue<string>("host") == hostname)
                        insert = false;
                }
            }

            if (insert)
            {
                xdoc.Root.Add(new XElement("allow", new XAttribute("host", hostname)));
                xdoc.Save(IOHelper.MapPath(SystemFiles.FeedProxyConfig));

                return true;
            }
            return false;
        }

        public string Alias()
        {
            return "addProxyFeedHost";
        }

        public bool Undo(string packageName, XElement xmlData)
        {
            var hostname = xmlData.AttributeValue<string>("host");
            if (string.IsNullOrEmpty(hostname))
                return false;

            var xdoc = XDocument.Load(IOHelper.MapPath(SystemFiles.FeedProxyConfig));

            bool inserted = false;
            if (xdoc.Root.HasElements)
            {
                foreach (var node in xdoc.Root.Elements("allow"))
                {
                    if (node.AttributeValue<string>("host") != null && node.AttributeValue<string>("host") == hostname)
                    {
                        node.Remove();
                        inserted = true;
                    }
                }
            }

            if (inserted)
            {
                xdoc.Save(IOHelper.MapPath(SystemFiles.FeedProxyConfig));
                return true;
            }

            return false;
        }

        #endregion
        
    }
}
