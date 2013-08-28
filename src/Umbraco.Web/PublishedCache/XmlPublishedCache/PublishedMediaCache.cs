using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Providers;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;
using Umbraco.Web.Models;
using UmbracoExamine;
using umbraco;
using umbraco.cms.businesslogic;
using ContentType = umbraco.cms.businesslogic.ContentType;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
	/// <summary>
	/// An IPublishedMediaStore that first checks for the media in Examine, and then reverts to the database
	/// </summary>
	/// <remarks>
	/// NOTE: In the future if we want to properly cache all media this class can be extended or replaced when these classes/interfaces are exposed publicly.
	/// </remarks>
	internal class PublishedMediaCache : IPublishedMediaCache
	{
		public PublishedMediaCache()
		{			
		}

	    /// <summary>
	    /// Generally used for unit testing to use an explicit examine searcher
	    /// </summary>
	    /// <param name="searchProvider"></param>
	    /// <param name="indexProvider"></param>
	    internal PublishedMediaCache(BaseSearchProvider searchProvider, BaseIndexProvider indexProvider)
	    {
		    _searchProvider = searchProvider;
		    _indexProvider = indexProvider;
		}

	    private readonly BaseSearchProvider _searchProvider;
        private readonly BaseIndexProvider _indexProvider;

        public virtual IPublishedContent GetById(UmbracoContext umbracoContext, bool preview, int nodeId)
		{
			return GetUmbracoMedia(nodeId);
		}

		public virtual IEnumerable<IPublishedContent> GetAtRoot(UmbracoContext umbracoContext, bool preview)
		{
			var rootMedia = global::umbraco.cms.businesslogic.media.Media.GetRootMedias();
			var result = new List<IPublishedContent>();
			//TODO: need to get a ConvertFromMedia method but we'll just use this for now.
			foreach (var media in rootMedia
				.Select(m => global::umbraco.library.GetMedia(m.Id, true))
				.Where(media => media != null && media.Current != null))
			{
				media.MoveNext();
				result.Add(ConvertFromXPathNavigator(media.Current));
			}
			return result;
		}

        public virtual IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, string xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
        }

        public virtual IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
        }
        
        public virtual IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, string xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
        }

        public virtual IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
        }

        public virtual XPathNavigator GetXPathNavigator(UmbracoContext umbracoContext, bool preview)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
        }

        public virtual bool HasContent(UmbracoContext context, bool preview) { throw new NotImplementedException(); }

        private ExamineManager GetExamineManagerSafe()
		{
			try
			{
				return ExamineManager.Instance;
			}
			catch (TypeInitializationException)
			{
				return null;
			}
		}

	    private BaseIndexProvider GetIndexProviderSafe()
	    {
            if (_indexProvider != null)
                return _indexProvider;

            var eMgr = GetExamineManagerSafe();
            if (eMgr != null)
            {
                try
                {
                    //by default use the InternalSearcher
                    return eMgr.IndexProviderCollection["InternalIndexer"];
                }
                catch (Exception ex)
                {                
                    LogHelper.Error<PublishedMediaCache>("Could not retreive the InternalIndexer", ex);
                    //something didn't work, continue returning null.
                }
            }
            return null;
	    }

	    private BaseSearchProvider GetSearchProviderSafe()
		{
			if (_searchProvider != null)
				return _searchProvider;

			var eMgr = GetExamineManagerSafe();
			if (eMgr != null)
			{
				try
				{
					//by default use the InternalSearcher
					return eMgr.SearchProviderCollection["InternalSearcher"];
				}
				catch (FileNotFoundException)
				{
					//Currently examine is throwing FileNotFound exceptions when we have a loadbalanced filestore and a node is published in umbraco
					//See this thread: http://examine.cdodeplex.com/discussions/264341
					//Catch the exception here for the time being, and just fallback to GetMedia
					//TODO: Need to fix examine in LB scenarios!
				}
			}
			return null;
		}

		private IPublishedContent GetUmbracoMedia(int id)
		{
			var searchProvider = GetSearchProviderSafe();

			if (searchProvider != null)
			{
				try
				{
					//first check in Examine as this is WAY faster
					var criteria = searchProvider.CreateSearchCriteria("media");
				    
                    var filter = criteria.Id(id).Not().Field(UmbracoContentIndexer.IndexPathFieldName, "-1,-21,".MultipleCharacterWildcard());
                    //the above filter will create a query like this, NOTE: That since the use of the wildcard, it automatically escapes it in Lucene.
                    //+(+__NodeId:3113 -__Path:-1,-21,*) +__IndexType:media
                    
					var results = searchProvider.Search(filter.Compile());
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
			}

			var media = global::umbraco.library.GetMedia(id, true);
			if (media != null && media.Current != null)
			{
				media.MoveNext();
				var moved = media.Current.MoveToFirstChild();
				//first check if we have an error
				if (moved)
				{
					if (media.Current.Name.InvariantEquals("error"))
					{
						return null;
					}	
				}
				if (moved)
				{
					//move back to the parent and return
					media.Current.MoveToParent();	
				}
				return ConvertFromXPathNavigator(media.Current);
			}

			return null;
		}

		internal IPublishedContent ConvertFromSearchResult(SearchResult searchResult)
		{
			//NOTE: Some fields will not be included if the config section for the internal index has been 
            //mucked around with. It should index everything and so the index definition should simply be:
            // <IndexSet SetName="InternalIndexSet" IndexPath="~/App_Data/TEMP/ExamineIndexes/Internal/" />
			

			var values = new Dictionary<string, string>(searchResult.Fields);
			//we need to ensure some fields exist, because of the above issue
			if (!new []{"template", "templateId"}.Any(values.ContainsKey)) 
				values.Add("template", 0.ToString());
			if (!new[] { "sortOrder" }.Any(values.ContainsKey))
				values.Add("sortOrder", 0.ToString());
			if (!new[] { "urlName" }.Any(values.ContainsKey))
				values.Add("urlName", "");
			if (!new[] { "nodeType" }.Any(values.ContainsKey))
				values.Add("nodeType", 0.ToString());
			if (!new[] { "creatorName" }.Any(values.ContainsKey))
				values.Add("creatorName", "");
			if (!new[] { "writerID" }.Any(values.ContainsKey))
				values.Add("writerID", 0.ToString());
			if (!new[] { "creatorID" }.Any(values.ContainsKey))
				values.Add("creatorID", 0.ToString());
			if (!new[] { "createDate" }.Any(values.ContainsKey))
				values.Add("createDate", default(DateTime).ToIsoString());
			if (!new[] { "level" }.Any(values.ContainsKey))
			{				
				values.Add("level", values["__Path"].Split(',').Length.ToString());
			}


			return new DictionaryPublishedContent(values,
			                                      d => d.ParentId != -1 //parent should be null if -1
				                                           ? GetUmbracoMedia(d.ParentId)
				                                           : null,
			                                      //callback to return the children of the current node
			                                      d => GetChildrenMedia(d.Id),
			                                      GetProperty,
			                                      true);
		}

		internal IPublishedContent ConvertFromXPathNavigator(XPathNavigator xpath)
		{
			if (xpath == null) throw new ArgumentNullException("xpath");

			var values = new Dictionary<string, string> {{"nodeName", xpath.GetAttribute("nodeName", "")}};
			if (!UmbracoSettings.UseLegacyXmlSchema)
			{
				values.Add("nodeTypeAlias", xpath.Name);
			}
			
			var result = xpath.SelectChildren(XPathNodeType.Element);
			//add the attributes e.g. id, parentId etc
			if (result.Current != null && result.Current.HasAttributes)
			{
				if (result.Current.MoveToFirstAttribute())
				{
					//checking for duplicate keys because of the 'nodeTypeAlias' might already be added above.
					if (!values.ContainsKey(result.Current.Name))
					{
						values.Add(result.Current.Name, result.Current.Value);	
					}
					while (result.Current.MoveToNextAttribute())
					{
						if (!values.ContainsKey(result.Current.Name))
						{
							values.Add(result.Current.Name, result.Current.Value);
						}						
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

			return new DictionaryPublishedContent(values, 
				d => d.ParentId != -1 //parent should be null if -1
					? GetUmbracoMedia(d.ParentId) 
					: null,
				//callback to return the children of the current node based on the xml structure already found
				d => GetChildrenMedia(d.Id, xpath),
				GetProperty,
				false);
		}

		/// <summary>
		/// We will need to first check if the document was loaded by Examine, if so we'll need to check if this property exists 
		/// in the results, if it does not, then we'll have to revert to looking up in the db. 
		/// </summary>
		/// <param name="dd"> </param>
		/// <param name="alias"></param>
		/// <returns></returns>
		private IPublishedContentProperty GetProperty(DictionaryPublishedContent dd, string alias)
		{
			if (dd.LoadedFromExamine)
			{
				//if this is from Examine, lets check if the alias does not exist on the document
				if (dd.Properties.All(x => x.Alias != alias))
				{
					//ok it doesn't exist, we might assume now that Examine didn't index this property because the index is not set up correctly
					//so before we go loading this from the database, we can check if the alias exists on the content type at all, this information
					//is cached so will be quicker to look up.
					if (dd.Properties.Any(x => x.Alias == UmbracoContentIndexer.NodeTypeAliasFieldName))
					{
						var aliasesAndNames = ContentType.GetAliasesAndNames(dd.Properties.First(x => x.Alias.InvariantEquals(UmbracoContentIndexer.NodeTypeAliasFieldName)).Value.ToString());
						if (aliasesAndNames != null)
						{
							if (!aliasesAndNames.ContainsKey(alias))
							{
								//Ok, now we know it doesn't exist on this content type anyways
								return null;
							}
						}
					}

					//if we've made it here, that means it does exist on the content type but not in examine, we'll need to query the db :(
					var media = global::umbraco.library.GetMedia(dd.Id, true);
					if (media != null && media.Current != null)
					{
						media.MoveNext();
						var mediaDoc = ConvertFromXPathNavigator(media.Current);
						return mediaDoc.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
					}					
				}							
			}
			
            //We've made it here which means that the value is stored in the Examine index.
            //We are going to check for a special field however, that is because in some cases we store a 'Raw'
            //value in the index such as for xml/html.
            var rawValue = dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(UmbracoContentIndexer.RawFieldPrefix + alias));
		    return rawValue
		           ?? dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
		}

		/// <summary>
		/// A Helper methods to return the children for media whther it is based on examine or xml
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="xpath"></param>
		/// <returns></returns>
		private IEnumerable<IPublishedContent> GetChildrenMedia(int parentId, XPathNavigator xpath = null)
		{	

			//if there is no navigator, try examine first, then re-look it up
			if (xpath == null)
			{
				var searchProvider = GetSearchProviderSafe();

				if (searchProvider != null)
				{
					try
					{
						//first check in Examine as this is WAY faster
						var criteria = searchProvider.CreateSearchCriteria("media");
                        var filter = criteria.ParentId(parentId);
					    ISearchResults results;

                        //we want to check if the indexer for this searcher has "sortOrder" flagged as sortable.
                        //if so, we'll use Lucene to do the sorting, if not we'll have to manually sort it (slower).
                        var indexer = GetIndexProviderSafe();
					    var useLuceneSort = indexer != null && indexer.IndexerData.StandardFields.Any(x => x.Name.InvariantEquals("sortOrder") && x.EnableSorting);
                        if (useLuceneSort)
                        {
                            //we have a sortOrder field declared to be sorted, so we'll use Examine
                            results = searchProvider.Search(
                                filter.And().OrderBy(new SortableField("sortOrder", SortType.Int)).Compile());
                        }
                        else
                        {
                            results = searchProvider.Search(filter.Compile());
                        }
						
						if (results.Any())
						{
						    return useLuceneSort
						               ? results.Select(ConvertFromSearchResult) //will already be sorted by lucene
						               : results.Select(ConvertFromSearchResult).OrderBy(x => x.SortOrder);
						}
						else
						{
						    //if there's no result then return null. Previously we defaulted back to library.GetMedia below
                            //but this will always get called for when we are getting descendents since many items won't have 
                            //children and then we are hitting the database again!
                            //So instead we're going to rely on Examine to have the correct results like it should.
						    return Enumerable.Empty<IPublishedContent>();
						}
					}
					catch (FileNotFoundException)
					{
						//Currently examine is throwing FileNotFound exceptions when we have a loadbalanced filestore and a node is published in umbraco
						//See this thread: http://examine.cdodeplex.com/discussions/264341
						//Catch the exception here for the time being, and just fallback to GetMedia
					}	
				}

                //falling back to get media

				var media = library.GetMedia(parentId, true);
				if (media != null && media.Current != null)
				{
				    media.MoveNext();
					xpath = media.Current;
				}
				else
				{
                    return Enumerable.Empty<IPublishedContent>();
				}
			}

			//The xpath might be the whole xpath including the current ones ancestors so we need to select the current node
			var item = xpath.Select("//*[@id='" + parentId + "']");
			if (item.Current == null)
			{
                return Enumerable.Empty<IPublishedContent>();
			}
			var children = item.Current.SelectChildren(XPathNodeType.Element);

			var mediaList = new List<IPublishedContent>();
			foreach(XPathNavigator x in children)
			{
				//NOTE: I'm not sure why this is here, it is from legacy code of ExamineBackedMedia, but
				// will leave it here as it must have done something!
				if (x.Name != "contents")
				{
					//make sure it's actually a node, not a property 
					if (!string.IsNullOrEmpty(x.GetAttribute("path", "")) &&
						!string.IsNullOrEmpty(x.GetAttribute("id", "")))
					{
						mediaList.Add(ConvertFromXPathNavigator(x));
					}
				}	
			}
			return mediaList;
		}

		/// <summary>
		/// An IPublishedContent that is represented all by a dictionary.
		/// </summary>
		/// <remarks>
		/// This is a helper class and definitely not intended for public use, it expects that all of the values required 
		/// to create an IPublishedContent exist in the dictionary by specific aliases.
		/// </remarks>
		internal class DictionaryPublishedContent : PublishedContentBase
		{

			public DictionaryPublishedContent(
				IDictionary<string, string> valueDictionary, 
				Func<DictionaryPublishedContent, IPublishedContent> getParent,
				Func<DictionaryPublishedContent, IEnumerable<IPublishedContent>> getChildren,
				Func<DictionaryPublishedContent, string, IPublishedContentProperty> getProperty,
				bool fromExamine)
			{
				if (valueDictionary == null) throw new ArgumentNullException("valueDictionary");
				if (getParent == null) throw new ArgumentNullException("getParent");
				if (getProperty == null) throw new ArgumentNullException("getProperty");

				_getParent = getParent;
				_getChildren = getChildren;
				_getProperty = getProperty;

				LoadedFromExamine = fromExamine;

				ValidateAndSetProperty(valueDictionary, val => _id = int.Parse(val), "id", "nodeId", "__NodeId"); //should validate the int!
				ValidateAndSetProperty(valueDictionary, val => _templateId = int.Parse(val), "template", "templateId");
				ValidateAndSetProperty(valueDictionary, val => _sortOrder = int.Parse(val), "sortOrder");
				ValidateAndSetProperty(valueDictionary, val => _name = val, "nodeName", "__nodeName");
				ValidateAndSetProperty(valueDictionary, val => _urlName = val, "urlName");
				ValidateAndSetProperty(valueDictionary, val => _documentTypeAlias = val, "nodeTypeAlias", UmbracoContentIndexer.NodeTypeAliasFieldName);
				ValidateAndSetProperty(valueDictionary, val => _documentTypeId = int.Parse(val), "nodeType");
				ValidateAndSetProperty(valueDictionary, val => _writerName = val, "writerName");
				ValidateAndSetProperty(valueDictionary, val => _creatorName = val, "creatorName", "writerName"); //this is a bit of a hack fix for: U4-1132
				ValidateAndSetProperty(valueDictionary, val => _writerId = int.Parse(val), "writerID");
				ValidateAndSetProperty(valueDictionary, val => _creatorId = int.Parse(val), "creatorID", "writerID"); //this is a bit of a hack fix for: U4-1132
				ValidateAndSetProperty(valueDictionary, val => _path = val, "path", "__Path");
				ValidateAndSetProperty(valueDictionary, val => _createDate = ParseDateTimeValue(val), "createDate");
				ValidateAndSetProperty(valueDictionary, val => _updateDate = ParseDateTimeValue(val), "updateDate");
				ValidateAndSetProperty(valueDictionary, val => _level = int.Parse(val), "level");
				ValidateAndSetProperty(valueDictionary, val =>
					{
						int pId;
						ParentId = -1;
						if (int.TryParse(val, out pId))
						{
							ParentId = pId;
						}						
					}, "parentID");

				_properties = new Collection<IPublishedContentProperty>();

				//loop through remaining values that haven't been applied
				foreach (var i in valueDictionary.Where(x => !_keysAdded.Contains(x.Key)))
				{
					//this is taken from examine
					_properties.Add(i.Key.InvariantStartsWith("__") 
					               	? new PropertyResult(i.Key, i.Value, Guid.Empty, PropertyResultType.CustomProperty) 
					               	: new PropertyResult(i.Key, i.Value, Guid.Empty, PropertyResultType.UserProperty));
				}
			}

			private DateTime ParseDateTimeValue(string val)
			{
				if (LoadedFromExamine)
				{
					try
					{
						//we might need to parse the date time using Lucene converters
						return DateTools.StringToDate(val);
					}
					catch (FormatException)
					{
						//swallow exception, its not formatted correctly so revert to just trying to parse
					}
				}

				return DateTime.Parse(val);
			}

			/// <summary>
			/// Flag to get/set if this was laoded from examine cache
			/// </summary>
			internal bool LoadedFromExamine { get; private set; }

			private readonly Func<DictionaryPublishedContent, IPublishedContent> _getParent;
			private readonly Func<DictionaryPublishedContent, IEnumerable<IPublishedContent>> _getChildren;
			private readonly Func<DictionaryPublishedContent, string, IPublishedContentProperty> _getProperty;

			/// <summary>
			/// Returns 'Media' as the item type
			/// </summary>
			public override PublishedItemType ItemType
			{
				get { return PublishedItemType.Media; }
			}

			public override IPublishedContent Parent
			{
				get { return _getParent(this); }
			}

			public int ParentId { get; private set; }
			public override int Id
			{
				get { return _id; }
			}

			public override int TemplateId
			{
				get
				{
					//TODO: should probably throw a not supported exception since media doesn't actually support this.
					return _templateId;
				}
			}

			public override int SortOrder
			{
				get { return _sortOrder; }
			}

			public override string Name
			{
				get { return _name; }
			}

			public override string UrlName
			{
				get { return _urlName; }
			}

			public override string DocumentTypeAlias
			{
				get { return _documentTypeAlias; }
			}

			public override int DocumentTypeId
			{
				get { return _documentTypeId; }
			}

			public override string WriterName
			{
				get { return _writerName; }
			}

			public override string CreatorName
			{
				get { return _creatorName; }
			}

			public override int WriterId
			{
				get { return _writerId; }
			}

			public override int CreatorId
			{
				get { return _creatorId; }
			}

			public override string Path
			{
				get { return _path; }
			}

			public override DateTime CreateDate
			{
				get { return _createDate; }
			}

			public override DateTime UpdateDate
			{
				get { return _updateDate; }
			}

			public override Guid Version
			{
				get { return _version; }
			}

			public override int Level
			{
				get { return _level; }
			}

			public override ICollection<IPublishedContentProperty> Properties
			{
				get { return _properties; }
			}

			public override IEnumerable<IPublishedContent> Children
			{
				get { return _getChildren(this); }
			}

			public override IPublishedContentProperty GetProperty(string alias)
			{
				return _getProperty(this, alias);
			}

			private readonly List<string> _keysAdded = new List<string>();
			private int _id;
			private int _templateId;
			private int _sortOrder;
			private string _name;
			private string _urlName;
			private string _documentTypeAlias;
			private int _documentTypeId;
			private string _writerName;
			private string _creatorName;
			private int _writerId;
			private int _creatorId;
			private string _path;
			private DateTime _createDate;
			private DateTime _updateDate;
			private Guid _version;
			private int _level;
			private readonly ICollection<IPublishedContentProperty> _properties;

			private void ValidateAndSetProperty(IDictionary<string, string> valueDictionary, Action<string> setProperty, params string[] potentialKeys)
			{
				var key = potentialKeys.FirstOrDefault(x => valueDictionary.ContainsKey(x) && valueDictionary[x] != null);
				if (key == null)
				{
					throw new FormatException("The valueDictionary is not formatted correctly and is missing any of the  '" + string.Join(",", potentialKeys) + "' elements");
				}

				setProperty(valueDictionary[key]);
				_keysAdded.Add(key);
			}
		}
	}
}