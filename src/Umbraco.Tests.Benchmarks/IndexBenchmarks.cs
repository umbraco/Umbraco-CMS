using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Examine;
using Umbraco.Tests.Testing;
using Umbraco.Tests.UmbracoExamine;
using Umbraco.Core;
using Moq;

namespace Umbraco.Tests.Benchmarks
{

    [MemoryDiagnoser]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class IndexBenchmarks : ExamineBaseTest
    {
        [IterationSetup]
        public void IterationSetup()
        {
            this._profilingLogger = new Mock<IProfilingLogger>().Object;
            TestOptionAttributeBase.ScanAssemblies.Add(typeof(IndexBenchmarks).Assembly);
            this.SetUp();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            this.Reset();
        }

        [Benchmark]
        public void Rebuild_Index()
        {
            Console.WriteLine("Rebuilding Index");
            var contentRebuilder = IndexInitializer.GetContentIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockContentService(), ScopeProvider, false);
            var mediaRebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(false)))
            using (indexer.ProcessNonAsync())
            {

                var searcher = indexer.GetSearcher();

                //create the whole thing
                contentRebuilder.Populate(indexer);
                mediaRebuilder.Populate(indexer);

                var result = searcher.CreateQuery().All().Execute();

                //Assert.AreEqual(29, result.TotalItemCount);
            }
        }
    }
}
