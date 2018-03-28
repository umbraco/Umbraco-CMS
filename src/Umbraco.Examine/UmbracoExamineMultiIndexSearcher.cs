//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.IO;
//using System.Linq;
//using Examine.LuceneEngine.Providers;
//using Lucene.Net.Analysis;
//using Umbraco.Examine.Config;
//using Directory = Lucene.Net.Store.Directory;

//namespace Umbraco.Examine
//{
//    public class UmbracoExamineMultiIndexSearcher : MultiIndexSearcher
//    {
//        public UmbracoExamineMultiIndexSearcher()
//        {
//        }

//        public UmbracoExamineMultiIndexSearcher(string name, IEnumerable<DirectoryInfo> indexPath, Analyzer analyzer)
//            : base(name, indexPath, analyzer)
//        {
//        }

//        public UmbracoExamineMultiIndexSearcher(string name, IEnumerable<Directory> luceneDirs, Analyzer analyzer)
//            : base(name, luceneDirs, analyzer)
//        {
//        }

//        public override void Initialize(string name, NameValueCollection config)
//        {
//            base.Initialize(name, config);

//            //need to check if the index set is specified, if it's not, we'll see if we can find one by convension
//            //if the folder is not null and the index set is null, we'll assume that this has been created at runtime.
//            if (config["indexSets"] == null)
//            {
//                throw new ArgumentNullException("indexSets on MultiIndexSearcher provider has not been set in configuration");
//            }

//            var toSearch = new List<IndexSet>();
//            var sets = IndexSets.Instance.Sets.Cast<IndexSet>();
//            foreach (var i in config["indexSets"].Split(','))
//            {
//                var s = sets.SingleOrDefault(x => x.SetName == i);
//                if (s == null)
//                {
//                    throw new ArgumentException("The index set " + i + " does not exist");
//                }
//                toSearch.Add(s);
//            }

//            //create the searchers
//            var analyzer = LuceneAnalyzer;
//            var searchers = new List<LuceneSearcher>();

//            var n = 0;
//            foreach (var s in toSearch)
//            {
//                searchers.Add(new LuceneSearcher(Name + "_" + n, s.IndexDirectory, analyzer));
//                n++;
//            }
//            Searchers = searchers;
//        }
//    }
//}