using Umbraco.Cms.Core.Models;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public partial class InvariantContentTests : InvariantContentTestBase
{
    private void SetupDraftContent()
    {
        foreach (Guid key in new [] { RootKey, ChildKey, GrandchildKey, GreatGrandchildKey })
        {
            IContent content = ContentService.GetById(key)
                               ?? throw new InvalidOperationException($"Could not find content for key: {key}");
            content.Name += " (draft)";
            content.SetValue("title", content.GetValue<string>("title") + " (draft)");
            content.SetValue("count", content.GetValue<int>("count") + 1);
            content.SetValue("tags", content.GetValue<string>("tags")!.TrimEnd("]") + ",\"draft\"]");
            ContentService.Save(content);
        }

        IndexerAndSearcher.Reset();
    }

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string title, int count)
        => Assert.Multiple(() =>
        {
            var titleValue = document.Fields.FirstOrDefault(f => f.FieldName == "title")?.Value.Texts?.SingleOrDefault();
            Assert.That(titleValue, Is.EqualTo(title));

            var countValue = document.Fields.FirstOrDefault(f => f.FieldName == "count")?.Value.Integers?.SingleOrDefault();
            Assert.That(countValue, Is.EqualTo(count));
        });
}
