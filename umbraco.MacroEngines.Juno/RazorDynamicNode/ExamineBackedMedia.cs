using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Examine;
using UmbracoExamine;
using Lucene.Net.Documents;
using umbraco.interfaces;


namespace umbraco.MacroEngines
{

    public class ExamineBackedMedia : INode
    {
        public static ExamineBackedMedia GetUmbracoMedia(int id)
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

            var media = umbraco.library.GetMedia(id, false);
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
            while (result.MoveNext())
            {
                if (result.Current != null && !result.Current.HasAttributes)
                {
                    Values.Add(result.Current.Name, result.Current.Value);
                }
            }
        }

        public ExamineBackedMedia(SearchResult result)
        {
            if (result == null) throw new ArgumentNullException("result");
            Name = result.Fields["nodeName"];
            Values = result.Fields;

        }

        public string Name { get; private set; }
        private IDictionary<string, string> Values { get; set; }

        //make this one lazy
        public INode Parent
        {
            get { throw new NotImplementedException(); }
        }

        public int Id
        {
            get { throw new NotImplementedException(); }
        }

        public int template
        {
            get { throw new NotImplementedException(); }
        }

        public int SortOrder
        {
            get { throw new NotImplementedException(); }
        }

        public string Url
        {
            get { throw new NotImplementedException(); }
        }

        public string UrlName
        {
            get { throw new NotImplementedException(); }
        }

        public string NodeTypeAlias
        {
            get { throw new NotImplementedException(); }
        }

        public string WriterName
        {
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
        }

        public DateTime CreateDate
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime UpdateDate
        {
            get { throw new NotImplementedException(); }
        }

        public Guid Version
        {
            get { throw new NotImplementedException(); }
        }

        public string NiceUrl
        {
            get { throw new NotImplementedException(); }
        }

        public int Level
        {
            get { throw new NotImplementedException(); }
        }

        public List<IProperty> PropertiesAsList
        {
            get { throw new NotImplementedException(); }
        }
        //make this one lazy
        public List<INode> ChildrenAsList
        {
            get { throw new NotImplementedException(); }
        }

        //make this one lazy
        public System.Data.DataTable ChildrenAsTable()
        {
            throw new NotImplementedException();
        }

        //make this one lazy
        public System.Data.DataTable ChildrenAsTable(string nodeTypeAliasFilter)
        {
            throw new NotImplementedException();
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