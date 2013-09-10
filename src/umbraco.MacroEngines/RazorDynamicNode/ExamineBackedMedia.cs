using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Providers;
using Umbraco.Core;
using UmbracoExamine;
using Lucene.Net.Documents;
using umbraco.interfaces;
using System.IO;


namespace umbraco.MacroEngines
{

    public class ExamineBackedMedia
    {
        private readonly BaseIndexProvider _indexer;
        private readonly BaseSearchProvider _searcher;
        //Custom properties won't be available
        public bool WasLoadedFromExamine;

        public static ExamineBackedMedia GetUmbracoMedia(int id)
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
                    return new ExamineBackedMedia(results.First());
                }
            }
            catch (FileNotFoundException)
            {
                //Currently examine is throwing FileNotFound exceptions when we have a loadbalanced filestore and a node is published in umbraco
                //See this thread: http://examine.cdodeplex.com/discussions/264341
                //Catch the exception here for the time being, and just fallback to GetMedia

            }

            var media = umbraco.library.GetMedia(id, true);
            if (media != null && media.Current != null)
            {
                media.MoveNext();
                return new ExamineBackedMedia(media.Current);
            }

            return null;
        }

        public ExamineBackedMedia(XPathNavigator xpath)
        {
            if (xpath == null) throw new ArgumentNullException("xpath");
            Name = xpath.GetAttribute("nodeName", "");
            Values = new Dictionary<string, string>();
            var result = xpath.SelectChildren(XPathNodeType.Element);
            //add the attributes e.g. id, parentId etc
            if (result.Current.HasAttributes)
            {
                if (result.Current.MoveToFirstAttribute())
                {
                    Values.Add(result.Current.Name, result.Current.Value);
                    while (result.Current.MoveToNextAttribute())
                    {
                        Values.Add(result.Current.Name, result.Current.Value);
                    }
                    result.Current.MoveToParent();
                }
            }
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
                    Values.Add(result.Current.Name, value);
                }

            }
            WasLoadedFromExamine = false;
        }

        public ExamineBackedMedia(SearchResult result)
        {
            if (result == null) throw new ArgumentNullException("result");
            Name = result.Fields["nodeName"];
            Values = result.Fields;
            WasLoadedFromExamine = true;
        }

        /// <summary>
        /// Internal constructor used for unit tests
        /// </summary>
        /// <param name="result"></param>
        /// <param name="indexer"></param>
        /// <param name="searcher"></param>
        internal ExamineBackedMedia(SearchResult result, BaseIndexProvider indexer, BaseSearchProvider searcher)
            : this(result)
        {
            _indexer = indexer;
            _searcher = searcher;
        }

        public IProperty LoadCustomPropertyNotFoundInExamine(string alias, out bool propertyExists)
        {
            //custom property, not loaded from examine
            //have to do a getmedia and get it that way, but then add it to the cache
            var media = umbraco.library.GetMedia(this.Id, true);
            if (media != null && media.Current != null)
            {
                media.MoveNext();
                XPathNavigator xpath = media.Current;
                var result = xpath.SelectChildren(XPathNodeType.Element);
                while (result.MoveNext())
                {
                    if (result.Current != null && !result.Current.HasAttributes)
                    {
                        if (string.Equals(result.Current.Name, alias))
                        {
                            string value = result.Current.Value;
                            if (string.IsNullOrEmpty(value))
                            {
                                value = result.Current.OuterXml;
                            }
                            Values.Add(result.Current.Name, value);
                            propertyExists = true;
                            return new PropertyResult(alias, value);
                        }
                    }
                }
            }
            propertyExists = false;
            return null;
        }
        public string Name { get; private set; }
        internal IDictionary<string, string> Values { get; set; }

        public Lazy<ExamineBackedMedia> Parent
        {
            get
            {
                var parentId = GetValueAsInt("parentID");
                return parentId != 0 
                    ? new Lazy<ExamineBackedMedia>(() => GetUmbracoMedia(parentId)) 
                    : null;
            }
        }

        public int ParentId
        {
            get
            {
                return GetValueAsInt("parentID");  
            }
        }

        public int Id
        {
            get
            {
                return GetValueAsInt("id");  
            }
        }

        public int SortOrder
        {
            get
            {
                return GetValueAsInt("sortOrder");  
            }
        }

        public string Url
        {
            get
            {
                return GetValueAsString(Constants.Conventions.Media.File);
            }
        }

        public string UrlName
        {
            get
            {
                return GetValueAsString("urlName");  
            }
        }

        public string NodeTypeAlias
        {
            get
            {
                return GetValueAsString("nodeTypeAlias");  
            }
        }

        public string WriterName
        {
            get
            {
                return GetValueAsString("writerName");  
            }
        }

        public string CreatorName
        {
            get
            {
                return GetValueAsString("writerName");  
            }
        }

        public int WriterID
        {
            get
            {
                return GetValueAsInt("writerID");                     
            }
        }

        public int CreatorID
        {
            get
            {
                //there is no creator id in xml, so will have to be the same as writer id :(
                return GetValueAsInt("writerID");                
            }
        }

        public string Path
        {
            get
            {
                return GetValueAsString("__Path");                
            }
        }

        public DateTime CreateDate
        {
            get
            {
                return GetvalueAsDateTime("createDate");                
            }
        }

        public DateTime UpdateDate
        {
            get
            {
                return GetvalueAsDateTime("updateDate");                
            }
        }

        public Guid Version
        {
            get
            {
                return GetValueAsGuid("version");               
            }
        }

        public string NiceUrl
        {
            get
            {
                return GetValueAsString(Constants.Conventions.Media.File);
            }
        }

        public int Level
        {
            get
            {
                var level = GetValueAsInt("level");
                if (level != 0) return level;

                //return it based on the path if level is not there
                string value;
                return Values.TryGetValue("__Path", out value) ? value.Split(',').Length : 0;
            }
        }

        private BaseIndexProvider GetIndexer()
        {
            return _indexer ?? ExamineManager.Instance.IndexProviderCollection["InternalIndexer"];
        }

        private BaseSearchProvider GetSearcher()
        {
            return _searcher ?? ExamineManager.Instance.SearchProviderCollection["InternalSearcher"];
        }

        private DateTime GetvalueAsDateTime(string key)
        {
            var dt = DateTime.MinValue;
            string value = null;
            if (Values.TryGetValue(key, out value))
            {
                if (DateTime.TryParse(value, out dt))
                {
                    return dt;
                }
                //normally dates in lucene are stored with a specific lucnene date format so we'll try to parse that.
                try
                {
                    return DateTools.StringToDate(value);
                }
                catch (FormatException)
                {
                    //it could not be formatted :(
                }
            }
            return dt;
        }

        private Guid GetValueAsGuid(string key)
        {
            string value;
            if (Values.TryGetValue(key, out value))
            {
                Guid gId;
                if (Guid.TryParse(value, out gId))
                {
                    return gId;
                }
            }
            return Guid.Empty;
        }

        private string GetValueAsString(string key)
        {
            string value;
            return Values.TryGetValue(key, out value) ? value : null;
        }

        private int GetValueAsInt(string key)
        {
            var val = 0;
            string value;
            if (Values.TryGetValue(key, out value))
            {                
                if (int.TryParse(value, out val))
                {
                    return val;
                }
            }
            return val;
        }

        public List<IProperty> PropertiesAsList
        {
            get
            {
                var internalProperties = new[] {
                    "id", "nodeName", "updateDate", "writerName", "path", "nodeTypeAlias",
                    "parentID", "__NodeId", "__IndexType", "__Path", "__NodeTypeAlias", 
                    "__nodeName"
                };
                return Values
                    .Where(kvp => !internalProperties.Contains(kvp.Key))
                    .ToList()
                    .ConvertAll(kvp => new PropertyResult(kvp.Key, kvp.Value))
                    .Cast<IProperty>()
                    .ToList();
            }
        }

        public Lazy<List<ExamineBackedMedia>> ChildrenAsList
        {
            get
            {
                return new Lazy<List<ExamineBackedMedia>>(() => GetChildrenMedia(this.Id));
            }
        }

        private List<ExamineBackedMedia> GetChildrenMedia(int parentId)
        {
            try
            {
                //first check in Examine as this is WAY faster
                var searcher = GetSearcher();
                var indexer = GetIndexer();
                var criteria = searcher.CreateSearchCriteria("media");
                var filter = criteria.ParentId(parentId);
                ISearchResults results;
                var useLuceneSort = indexer != null && indexer.IndexerData.StandardFields.Any(x => x.Name.InvariantEquals("sortOrder") && x.EnableSorting);
                if (useLuceneSort)
                {
                    //we have a sortOrder field declared to be sorted, so we'll use Examine
                    results = searcher.Search(
                        filter.And().OrderBy(new SortableField("sortOrder", SortType.Int)).Compile());
                }
                else
                {
                    results = searcher.Search(filter.Compile());
                }
                
                if (results.Any())
                {
                    return useLuceneSort
                                ? results.Select(result => new ExamineBackedMedia(result)).ToList() //will already be sorted by lucene
                                : results.Select(result => new ExamineBackedMedia(result)).OrderBy(x => x.SortOrder).ToList();
                }
            }
            catch (FileNotFoundException)
            {
                //Currently examine is throwing FileNotFound exceptions when we have a loadbalanced filestore and a node is published in umbraco
                //See this thread: http://examine.cdodeplex.com/discussions/264341
                //Catch the exception here for the time being, and just fallback to GetMedia
            }

            var media = umbraco.library.GetMedia(parentId, true);
            if (media != null && media.Current != null)
            {
                media.MoveNext();
                var children = media.Current.SelectChildren(XPathNodeType.Element);
                var mediaList = new List<ExamineBackedMedia>();
                while (children != null && children.Current != null)
                {
                    if (children.MoveNext())
                    {
                        if (children.Current.Name != "contents")
                        {
                            //make sure it's actually a node, not a property 
                            if (!string.IsNullOrEmpty(children.Current.GetAttribute("path", "")) &&
                                !string.IsNullOrEmpty(children.Current.GetAttribute("id", "")) &&
                                !string.IsNullOrEmpty(children.Current.GetAttribute("version", "")))
                            {
                                mediaList.Add(new ExamineBackedMedia(children.Current));
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return mediaList;
            }

            return null;
        }

        public IProperty GetProperty(string Alias)
        {
            bool exists = false;
            return GetProperty(Alias, out exists);
        }

        public IProperty GetProperty(string alias, out bool propertyExists)
        {
            string value = null;

            //First, try to get the 'raw' value, if that doesn't work try to get the normal one
            if (Values.TryGetValue(UmbracoContentIndexer.RawFieldPrefix + alias, out value)
                || Values.TryGetValue(alias, out value))
            {
                propertyExists = true;
                return new PropertyResult(alias, value);
            }

            propertyExists = false;
            return null;
        }

    }
}