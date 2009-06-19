using System;

using System.Collections;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System.Collections.Generic;
using System.Xml;

namespace umbraco.cms.businesslogic.index {
    /// <summary>
    /// Searcher is used in the internal search (autosuggestion) in the umbraco administration console
    /// </summary>
    public class searcher {

        public static XmlDocument SearchAsXml(Guid ObjectType, string Keyword, int Max)
        {
            XmlDocument result = new XmlDocument();
            result.LoadXml("<results/>");
            List<SearchItem> s = Search(ObjectType, Keyword, Max);
            foreach(SearchItem si in s) {
                XmlNode x = xmlHelper.addTextNode(result, "result", si.Description);
                x.Attributes.Append(xmlHelper.addAttribute(result, "id", si.NodeId.ToString()));
                x.Attributes.Append(xmlHelper.addAttribute(result, "icon", si.Icon));
                x.Attributes.Append(xmlHelper.addAttribute(result, "title", si.Title));
                x.Attributes.Append(xmlHelper.addAttribute(result, "author", si.Author));
                x.Attributes.Append(xmlHelper.addAttribute(result, "changeDate", si.ChangeDate.ToString("t")));
                result.DocumentElement.AppendChild(x);
            }

            return result;
        }

        /// <summary>
        /// Method for retrieving a list of documents where the keyword is present
        /// </summary>
        /// <param name="ObjectType">[not implemented] search only available for documents</param>
        /// <param name="Keyword">The word being searched for</param>
        /// <param name="Max">The maximum limit on results returned</param>
        /// <returns>A list of documentnames indexed by the id of the document</returns>
        public static List<SearchItem> Search(Guid ObjectType, string Keyword, int Max) {
            List<SearchItem> items = new List<SearchItem>();

            IndexSearcher searcher = null;
            try {
                searcher = new IndexSearcher(index.Indexer.IndexDirectory);
                QueryParser parser = new QueryParser("Content", new StandardAnalyzer());
                Query query = parser.Parse(Keyword);

                Hits hits;

                // Sorting
                SortField[] sf = { new SortField("SortText") };
                hits = searcher.Search(query, new Sort(sf));
                if (hits.Length() < Max)
                    Max = hits.Length();

                for (int i = 0; i < Max; i++) {
                    try {
                        string CreateDate = hits.Doc(i).Get("CreateDate");
                        DateTime itemDate = new DateTime(1900, 1, 1);
                        DateTime.TryParse(CreateDate, out itemDate);
                        items.Add(new SearchItem(
                            int.Parse(hits.Doc(i).Get("Id")),
                            hits.Doc(i).Get("Text"),
                            "doc.png",
                            "",
                            "",
                            itemDate,
                            new Guid(hits.Doc(i).Get("ObjectType")),
                            null));
                    } catch (Exception ee) {

                        umbraco.cms.businesslogic.index.Indexer.ReIndex((System.Web.HttpApplication)System.Web.HttpContext.Current.ApplicationInstance);
                        throw new Exception("Error adding search item", ee);
                    }
                }

            } catch (Exception ee) {
                // index is broken, fix

                throw ee;
            } finally {
                if (searcher != null)
                    searcher.Close();
            }

            return items;

        }
    }
}
