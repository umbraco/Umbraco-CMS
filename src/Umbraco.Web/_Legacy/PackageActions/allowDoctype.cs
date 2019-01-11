using System;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core._Legacy.PackageActions;
using Umbraco.Web.Composing;

namespace Umbraco.Web._Legacy.PackageActions
{
    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class allowDoctype : IPackageAction
    {

        #region IPackageAction Members

        /// <summary>
        /// Allows a documentType to be created below another documentType.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example><code>
        /// <Action runat="install" alias="allowDocumenttype" documentTypeAlias="MyNewDocumentType" parentDocumentTypeAlias="HomePage"  />
        /// </code></example>
        /// <returns>Returns true on success</returns>
        public bool Execute(string packageName, XElement xmlData)
        {
            string doctypeName = xmlData.AttributeValue<string>("documentTypeAlias");
            string parentDoctypeName = xmlData.AttributeValue<string>("parentDocumentTypeAlias");

            //global::umbraco.cms.businesslogic.ContentType ct = global::umbraco.cms.businesslogic.ContentType.GetByAlias(doctypeName);
            //global::umbraco.cms.businesslogic.ContentType parentct = global::umbraco.cms.businesslogic.ContentType.GetByAlias(parentDoctypeName);
            var ct = Current.Services.ContentTypeService.Get(doctypeName);
            var parentct = Current.Services.ContentTypeService.Get(parentDoctypeName);

            if (ct != null && parentct != null)
            {
                bool containsId = false;
                ArrayList tmp = new ArrayList();

                foreach (int i in parentct.AllowedContentTypes.Select(x => x.Id.Value).ToList())
                {
                    tmp.Add(i);
                    if (i == ct.Id)
                        containsId = true;
                }

                if (!containsId)
                {

                    int[] ids = new int[tmp.Count + 1];
                    for (int i = 0; i < tmp.Count; i++) ids[i] = (int)tmp[i];
                    ids[ids.Length - 1] = ct.Id;

                    //parentct.AllowedChildContentTypeIDs = ids;
                    var so = 0;
                    parentct.AllowedContentTypes = ids.Select(x => new ContentTypeSort(x, so++));
                    //parentct.Save();
                    Current.Services.ContentTypeService.Save(parentct);
                    return true;
                }
            }
            return false;
        }

        //this has no undo.
        /// <summary>
        /// This action has no undo.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XElement xmlData)
        {
            return true;
        }

        /// <summary>
        /// Action Alias.
        /// </summary>
        /// <returns></returns>
        public string Alias()
        {
            return "allowDocumenttype";
        }

        #endregion
        
    }
}
