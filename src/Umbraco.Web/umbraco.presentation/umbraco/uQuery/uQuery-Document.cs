using System.Collections.Generic;
using umbraco.cms.businesslogic.web;

namespace umbraco
{
	public static partial class uQuery
	{
		/// <summary>
		/// Get collection of Document objects from the supplied CSV of IDs
		/// </summary>
		/// <param name="csv">string csv of IDs</param>
		/// <returns>collection or emtpy collection</returns>
		public static IEnumerable<Document> GetDocumentsByCsv(string csv)
		{
			var documents = new List<Document>();
			var ids = uQuery.GetCsvIds(csv);

			if (ids != null)
			{
				foreach (string id in ids)
				{
					var document = uQuery.GetDocument(id);
					if (document != null)
					{
						documents.Add(document);
					}
				}
			}

			return documents;
		}

		/// <summary>
		/// Gets the documents by XML.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static IEnumerable<Document> GetDocumentsByXml(string xml)
		{
			var documents = new List<Document>();
			var ids = uQuery.GetXmlIds(xml);

			if (ids != null)
			{
				foreach (int id in ids)
				{
					var document = uQuery.GetDocument(id);
					if (document != null)
					{
						documents.Add(document);
					}
				}
			}

			return documents;
		}

		/// <summary>
		/// checks to see if the current node can be got via the nodeFactory, and then constructed as a Document, if not then
		/// checks to see if there's an id parameter on the QueryString to construct a Document
		/// </summary>
		/// <returns>the current Document or null</returns>
		public static Document GetCurrentDocument()
		{
			Document document = null;
			var currentNode = uQuery.GetCurrentNode();

			if (currentNode != null)
			{
				document = uQuery.GetDocument(currentNode.Id);
			}
			else
			{
				document = uQuery.GetDocument(uQuery.GetIdFromQueryString());
			}

			return document;
		}

		/// <summary>
		/// Get document from an ID
		/// </summary>
		/// <param name="documentId">string ID of document to get</param>
		/// <returns>Document or null</returns>
		public static Document GetDocument(string documentId)
		{
			int id;
			Document document = null;

			if (!string.IsNullOrWhiteSpace(documentId) && int.TryParse(documentId, out id))
			{
				document = uQuery.GetDocument(id);
			}

			return document;
		}

		/// <summary>
		/// Get media item from an ID
		/// </summary>
		/// <param name="id">ID of media item to get</param>
		/// <returns>Media or null</returns>
		public static Document GetDocument(int id)
		{
			// suppress error if Document with supplied id doesn't exist
			try
			{
				return new Document(id);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Determines whether the specified Document is published.
		/// </summary>
		/// <param name="id">The Document's id.</param>
		/// <returns>
		/// 	<c>true</c> if the specified id is published; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsDocumentPublished(string id)
		{
			return umbraco.content.Instance.XmlContent.GetElementById(id) != null;
		}

		/// <summary>
		/// Extension method on Document collection to return key value pairs of: Id / Text
		/// </summary>
		/// <param name="documents">generic list of Document objects</param>
		/// <returns>a collection of document IDs and their text fields</returns>
		public static Dictionary<int, string> ToNameIds(this IEnumerable<Document> documents)
		{
			var dictionary = new Dictionary<int, string>();

			foreach (var document in documents)
			{
				if (document != null && document.Id != -1) // to compensate for the root document now throwing a null error on it's .Text property
				{
					dictionary.Add(document.Id, document.Text);
				}
			}

			return dictionary;
		}
	}
}