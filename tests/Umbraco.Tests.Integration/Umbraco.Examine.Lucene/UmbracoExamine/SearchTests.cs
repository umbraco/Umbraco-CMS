using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Search;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class SearchTests : ExamineBaseTest
{
    [Test]
    public void Test_Sort_Order_Sorting()
    {
        long totalRecs;
        var demoData = new ExamineDemoDataContentService(TestFiles.umbraco_sort);
        var allRecs = demoData.GetLatestContentByXPath("//*[@isDoc]")
            .Root
            .Elements()
            .Select(x => Mock.Of<IContent>(
                m =>
                    m.Id == (int)x.Attribute("id") &&
                    m.ParentId == (int)x.Attribute("parentID") &&
                    m.Level == (int)x.Attribute("level") &&
                    m.CreatorId == 0 &&
                    m.SortOrder == (int)x.Attribute("sortOrder") &&
                    m.CreateDate == (DateTime)x.Attribute("createDate") &&
                    m.UpdateDate == (DateTime)x.Attribute("updateDate") &&
                    m.Name == (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                    m.GetCultureName(It.IsAny<string>()) ==
                    (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                    m.Path == (string)x.Attribute("path") &&
                    m.Properties == new PropertyCollection() &&
                    m.Published == true &&
                    m.ContentType == Mock.Of<ISimpleContentType>(mt =>
                        mt.Icon == "test" &&
                        mt.Alias == x.Name.LocalName &&
                        mt.Id == (int)x.Attribute("nodeType"))))
            .ToArray();
        var contentService = Mock.Of<IContentService>(
            x => x.GetPagedDescendants(
                     It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering>())
                 ==
                 allRecs);

        using (GetSynchronousContentIndex(false, out var index, out var contentRebuilder, out _, null, contentService))
        {
            index.CreateIndex();
            contentRebuilder.Populate(index);

            var searcher = index.Searcher;

            Assert.Greater(searcher.CreateQuery().All().Execute().TotalItemCount, 0);

            var numberSortedCriteria = searcher.CreateQuery()
                .ParentId(1148)
                .OrderBy(new SortableField("sortOrder", SortType.Int));
            var numberSortedResult = numberSortedCriteria.Execute();

            var stringSortedCriteria = searcher.CreateQuery()
                .ParentId(1148)
                .OrderBy(new SortableField("sortOrder")); //will default to string
            var stringSortedResult = stringSortedCriteria.Execute();

            Assert.AreEqual(12, numberSortedResult.TotalItemCount);
            Assert.AreEqual(12, stringSortedResult.TotalItemCount);

            Assert.IsTrue(IsSortedByNumber(numberSortedResult));
            Assert.IsFalse(IsSortedByNumber(stringSortedResult));
        }
    }

    private bool IsSortedByNumber(IEnumerable<ISearchResult> results)
    {
        var currentSort = 0;
        foreach (var searchResult in results)
        {
            var sort = int.Parse(searchResult.Values["sortOrder"]);
            if (currentSort >= sort)
            {
                return false;
            }

            currentSort = sort;
        }

        return true;
    }
}
