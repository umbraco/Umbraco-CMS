using System;
using System.Collections.Generic;
using System.Linq;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the Document object
	/// </summary>
	public static class DocumentExtensions
	{
		/// <summary>
		/// Gets the ancestor by path level.
		/// </summary>
		/// <param name="document">an <c>umbraco.cms.businesslogic.web.Document</c> object.</param>
		/// <param name="level">The level.</param>
		/// <returns>Returns an ancestor document by path level.</returns>
		public static Document GetAncestorByPathLevel(this Document document, int level)
		{
			var documentId = uQuery.GetNodeIdByPathLevel(document.Path, level);
			return uQuery.GetDocument(documentId);
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'ancestor'
		/// </summary>
		/// <param name="document">an <c>umbraco.cms.businesslogic.web.Document</c> object.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetAncestorDocuments(this Document document)
		{
			var ancestor = uQuery.GetDocument(document.ParentId);
			while (ancestor != null && ancestor.Id != -1)
			{
				yield return ancestor;

				ancestor = uQuery.GetDocument(ancestor.ParentId);
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'ancestor-or-self'
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetAncestorOrSelfDocuments(this Document document)
		{
			yield return document;
			foreach (var ancestor in document.GetAncestorDocuments())
			{
				yield return ancestor;
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'preceding-sibling'
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetPrecedingSiblingDocuments(this Document document)
		{
			if (document.Parent != null)
			{
				var parentDocument = (Document)document.Parent;
				foreach (var precedingSiblingNode in parentDocument.GetChildDocuments().Where(d => d.sortOrder < document.sortOrder))
				{
					yield return precedingSiblingNode;
				}
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'following-sibling'
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetFollowingSiblingDocuments(this Document document)
		{
			if (document.Parent != null)
			{
				var parentDocument = (Document)document.Parent;
				foreach (var followingSiblingNode in parentDocument.GetChildDocuments().Where(d => d.sortOrder > document.sortOrder))
				{
					yield return followingSiblingNode;
				}
			}
		}

		/// <summary>
		/// Gets all sibling Documents
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetSiblingDocuments(this Document document)
		{
			if (document.Parent != null)
			{
				var parentDocument = (Document)document.Parent;
				foreach (var siblingNode in parentDocument.GetChildDocuments().Where(childNode => childNode.Id != document.Id))
				{
					yield return siblingNode;
				}
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'descendant-or-self'
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Node as IEnumerable</returns>
		public static IEnumerable<Document> GetDescendantOrSelfDocuments(this Document document)
		{
			yield return document;

			foreach (var descendant in document.GetDescendantDocuments())
			{
				yield return descendant;
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'descendant'
		/// Make the All Descendants LINQ queryable
		/// taken from: http://our.umbraco.org/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetDescendantDocuments(this Document document)
		{
			foreach (var child in document.Children)
			{
				yield return child;

				foreach (var descendant in child.GetDescendantDocuments())
				{
					yield return descendant;
				}
			}
		}

		/// <summary>
		/// Drills down into the descendant documents returning those where Func is true, when Func is false further descendants are not checked
		/// taken from: http://ucomponents.codeplex.com/discussions/246406
		/// </summary>
		/// <param name="document">The <c>umbraco.cms.businesslogic.web.Document</c>.</param>
		/// <param name="func">The func</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetDescendantDocuments(this Document document, Func<Document, bool> func)
		{
			foreach (var child in document.Children)
			{
				if (func(child))
				{
					yield return child;

					foreach (var descendant in child.GetDescendantDocuments(func))
					{
						yield return descendant;
					}
				}
			}
		}

		/// <summary>
		/// Gets the descendant documents by document-type.
		/// </summary>
		/// <param name="document">The <c>umbraco.cms.businesslogic.web.Document</c>.</param>
		/// <param name="documentTypeAlias">The document type alias.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetDescendantDocumentsByType(this Document document, string documentTypeAlias)
		{
			return document.GetDescendantDocuments(d => d.ContentType.Alias == documentTypeAlias);
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'child'
		/// Make the imediate Children LINQ queryable
		/// Performance optimised for just imediate children.
		/// taken from: http://our.umbraco.org/wiki/how-tos/useful-helper-extension-methods-%28linq-null-safe-access%29
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Documents as IEnumerable</returns>
		public static IEnumerable<Document> GetChildDocuments(this Document document)
		{
			foreach (var child in document.Children)
			{
				yield return child;
			}
		}

		/// <summary>
		/// Gets the child documents.
		/// </summary>
		/// <param name="document">The <c>umbraco.cms.businesslogic.web.Document</c>.</param>
		/// <param name="func">The func.</param>
		/// <returns>Nodes as IEnumerable</returns>
		public static IEnumerable<Document> GetChildDocuments(this Document document, Func<Document, bool> func)
		{
			foreach (var child in document.Children)
			{
				if (func(child))
				{
					yield return child;
				}
			}
		}

		/// <summary>
		/// Gets the child documents by document-type.
		/// </summary>
		/// <param name="document">The <c>umbraco.cms.businesslogic.web.Document</c>.</param>
		/// <param name="documentTypeAlias">The document type alias.</param>
		/// <returns>Nodes as IEnumerable</returns>
		public static IEnumerable<Document> GetChildDocumentsByType(this Document document, string documentTypeAlias)
		{
			return document.GetChildDocuments(d => d.ContentType.Alias == documentTypeAlias);
		}

		/// <summary>
		/// Extension method on Document to retun a matching child document by name
		/// </summary>
		/// <param name="document">The <c>umbraco.cms.businesslogic.web.Document</c>.</param>
		/// <param name="documentName">name of node to search for</param>
		/// <returns>null or Node</returns>
		public static Document GetChildDocumentByName(this Document document, string documentName)
		{
			return document.GetChildDocuments(d => d.Text == documentName).FirstOrDefault();
		}

		/// <summary>
		/// Publishes this document
		/// </summary>
		/// <param name="document">an umbraco.cms.businesslogic.web.Document object</param>
		/// <param name="useAdminUser">if true then publishes under the context of User(0), if false uses current user</param>
		/// <returns>the same document object on which this is an extension method</returns>
		public static Document Publish(this Document document, bool useAdminUser)
		{
			if (useAdminUser)
			{
                document.SaveAndPublish(new User(0));
			}
			else
			{
				if (User.GetCurrent() != null)
				{
					document.SaveAndPublish(User.GetCurrent());
				}
			}
			return document;
		}

		/// <summary>
		/// Returns a node representation of the document (if it exists)
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Node for the document (if it's published)</returns>
		public static Node ToNode(this Document document)
		{
			return uQuery.GetNode(document.Id);
		}
	}
}