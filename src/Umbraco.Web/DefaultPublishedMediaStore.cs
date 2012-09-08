using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// An IPublishedMediaStore that first checks for the media in Examine, and then reverts to the database
	/// </summary>
	internal class DefaultPublishedMediaStore : IPublishedMediaStore
	{
		public virtual IDocument GetDocumentById(UmbracoContext umbracoContext, int nodeId)
		{
			return GetUmbracoMedia(nodeId);
		}

		public virtual string GetDocumentProperty(UmbracoContext umbracoContext, IDocument node, string propertyAlias)
		{
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			if (node == null) throw new ArgumentNullException("node");
			if (propertyAlias == null) throw new ArgumentNullException("propertyAlias");

			if (propertyAlias.StartsWith("@"))
			{
				//if it starts with an @ then its a property of the object, not a user defined property
				var propName = propertyAlias.TrimStart('@');
				var prop = TypeHelper.GetProperty(typeof(IDocument), propName, true, false, false, false);
				if (prop == null)
					throw new ArgumentException("The property name " + propertyAlias + " was not found on type " + typeof(IDocument));
				var val = prop.GetValue(node, null);
				var valAsString = val == null ? "" : val.ToString();
				return valAsString;
			}
			else
			{
				var prop = node.GetProperty(propertyAlias);
				return prop == null ? null : Convert.ToString(prop.Value);
				//var propertyNode = node.SelectSingleNode("./" + propertyAlias);
				//return propertyNode == null ? null : propertyNode.InnerText;
			}
		}

		private IDocument GetUmbracoMedia(int id)
		{

			try
			{
				//first check in Examine as this is WAY faster
				var criteria = ExamineManager.Instance
					.SearchProviderCollection["InternalSearcher"]
					.CreateSearchCriteria("media");
				var filter = criteria.Id(id);
				var results = ExamineManager
					.Instance.SearchProviderCollection["InternalSearcher"]
					.Search(filter.Compile());
				if (results.Any())
				{
					return ConvertFromSearchResult(results.First());
				}
			}
			catch (FileNotFoundException)
			{
				//Currently examine is throwing FileNotFound exceptions when we have a loadbalanced filestore and a node is published in umbraco
				//See this thread: http://examine.cdodeplex.com/discussions/264341
				//Catch the exception here for the time being, and just fallback to GetMedia
				//TODO: Need to fix examine in LB scenarios!
			}

			var media = global::umbraco.library.GetMedia(id, true);
			if (media != null && media.Current != null)
			{
				media.MoveNext();
				return ConvertFromXPathNavigator(media.Current);
			}

			return null;
		}

		internal IDocument ConvertFromSearchResult(SearchResult searchResult)
		{
			//TODO: Unit test this
			throw new NotImplementedException();
		}

		internal IDocument ConvertFromXPathNavigator(XPathNavigator xpath)
		{
			//TODO: Unit test this
			
			if (xpath == null) throw new ArgumentNullException("xpath");

			var values = new Dictionary<string, string> {{"nodeName", xpath.GetAttribute("nodeName", "")}};

			var result = xpath.SelectChildren(XPathNodeType.Element);
			//add the attributes e.g. id, parentId etc
			if (result.Current != null && result.Current.HasAttributes)
			{
				if (result.Current.MoveToFirstAttribute())
				{
					values.Add(result.Current.Name, result.Current.Value);
					while (result.Current.MoveToNextAttribute())
					{
						values.Add(result.Current.Name, result.Current.Value);
					}
					result.Current.MoveToParent();
				}
			}
			//add the user props
			while (result.MoveNext())
			{
				if (result.Current != null && !result.Current.HasAttributes)
				{
					string value = result.Current.Value;
					if (string.IsNullOrEmpty(value))
					{
						if (result.Current.HasAttributes || result.Current.SelectChildren(XPathNodeType.Element).Count > 0)
						{
							value = result.Current.OuterXml;
						}
					}
					values.Add(result.Current.Name, value);
				}
			}

			return new DictionaryDocument(values, d => d.ParentId.HasValue ? GetUmbracoMedia(d.ParentId.Value) : null);
		}

		/// <summary>
		/// An IDocument that is represented all by a dictionary.
		/// </summary>
		/// <remarks>
		/// This is a helper class and definitely not intended for public use, it expects that all of the values required 
		/// to create an IDocument exist in the dictionary by specific aliases.
		/// </remarks>
		internal class DictionaryDocument : IDocument
		{
			
			//TODO: Unit test this!

			public DictionaryDocument(IDictionary<string, string> valueDictionary, Func<DictionaryDocument, IDocument> getParent)
			{
				if (valueDictionary == null) throw new ArgumentNullException("valueDictionary");
				if (getParent == null) throw new ArgumentNullException("getParent");
				
				_getParent = getParent;
				
				ValidateAndSetProperty(valueDictionary, val => Id = int.Parse(val), "id", "nodeId", "__NodeId"); //should validate the int!
				ValidateAndSetProperty(valueDictionary, val => TemplateId = int.Parse(val), "template", "templateId");
				ValidateAndSetProperty(valueDictionary, val => SortOrder = int.Parse(val), "sortOrder");
				ValidateAndSetProperty(valueDictionary, val => Name = val, "nodeName", "__nodeName");
				ValidateAndSetProperty(valueDictionary, val => UrlName = val, "urlName");
				ValidateAndSetProperty(valueDictionary, val => DocumentTypeAlias = val, "nodeTypeAlias", "__NodeTypeAlias");
				ValidateAndSetProperty(valueDictionary, val => DocumentTypeId = int.Parse(val), "nodeType");
				ValidateAndSetProperty(valueDictionary, val => WriterName = val, "writerName");
				ValidateAndSetProperty(valueDictionary, val => CreatorName = val, "creatorName");
				ValidateAndSetProperty(valueDictionary, val => WriterId = int.Parse(val), "writerID");
				ValidateAndSetProperty(valueDictionary, val => CreatorId = int.Parse(val), "creatorID");
				ValidateAndSetProperty(valueDictionary, val => Path = val, "path", "__Path");
				ValidateAndSetProperty(valueDictionary, val => CreateDate = DateTime.Parse(val), "createDate");
				ValidateAndSetProperty(valueDictionary, val => Level = int.Parse(val), "level");
				ValidateAndSetProperty(valueDictionary, val =>
					{
						int pId;
						ParentId = null;
						if (int.TryParse(val, out pId))
						{
							ParentId = pId;
						}						
					}, "parentID");

				Properties = new Collection<IDocumentProperty>();

				//loop through remaining values that haven't been applied
				foreach (var i in valueDictionary.Where(x => !_keysAdded.Contains(x.Key)))
				{
					//this is taken from examine
					Properties.Add(i.Key.InvariantStartsWith("__") 
					               	? new PropertyResult(i.Key, i.Value, Guid.Empty, PropertyResultType.CustomProperty) 
					               	: new PropertyResult(i.Key, i.Value, Guid.Empty, PropertyResultType.UserProperty));
				}
			}

			private readonly Func<DictionaryDocument, IDocument> _getParent;
			public IDocument Parent
			{
				get { return _getParent(this); }
			}

			public int? ParentId { get; private set; }
			public int Id { get; private set; }
			public int TemplateId { get; private set; }
			public int SortOrder { get; private set; }
			public string Name { get; private set; }
			public string UrlName { get; private set; }
			public string DocumentTypeAlias { get; private set; }
			public int DocumentTypeId { get; private set; }
			public string WriterName { get; private set; }
			public string CreatorName { get; private set; }
			public int WriterId { get; private set; }
			public int CreatorId { get; private set; }
			public string Path { get; private set; }
			public DateTime CreateDate { get; private set; }
			public DateTime UpdateDate { get; private set; }
			public Guid Version { get; private set; }
			public int Level { get; private set; }
			public Collection<IDocumentProperty> Properties { get; private set; }
			public IEnumerable<IDocument> Children { get; private set; }

			private readonly List<string> _keysAdded = new List<string>();
			private void ValidateAndSetProperty(IDictionary<string, string> valueDictionary, Action<string> setProperty, params string[] potentialKeys)
			{
				foreach (var s in potentialKeys)
				{
					if (valueDictionary[s] == null)
						throw new FormatException("The valueDictionary is not formatted correctly and is missing the '" + s + "' element");
					setProperty(valueDictionary[s]);
					_keysAdded.Add(s);
					break;
				}

			}
		}
	}
}