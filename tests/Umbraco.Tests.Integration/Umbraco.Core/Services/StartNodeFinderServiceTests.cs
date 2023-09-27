// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.StartNodeFinder;
using Umbraco.Cms.Core.StartNodeFinder.Filters;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the StartNodeFinderService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class StartNodeFinderServiceTests : UmbracoIntegrationTestWithContent
{
    private StartNodeFinder StartNodeFinder => GetRequiredService<IStartNodeFinder>() as StartNodeFinder;
    private IDomainService DomainService => GetRequiredService<IDomainService>();

    [Test]
    public void GetDynamicStartNodes__With_NearestAncestorOrSelf_and_filter_of_own_doc_type_should_return_self()
    {
        // Arrange
        var startNodeSelector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Current.ToString(),
            OriginKey = null,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = Subpage2.Key,
                ParentKey = Textpage.Key
            },
            Filter = new StartNodeFilter[]
            {
                new StartNodeFilter()
                {
                    DirectionAlias = StartNodeSelectorDirection.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeAlias = new []{ContentType.Alias}
                }
            }
        };

        // Act
        var result = StartNodeFinder.GetDynamicStartNodes(startNodeSelector);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, startNodeSelector.Context.CurrentKey.Value);
        });
    }


    [Test]
    public void GetDynamicStartNodes__With_no_filters_shold_return_what_origin_finds()
    {
        // Arrange
        var startNodeSelector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Current.ToString(),
            OriginKey = null,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = Subpage2.Key,
                ParentKey = Textpage.Key
            },
            Filter = Array.Empty<StartNodeFilter>()
        };

        // Act
        var result = StartNodeFinder.GetDynamicStartNodes(startNodeSelector);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, startNodeSelector.Context.CurrentKey.Value);
        });
    }


    [Test]
    public void CalculateOriginKey__Current_should_just_return_the_origin_key()
    {
        // Arrange
        var selector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Current.ToString(),
            OriginKey = null,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = Subpage2.Key,
                ParentKey = Textpage.Key
            }
        };

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.AreEqual(selector.Context.CurrentKey, result);
    }

    [Test]
    public void CalculateOriginKey__Root_should_traverse_the_path_and_take_the_first_level_in_the_root()
    {
        // Arrange
        var selector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Root.ToString(),
            OriginKey = null,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = Subpage2.Key,
                ParentKey = Textpage.Key
            }
        };

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.AreEqual(Textpage.Key, result);
    }

    [Test]
    public void CalculateOriginKey__Site_should_return_the_first_with_an_assigned_domain_also_it_self()
    {
        // Arrange
        var origin = Subpage2;
        var selector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Site.ToString(),
            OriginKey =origin.Key,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = origin.Key,
                ParentKey = Textpage.Key
            }
        };

        DomainService.Save(new UmbracoDomain("http://test.umbraco.com") { RootContentId = origin.Id, LanguageIsoCode = "en-us"});

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.AreEqual(origin.Key, result);
    }

    [Test]
    public void CalculateOriginKey__Site_should_return_the_first_with_an_assigned_domain()
    {
        // Arrange
        var origin = Subpage2;
        var selector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Site.ToString(),
            OriginKey = origin.Key,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = origin.Key,
                ParentKey = Textpage.Key
            }
        };

        DomainService.Save(new UmbracoDomain("http://test.umbraco.com") { RootContentId = Textpage.Id, LanguageIsoCode = "en-us"});

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.AreEqual(Textpage.Key, result);
    }

    [Test]
    public void CalculateOriginKey__Site_should_fallback_to_root_when_no_domain_is_assigned()
    {
        // Arrange
        var selector = new StartNodeSelector()
        {
            OriginAlias = StartNodeSelectorOrigin.Site.ToString(),
            OriginKey = Subpage2.Key,
            Context = new StartNodeSelectorContext()
            {
                CurrentKey = Subpage2.Key,
                ParentKey = Textpage.Key
            }
        };

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.AreEqual(Textpage.Key, result);
    }

    [Test]
    [TestCase(StartNodeSelectorOrigin.ByKey)]
    [TestCase(StartNodeSelectorOrigin.Current)]
    [TestCase(StartNodeSelectorOrigin.Root)]
    [TestCase(StartNodeSelectorOrigin.Site)]
    public void CalculateOriginKey__with_a_random_key_should_return_null(StartNodeSelectorOrigin origin)
    {
        // Arrange
        var randomKey = Guid.NewGuid();
        var selector = new StartNodeSelector()
        {
            OriginAlias = origin.ToString(),
            OriginKey = randomKey,
            Context = new StartNodeSelectorContext() { CurrentKey = randomKey, ParentKey = Guid.NewGuid() }
        };

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    [TestCase(StartNodeSelectorOrigin.ByKey)]
    [TestCase(StartNodeSelectorOrigin.Current)]
    [TestCase(StartNodeSelectorOrigin.Root)]
    [TestCase(StartNodeSelectorOrigin.Site)]
    public void CalculateOriginKey__with_a_trashed_key_should_still_be_allowed(StartNodeSelectorOrigin origin)
    {
        // Arrange
        var trashedKey = Trashed.Key;
        var selector = new StartNodeSelector()
        {
            OriginAlias = origin.ToString(),
            OriginKey = trashedKey,
            Context = new StartNodeSelectorContext() { CurrentKey = trashedKey, ParentKey = Guid.NewGuid() }
        };

        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.IsNotNull(result);
    }

    [Test]
    [TestCase(StartNodeSelectorOrigin.ByKey)]
    [TestCase(StartNodeSelectorOrigin.Current)]
    [TestCase(StartNodeSelectorOrigin.Root)]
    [TestCase(StartNodeSelectorOrigin.Site)]
    public void CalculateOriginKey__with_a_ContentType_key_should_return_null(StartNodeSelectorOrigin origin)
    {
        // Arrange
        var contentTypeKey = ContentType.Key;
        var selector = new StartNodeSelector()
        {
            OriginAlias = origin.ToString(),
            OriginKey = contentTypeKey,
            Context = new StartNodeSelectorContext() { CurrentKey = contentTypeKey, ParentKey = Guid.NewGuid() }
        };
        // Act
        var result = StartNodeFinder.CalculateOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

}
