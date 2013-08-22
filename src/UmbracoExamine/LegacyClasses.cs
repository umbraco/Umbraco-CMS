using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.IO;
using Examine;
using Examine.LuceneEngine.Config;
using UmbracoExamine;
using Lucene.Net.Analysis;
using Examine.SearchCriteria;
using Lucene.Net.Search;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco.cms.businesslogic;
using System.Xml;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Config
{
    /// <summary>
    /// This class is defined purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.Config.IndexSets")]
    public class ExamineLuceneIndexes : Examine.LuceneEngine.Config.IndexSets
    {
    }

}

namespace UmbracoExamine
{
    /// <summary>
    /// This class purely exists to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new UmbracoExamineSearcher instead")]
    public class LuceneExamineSearcher : UmbracoExamineSearcher
    {
        #region Constructors
        [Obsolete]
        public LuceneExamineSearcher()
            : base() { }

        [SecuritySafeCritical]
        [Obsolete]
        public LuceneExamineSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer) { }
        #endregion
    }

    /// <summary>
    /// This class purely exists to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new UmbracoMemberIndexer instead")]
    public class MemberLuceneExamineIndexer : UmbracoMemberIndexer
    {
        #region Constructors
        [Obsolete]
        public MemberLuceneExamineIndexer()
            : base() { }

        [SecuritySafeCritical]
        [Obsolete]
        public MemberLuceneExamineIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async) { }
        #endregion
    }

    /// <summary>
    /// This class purely exists to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new UmbracoContentIndexer instead")]
    public class LuceneExamineIndexer : UmbracoContentIndexer
    {
        #region Constructors
        [Obsolete]
        public LuceneExamineIndexer()
            : base() { }

        [SecuritySafeCritical]
        [Obsolete]
        public LuceneExamineIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async) { }
        #endregion
    }

    //Was going to include this for backwards compatibility BUT this will cause too many other problems with people importing both
    //namespaces

    //[Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //public static class LinqXmlExtensions
    //{
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static XDocument ToXDocument(this IEnumerable<XElement> elements)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.ToXDocument(elements);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static XDocument ToXDocument(this XPathNodeIterator xml)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.ToXDocument(xml);
    //    }
    //    [Obsolete("Use the new UmbracoExamine.ContentExtensions")]
    //    public static XDocument ToXDocument(this Content node, bool cacheOnly)
    //    {
    //        return ContentExtensions.ToXDocument(node, cacheOnly);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static XElement ToXElement(this XmlNode node)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.ToXElement(node);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static bool UmbIsElement(this XElement x)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.IsExamineElement(x);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static bool UmbIsProperty(this XElement x, string alias)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.IsExamineProperty(x, alias);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static string UmbNodeTypeAlias(this XElement x)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.ExamineNodeTypeAlias(x);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static string UmbSelectDataValue(this XElement xml, string alias)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.SelectExamineDataValue(xml, alias);
    //    }
    //    [Obsolete("Use the new Examine.LuceneEngine.ExamineXmlExtensions")]
    //    public static string UmbSelectPropertyValue(this XElement x, string alias)
    //    {
    //        return Examine.LuceneEngine.ExamineXmlExtensions.SelectExaminePropertyValue(x, alias);
    //    }
    //}

}

namespace UmbracoExamine.SearchCriteria
{

    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
    public static class LuceneSearchExtensions
    {
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue Boost(this string s, float boost)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.Boost(s, boost);
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue Escape(this string s)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.Escape(s);
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue Fuzzy(this string s)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.Fuzzy(s);
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue Fuzzy(this string s, float fuzzieness)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.Fuzzy(s, fuzzieness);
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue MultipleCharacterWildcard(this string s)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.MultipleCharacterWildcard(s);
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue Proximity(this string s, float proximity)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.Proximity(s, Convert.ToInt32(proximity));
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        public static IExamineValue SingleCharacterWildcard(this string s)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.SingleCharacterWildcard(s);
        }
        //[Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        //public static string Then(this IExamineValue examineValue, string s)
        //{
        //    return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.Then(examineValue, s);
        //}
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        [SecuritySafeCritical]
        public static BooleanOperation ToBooleanOperation(this BooleanClause.Occur o)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.ToBooleanOperation(o);
        }
        [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions")]
        [SecuritySafeCritical]
        public static BooleanClause.Occur ToLuceneOccurance(this BooleanOperation o)
        {
            return Examine.LuceneEngine.SearchCriteria.LuceneSearchExtensions.ToLuceneOccurance(o);
        }

    }

    /// <summary>
    /// This exists purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchCriteria")]
    public class LuceneSearchCriteria : Examine.LuceneEngine.SearchCriteria.LuceneSearchCriteria
    {
        [Obsolete]
        [SecuritySafeCritical]
        public LuceneSearchCriteria(string type, Analyzer analyzer, string[] fields, bool allowLeadingWildcards, Examine.SearchCriteria.BooleanOperation occurance)
            : base(type, analyzer, fields, allowLeadingWildcards, occurance) { }
    }

    /// <summary>
    /// This exists purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneBooleanOperation instead")]
    public class LuceneBooleanOperation : Examine.LuceneEngine.SearchCriteria.LuceneBooleanOperation
    {
        [Obsolete]
        public LuceneBooleanOperation(LuceneSearchCriteria search) : base(search) { }
    }

    /// <summary>
    /// This exists purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneQuery instead")]
    public class LuceneQuery : Examine.LuceneEngine.SearchCriteria.LuceneQuery
    {
        [Obsolete]
        [SecuritySafeCritical]
        public LuceneQuery(LuceneSearchCriteria search, Lucene.Net.Search.BooleanClause.Occur occurance)
            : base(search, occurance) { }
    }
}