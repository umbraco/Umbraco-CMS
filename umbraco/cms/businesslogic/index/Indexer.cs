using System;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using System.Collections;

namespace umbraco.cms.businesslogic.index
{
    /// <summary>
    /// Indexer contains methods for populating data to the internal search of the umbraco console
    /// 
    /// 
    /// </summary>
    public class Indexer
    {

        private static string _indexDirectory = "";
        /// <summary>
        /// The relative path tto the folder where umbraco stores the files used by the Lucene searchcomponent
        /// </summary>
        public static string RelativeIndexDir = umbraco.GlobalSettings.StorageDirectory + "/_systemUmbracoIndexDontDelete";

        /// <summary>
        /// The physical path to the folder where umbraco stores the files used by the Lucene searchcomponent
        /// </summary>
        public static string IndexDirectory
        {
            get
            {
                try
                {
                    if (_indexDirectory == "")
                        _indexDirectory = GlobalSettings.FullpathToRoot + RelativeIndexDir;
                    return _indexDirectory;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                _indexDirectory = value;
            }
        }



        /// <summary>
        /// Method for accesing the Lucene indexwriter in the internal search
        /// </summary>
        /// <param name="ForceRecreation">If set to true, a new index is created</param>
        /// <returns>The lucene indexwriter</returns>
        public static IndexWriter ContentIndex(bool ForceRecreation)
        {
            if (!ForceRecreation && System.IO.Directory.Exists(IndexDirectory) &&
                new System.IO.DirectoryInfo(IndexDirectory).GetFiles().Length > 1)
                return new IndexWriter(IndexDirectory, new StandardAnalyzer(), false);
            else
            {
                IndexWriter iw = new IndexWriter(IndexDirectory, new StandardAnalyzer(), true);
                return iw;
            }
        }

        /// <summary>
        /// Method for accessing the Lucene indexreader
        /// </summary>
        /// <returns></returns>
        public static IndexReader ContentIndexReader()
        {
            return IndexReader.Open(IndexDirectory);
        }

        /// <summary>
        /// Method for reindexing data in all documents of umbraco, this is a performaceheavy invocation and should be
        /// used with care!
        /// </summary>
        public static void ReIndex(System.Web.HttpApplication context)
        {
            
            if (context != null)
                context.Application["indexerReindexing"] = "true";
            // Create new index
            IndexWriter w = ContentIndex(true);
            w.Close();

            Guid[] documents = cms.businesslogic.web.Document.getAllUniquesFromObjectType(cms.businesslogic.web.Document._objectType);

            ResetIndexCounter(context, documents.Length.ToString());

            foreach (Guid g in documents)
            {
                cms.businesslogic.web.Document d =
                new cms.businesslogic.web.Document(g);
                d.Index(true);
                if (context != null)
                {
                    context.Application["umbIndexerInfo"] = d.Text;
                    context.Application["umbIndexerCount"] =
                        ((int)context.Application["umbIndexerCount"]) + 1;
                }
            }
            if (context != null)
            {
                context.Application["indexerReindexing"] = "done";
                context.Application["umbIndexerTotal"] = "";
                context.Application["umbIndexerCount"] = "";
                context.Application["umbIndexerInfo"] = "Done reindexing at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            }
        }

        public static void ResetIndexCounter(System.Web.HttpApplication context, string documents)
        {
            if (context != null)
            {
                context.Application["umbIndexerTotal"] = documents;
                context.Application["umbIndexerCount"] = 0;
                context.Application["umbIndexerInfo"] = "";
            }
        }

        /// <summary>
        /// Determines whether this instance is reindexing.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is reindexing; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReindexing()
        {
            if (System.Web.HttpContext.Current != null)
                if (System.Web.HttpContext.Current.Application["indexerReindexing"] != null && System.Web.HttpContext.Current.Application["indexerReindexing"].ToString() != "")
                    return true;

            return false;
        }

        /// <summary>
        /// Indexes the node.
        /// </summary>
        /// <param name="ObjectType">Type of the object.</param>
        /// <param name="Id">The id.</param>
        /// <param name="Text">The text.</param>
        /// <param name="UserName">Name of the user.</param>
        /// <param name="CreateDate">The create date.</param>
        /// <param name="Fields">The fields.</param>
        /// <param name="Optimize">if set to <c>true</c> [optimize].</param>
        public static void IndexNode(Guid ObjectType, int Id, string Text, string UserName, DateTime CreateDate, Hashtable Fields, bool Optimize)
        {
            // remove node if exists
            RemoveNode(Id);

            // Add node
            Document d = new Document(); // Lucene document, not umbraco Document
            d.Add(new Field("Id", Id.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.NO));
            d.Add(new Field("Text", Text, Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.YES));
            d.Add(new Field("ObjectType", ObjectType.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.NO));
            d.Add(new Field("User", UserName, Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.NO));
            d.Add(new Field("CreateDate", convertDate(CreateDate), Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.NO));

            // Icons
            string icon = "default.png";
            if (ObjectType == cms.businesslogic.web.Document._objectType ||
                ObjectType == cms.businesslogic.media.Media._objectType ||
                ObjectType == cms.businesslogic.member.Member._objectType)
            {
                icon = new umbraco.cms.businesslogic.Content(Id).ContentType.IconUrl;
            }

            // add icon
            d.Add(new Field("Icon", icon, Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.NO));

            // Sort key
            d.Add(new Field("SortText", Text, Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.NO));

            System.Text.StringBuilder total = new System.Text.StringBuilder();
            total.Append(Text + " ");

            // Add all fields
            if (Fields != null)
            {
                IDictionaryEnumerator ide = Fields.GetEnumerator();
                while (ide.MoveNext())
                {
                    d.Add(new Field("field_" + ide.Key.ToString(), ide.Value.ToString(), Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.YES));
                    total.Append(ide.Value.ToString());
                    total.Append(" ");
                }
            }
            total.Append(" id" + Id.ToString() + " ");

            IndexWriter writer = ContentIndex(false);
            try
            {
                d.Add(new Field("Content", total.ToString(), Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.YES));
                writer.AddDocument(d);
                writer.Optimize();
                writer.Close();
            }
            catch (Exception ee)
            {
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), Id, "Error indexing node: (" + ee.ToString() + ")");
            }
            finally
            {
                writer.Close();
            }

        }

        /// <summary>
        /// Removes the node from the index.
        /// </summary>
        /// <param name="Id">The id.</param>
        public static void RemoveNode(int Id)
        {
            IndexReader ir = null;
            try
            {
                ir = ContentIndexReader();
                ir.DeleteDocuments(new Term("Id", Id.ToString()));
            }
            catch (Exception ee)
            {
                BusinessLogic.Log.Add(
                    BusinessLogic.LogTypes.Error,
                    BusinessLogic.User.GetUser(0),
                    Id,
                    "Error removing node from umbraco index: '" + ee.ToString() + "'");
            }
            finally
            {
                if (ir != null)
                    ir.Close();
            }
        }

        private static string convertDate(DateTime Date)
        {
            try
            {
                string thisYear = Date.Year.ToString();
                if (thisYear.Length == 1)
                    thisYear = "0" + thisYear;
                string thisMonth = Date.Month.ToString();
                if (thisMonth.Length == 1)
                    thisMonth = "0" + thisMonth;
                string thisDay = Date.Day.ToString();
                if (thisDay.Length == 1)
                    thisDay = "0" + thisDay;

                return thisYear + thisMonth + thisDay;
            }
            catch
            {
                return "";
            }
        }
    }

}
