// using Umbraco.Cms.Core.Models;
// using Umbraco.Cms.Tests.Common.Builders;
// using Umbraco.Cms.Tests.Common.Builders.Extensions;
// using Umbraco.Cms.Tests.UnitTests.TestHelpers;
//
// namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;
//
// FIXME: Reintroduce if relevant
// public abstract class ContentFinderByIdentifierTestsBase : PublishedSnapshotServiceTestBase
// {
//     protected void PopulateCache(int nodeId, Guid nodeKey)
//     {
//         var dataTypes = GetDefaultDataTypes().Select(dt => dt as IDataType).ToArray();
//         var propertyDataTypes = new Dictionary<string, IDataType>
//         {
//             // we only have one data type for this test which will be resolved with string empty.
//             [string.Empty] = dataTypes[0],
//         };
//         IContentType contentType1 = new ContentType(ShortStringHelper, -1);
//
//         var rootData = new ContentDataBuilder()
//             .WithName("Page" + Guid.NewGuid())
//             .Build(ShortStringHelper, propertyDataTypes, contentType1, "alias");
//
//         var root = ContentNodeKitBuilder.CreateWithContent(
//             contentType1.Id,
//             9876,
//             "-1,9876",
//             draftData: rootData,
//             publishedData: rootData);
//
//         var parentData = new ContentDataBuilder()
//             .WithName("Page" + Guid.NewGuid())
//             .Build();
//
//         var parent = ContentNodeKitBuilder.CreateWithContent(
//             contentType1.Id,
//             5432,
//             "-1,9876,5432",
//             parentContentId: 9876,
//             draftData: parentData,
//             publishedData: parentData);
//
//         var contentData = new ContentDataBuilder()
//             .WithName("Page" + Guid.NewGuid())
//             .Build();
//
//         var content = ContentNodeKitBuilder.CreateWithContent(
//             contentType1.Id,
//             nodeId,
//             "-1,9876,5432," + nodeId,
//             parentContentId: 5432,
//             draftData: contentData,
//             publishedData: contentData,
//             uid: nodeKey);
//
//         InitializedCache(new[] { root, parent, content }, [contentType1], dataTypes);
//     }
// }
