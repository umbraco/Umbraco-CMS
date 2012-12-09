using System;
using System.Collections;
using System.Linq;
using System.Xml;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	/// <summary>
	/// This class implements the IPackageAction Interface, used to execute code when packages are installed.
	/// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
	/// </summary>
	public class allowDoctype : umbraco.interfaces.IPackageAction
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
		public bool Execute(string packageName, XmlNode xmlData)
		{
			string doctypeName = xmlData.Attributes["documentTypeAlias"].Value;
			string parentDoctypeName = xmlData.Attributes["parentDocumentTypeAlias"].Value;

			cms.businesslogic.ContentType ct = cms.businesslogic.ContentType.GetByAlias(doctypeName);
			cms.businesslogic.ContentType parentct = cms.businesslogic.ContentType.GetByAlias(parentDoctypeName);

			if (ct != null && parentct != null)
			{
				bool containsId = false;
				ArrayList tmp = new ArrayList();

				foreach (int i in parentct.AllowedChildContentTypeIDs.ToList())
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

					parentct.AllowedChildContentTypeIDs = ids;
					parentct.Save();
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
		public bool Undo(string packageName, XmlNode xmlData)
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

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

	}
}