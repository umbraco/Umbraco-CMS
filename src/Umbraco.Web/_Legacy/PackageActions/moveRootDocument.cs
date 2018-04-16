
//TODO: MIgrate this to core: http://issues.umbraco.org/issue/U4-5857

//using System;
//using System.Xml;

//namespace umbraco.cms.businesslogic.packager.standardPackageActions
//{
//    /// <summary>
//    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
//    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
//    /// </summary>
//    public class moveRootDocument : umbraco.interfaces.IPackageAction
//    {
//        #region IPackageAction Members

//        /// <summary>
//        /// Executes the specified package action.
//        /// </summary>
//        /// <param name="packageName">Name of the package.</param>
//        /// <param name="xmlData">The XML data.</param>
//        /// <example><code>
//        /// <Action runat="install" alias="moveRootDocument" documentName="News" parentDocumentType="Home"  />
//        /// </code></example>
//        /// <returns>True if executed succesfully</returns>
//        public bool Execute(string packageName, XmlNode xmlData)
//        {

//            string documentName = xmlData.Attributes["documentName"].Value;
//            string parentDocumentType = xmlData.Attributes["parentDocumentType"].Value;
//            string parentDocumentName = "";

//            if (xmlData.Attributes["parentDocumentName"] != null)
//                parentDocumentName = xmlData.Attributes["parentDocumentName"].Value;

//            int parentDocid = 0;

//            ContentType ct = ContentType.GetByAlias(parentDocumentType);
//            Content[] docs = web.Document.getContentOfContentType(ct);

//            if (docs.Length > 0)
//            {
//                if (String.IsNullOrEmpty(parentDocumentName))
//                    parentDocid = docs[0].Id;
//                else
//                {
//                    foreach (Content doc in docs)
//                    {
//                        if (doc.Text == parentDocumentName)
//                            parentDocid = doc.Id;
//                    }
//                }
//            }

//            if (parentDocid > 0)
//            {
//                web.Document[] rootDocs = web.Document.GetRootDocuments();

//                foreach (web.Document rootDoc in rootDocs)
//                {
//                    if (rootDoc.Text == documentName)
//                    {
//                        rootDoc.Move(parentDocid);
//                        rootDoc.PublishWithSubs(new umbraco.BusinessLogic.User(0));
//                    }
//                }
//            }


//            return true;
//        }

//        //this has no undo.
//        /// <summary>
//        /// This action has no undo.
//        /// </summary>
//        /// <param name="packageName">Name of the package.</param>
//        /// <param name="xmlData">The XML data.</param>
//        /// <returns></returns>
//        public bool Undo(string packageName, XmlNode xmlData)
//        {
//            return true;
//        }

//        /// <summary>
//        /// Action alias
//        /// </summary>
//        /// <returns></returns>
//        public string Alias()
//        {
//            return "moveRootDocument";
//        }

//        #endregion

//        public XmlNode SampleXml()
//        {
//            throw new NotImplementedException();
//        }

//    }
//}
