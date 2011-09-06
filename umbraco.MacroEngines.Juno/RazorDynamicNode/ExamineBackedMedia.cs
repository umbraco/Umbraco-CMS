using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Examine;
using UmbracoExamine;
using Lucene.Net.Documents;
using umbraco.interfaces;
using System.IO;


namespace umbraco.MacroEngines
{

    public class ExamineBackedMedia
    {
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
                        value = result.Current.OuterXml;
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
                            return new PropertyResult(alias, value, Guid.Empty);
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
                int parentId = 0;
                string value = null;
                if (Values.TryGetValue("parentID", out value))
                {
                    if (int.TryParse(value, out parentId))
                    {
                        return new Lazy<ExamineBackedMedia>(() => { return ExamineBackedMedia.GetUmbracoMedia(parentId); });
                    }
                }
                return null;
            }
        }
        public int ParentId
        {
            get
            {
                int parentId = 0;
                if (int.TryParse(Values["parentID"], out parentId))
                {
                    return parentId;
                }
                return 0;
            }
        }
        public int Id
        {
            get
            {
                int id = 0;
                string value = null;
                if (Values.TryGetValue("id", out value))
                {
                    if (int.TryParse(value, out id))
                    {
                        return id;
                    }
                }
                return 0;
            }
        }

        public int SortOrder
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Url
        {
            get
            {
                string value = null;
                if (Values.TryGetValue("umbracoFile", out value))
                {
                    return value;
                }
                return null;
            }
        }

        public string UrlName
        {
            get { throw new NotImplementedException(); }
        }

        public string NodeTypeAlias
        {
            get
            {
                string value = null;
                if (Values.TryGetValue("nodeTypeAlias", out value))
                {
                    return value;
                }
                return null;
            }
        }

        public string WriterName
        {
            get
            {
                string value = null;
                if (Values.TryGetValue("writerName", out value))
                {
                    return value;
                }
                return null;
            }
        }

        public string CreatorName
        {
            get { throw new NotImplementedException(); }
        }

        public int WriterID
        {
            get { throw new NotImplementedException(); }
        }

        public int CreatorID
        {
            get { throw new NotImplementedException(); }
        }

        public string Path
        {
            get
            {
                string value = null;
                if (Values.TryGetValue("__Path", out value))
                {
                    return value;
                }
                return null;
            }
        }

        public DateTime CreateDate
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime UpdateDate
        {
            get
            {
                DateTime dt = DateTime.MinValue;
                string value = null;
                if (Values.TryGetValue("UpdateDate", out value))
                {
                    if (DateTime.TryParse(value, out dt))
                    {
                        return dt;
                    }
                }
                return DateTime.Now;
            }
        }

        public Guid Version
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string NiceUrl
        {
            get
            {
                string value = null;
                if (Values.TryGetValue("umbracoFile", out value))
                {
                    return value;
                }
                return null;
            }
        }

        public int Level
        {
            get
            {
                string value = null;
                if (Values.TryGetValue("__Path", out value))
                {
                    return value.Split(',').Length;
                }
                return 0;
            }
        }

        public List<IProperty> PropertiesAsList
        {
            get
            {
                string[] internalProperties = new string[] {
                    "id", "nodeName", "updateDate", "writerName", "path", "nodeTypeAlias",
                    "parentID", "__NodeId", "__IndexType", "__Path", "__NodeTypeAlias", 
                    "__nodeName"
                };
                return Values
                    .Where(kvp => !internalProperties.Contains(kvp.Key))
                    .ToList()
                    .ConvertAll(kvp => new PropertyResult(kvp.Key, kvp.Value, Guid.Empty))
                    .Cast<IProperty>()
                    .ToList();
            }
        }

        public Lazy<List<ExamineBackedMedia>> ChildrenAsList
        {
            get
            {
                return new Lazy<List<ExamineBackedMedia>>(() =>
                {
                    return GetChildrenMedia(this.Id);
                });
            }
        }
        private static List<ExamineBackedMedia> GetChildrenMedia(int ParentId)
        {
            try
            {
                //first check in Examine as this is WAY faster
                var criteria = ExamineManager.Instance
                    .SearchProviderCollection["InternalSearcher"]
                    .CreateSearchCriteria("media");
                var filter = criteria.ParentId(ParentId);
                var results = ExamineManager
                    .Instance.SearchProviderCollection["InternalSearcher"]
                    .Search(filter.Compile());
                if (results.Any())
                {
                    return results.ToList().ConvertAll(result => new ExamineBackedMedia(result));
                }
            }
            catch (FileNotFoundException)
            {
                //Currently examine is throwing FileNotFound exceptions when we have a loadbalanced filestore and a node is published in umbraco
                //See this thread: http://examine.cdodeplex.com/discussions/264341
                //Catch the exception here for the time being, and just fallback to GetMedia
            }

            var media = umbraco.library.GetMedia(ParentId, true);
            if (media != null && media.Current != null)
            {
                media.MoveNext();
                var children = media.Current.SelectChildren(XPathNodeType.Element);
                List<ExamineBackedMedia> mediaList = new List<ExamineBackedMedia>();
                while (children != null && children.Current != null)
                {
                    if (children.MoveNext())
                    {
                        if (children.Current.Name != "contents")
                        {
                            mediaList.Add(new ExamineBackedMedia(children.Current));
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
            if (Values.TryGetValue(alias, out value))
            {
                propertyExists = true;
                return new PropertyResult(alias, value, Guid.Empty);
            }
            propertyExists = false;
            return null;
        }
    }
}