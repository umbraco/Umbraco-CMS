// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true,
    WithApplication = true)]
public class MemberTypeServiceTests : UmbracoIntegrationTest
{
    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    [Test]
    public void Member_Cannot_Edit_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        MemberTypeService.Save(memberType);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes)
        {
            Assert.IsFalse(memberType.MemberCanEditProperty(p.Alias));
        }
    }

    [Test]
    public void Member_Can_Edit_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        MemberTypeService.Save(memberType);
        var prop = memberType.PropertyTypes.First().Alias;
        memberType.SetMemberCanEditProperty(prop, true);
        MemberTypeService.Save(memberType);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes.Where(x => x.Alias != prop))
        {
            Assert.IsFalse(memberType.MemberCanEditProperty(p.Alias));
        }

        Assert.IsTrue(memberType.MemberCanEditProperty(prop));
    }

    [Test]
    public void Member_Cannot_View_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        MemberTypeService.Save(memberType);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes)
        {
            Assert.IsFalse(memberType.MemberCanViewProperty(p.Alias));
        }
    }

    [Test]
    public void Member_Can_View_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        MemberTypeService.Save(memberType);
        var prop = memberType.PropertyTypes.First().Alias;
        memberType.SetMemberCanViewProperty(prop, true);
        MemberTypeService.Save(memberType);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes.Where(x => x.Alias != prop))
        {
            Assert.IsFalse(memberType.MemberCanViewProperty(p.Alias));
        }

        Assert.IsTrue(memberType.MemberCanViewProperty(prop));
    }

    [Test]
    public void Deleting_PropertyType_Removes_The_Property_From_Member()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        MemberTypeService.Save(memberType);
        IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
        MemberService.Save(member);
        var initProps = member.Properties.Count;

        // remove a property (NOT ONE OF THE DEFAULTS)
        var standardProps = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);
        memberType.RemovePropertyType(memberType.PropertyTypes.First(x => standardProps.ContainsKey(x.Alias) == false)
            .Alias);
        MemberTypeService.Save(memberType);

        // re-load it from the db
        member = MemberService.GetById(member.Id);

        Assert.AreEqual(initProps - 1, member.Properties.Count);
    }

    [Test]
    public void Cannot_Save_MemberType_With_Empty_Name()
    {
        // Arrange
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("memberTypeAlias", string.Empty);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MemberTypeService.Save(memberType));
    }

    [Test]
    public void Empty_Description_Is_Always_Null_After_Saving_Member_Type()
    {
        var service = MemberTypeService;
        var memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.Description = null;
        service.Save(memberType);

        var memberType2 = MemberTypeBuilder.CreateSimpleMemberType("memberType2", "Member Type 2");
        memberType2.Description = string.Empty;
        service.Save(memberType2);

        Assert.IsNull(memberType.Description);
        Assert.IsNull(memberType2.Description);
    }

    // [Test]
    // public void Can_Save_MemberType_Structure_And_Create_A_Member_Based_On_It()
    // {
    //    // Arrange
    //    var cs = MemberService;
    //    var cts = MemberTypeService;
    //    var dtdYesNo = DataTypeService.GetDataTypeDefinitionById(-49);
    //    var ctBase = new MemberType(-1) { Name = "Base", Alias = "Base", Icon = "folder.gif", Thumbnail = "folder.png" };
    //    ctBase.AddPropertyType(new PropertyType(dtdYesNo)
    //        {
    //            Name = "Hide From Navigation",
    //            Alias = Constants.Conventions.Content.NaviHide
    //        }
    //        /*,"Navigation"*/);
    //    cts.Save(ctBase);

    // var ctHomePage = new MemberType(ctBase)
    //        {
    //            Name = "Home Page",
    //            Alias = "HomePage",
    //            Icon = "settingDomain.gif",
    //            Thumbnail = "folder.png",
    //            AllowedAsRoot = true
    //        };
    //    ctHomePage.AddPropertyType(new PropertyType(dtdYesNo) { Name = "Some property", Alias = "someProperty" }
    //        /*,"Navigation"*/);
    //    cts.Save(ctHomePage);

    // // Act
    //    var homeDoc = cs.CreateMember("Test", "test@test.com", "test", "HomePage");

    // // Assert
    //    Assert.That(ctBase.HasIdentity, Is.True);
    //    Assert.That(ctHomePage.HasIdentity, Is.True);
    //    Assert.That(homeDoc.HasIdentity, Is.True);
    //    Assert.That(homeDoc.ContentTypeId, Is.EqualTo(ctHomePage.Id));
    // }

    // [Test]
    // public void Can_Create_And_Save_MemberType_Composition()
    // {
    //    /*
    //     * Global
    //     * - Components
    //     * - Category
    //     */
    //    var service = ContentTypeService;
    //    var global = MemberTypeBuilder.CreateSimpleContentType("global", "Global");
    //    service.Save(global);

    // var components = MemberTypeBuilder.CreateSimpleContentType("components", "Components", global);
    //    service.Save(components);

    // var component = MemberTypeBuilder.CreateSimpleContentType("component", "Component", components);
    //    service.Save(component);

    // var category = MemberTypeBuilder.CreateSimpleContentType("category", "Category", global);
    //    service.Save(category);

    // var success = category.AddContentType(component);

    // Assert.That(success, Is.False);
    // }

    // [Test]
    // public void Can_Remove_ContentType_Composition_From_ContentType()
    // {
    //    //Test for U4-2234
    //    var cts = ContentTypeService;
    //    //Arrange
    //    var component = CreateComponent();
    //    cts.Save(component);
    //    var banner = CreateBannerComponent(component);
    //    cts.Save(banner);
    //    var site = CreateSite();
    //    cts.Save(site);
    //    var homepage = CreateHomepage(site);
    //    cts.Save(homepage);

    // //Add banner to homepage
    //    var added = homepage.AddContentType(banner);
    //    cts.Save(homepage);

    // //Assert composition
    //    var bannerExists = homepage.ContentTypeCompositionExists(banner.Alias);
    //    var bannerPropertyExists = homepage.CompositionPropertyTypes.Any(x => x.Alias.Equals("bannerName"));
    //    Assert.That(added, Is.True);
    //    Assert.That(bannerExists, Is.True);
    //    Assert.That(bannerPropertyExists, Is.True);
    //    Assert.That(homepage.CompositionPropertyTypes.Count(), Is.EqualTo(6));

    // //Remove banner from homepage
    //    var removed = homepage.RemoveContentType(banner.Alias);
    //    cts.Save(homepage);

    // //Assert composition
    //    var bannerStillExists = homepage.ContentTypeCompositionExists(banner.Alias);
    //    var bannerPropertyStillExists = homepage.CompositionPropertyTypes.Any(x => x.Alias.Equals("bannerName"));
    //    Assert.That(removed, Is.True);
    //    Assert.That(bannerStillExists, Is.False);
    //    Assert.That(bannerPropertyStillExists, Is.False);
    //    Assert.That(homepage.CompositionPropertyTypes.Count(), Is.EqualTo(4));
    // }

    // [Test]
    // public void Can_Copy_ContentType_By_Performing_Clone()
    // {
    //    // Arrange
    //    var service = ContentTypeService;
    //    var metaContentType = MemberTypeBuilder.CreateMetaContentType();
    //    service.Save(metaContentType);

    // var simpleContentType = MemberTypeBuilder.CreateSimpleContentType("category", "Category", metaContentType);
    //    service.Save(simpleContentType);
    //    var categoryId = simpleContentType.Id;

    // // Act
    //    var sut = simpleContentType.Clone("newcategory");
    //    service.Save(sut);

    // // Assert
    //    Assert.That(sut.HasIdentity, Is.True);

    // var contentType = service.GetContentType(sut.Id);
    //    var category = service.GetContentType(categoryId);

    // Assert.That(contentType.CompositionAliases().Any(x => x.Equals("meta")), Is.True);
    //    Assert.AreEqual(contentType.ParentId, category.ParentId);
    //    Assert.AreEqual(contentType.Level, category.Level);
    //    Assert.AreEqual(contentType.PropertyTypes.Count(), category.PropertyTypes.Count());
    //    Assert.AreNotEqual(contentType.Id, category.Id);
    //    Assert.AreNotEqual(contentType.Key, category.Key);
    //    Assert.AreNotEqual(contentType.Path, category.Path);
    //    Assert.AreNotEqual(contentType.SortOrder, category.SortOrder);
    //    Assert.AreNotEqual(contentType.PropertyTypes.First(x => x.Alias.Equals("title")).Id, category.PropertyTypes.First(x => x.Alias.Equals("title")).Id);
    //    Assert.AreNotEqual(contentType.PropertyGroups.First(x => x.Name.Equals("Content")).Id, category.PropertyGroups.First(x => x.Name.Equals("Content")).Id);

    // }

    // private ContentType CreateComponent()
    // {
    //    var component = new ContentType(-1)
    //        {
    //            Alias = "component",
    //            Name = "Component",
    //            Description = "ContentType used for Component grouping",
    //            Icon = ".sprTreeDoc3",
    //            Thumbnail = "doc.png",
    //            SortOrder = 1,
    //            CreatorId = 0,
    //            Trashed = false
    //        };

    // var contentCollection = new PropertyTypeCollection();
    //    contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "componentGroup", Name = "Component Group", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
    //    component.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Component", SortOrder = 1 });

    // return component;
    // }

    // private ContentType CreateBannerComponent(ContentType parent)
    // {
    //    var banner = new ContentType(parent)
    //        {
    //            Alias = "banner",
    //            Name = "Banner Component",
    //            Description = "ContentType used for Banner Component",
    //            Icon = ".sprTreeDoc3",
    //            Thumbnail = "doc.png",
    //            SortOrder = 1,
    //            CreatorId = 0,
    //            Trashed = false
    //        };

    // var propertyType = new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
    //        {
    //            Alias = "bannerName",
    //            Name = "Banner Name",
    //            Description = "",
    //            HelpText = "",
    //            Mandatory = false,
    //            SortOrder = 2,
    //            DataTypeDefinitionId = -88
    //        };
    //    banner.AddPropertyType(propertyType, "Component");
    //    return banner;
    // }

    // private ContentType CreateSite()
    // {
    //    var site = new ContentType(-1)
    //        {
    //            Alias = "site",
    //            Name = "Site",
    //            Description = "ContentType used for Site inheritence",
    //            Icon = ".sprTreeDoc3",
    //            Thumbnail = "doc.png",
    //            SortOrder = 2,
    //            CreatorId = 0,
    //            Trashed = false
    //        };

    // var contentCollection = new PropertyTypeCollection();
    //    contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "hostname", Name = "Hostname", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
    //    site.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Site Settings", SortOrder = 1 });

    // return site;
    // }

    // private ContentType CreateHomepage(ContentType parent)
    // {
    //    var contentType = new ContentType(parent)
    //        {
    //            Alias = "homepage",
    //            Name = "Homepage",
    //            Description = "ContentType used for the Homepage",
    //            Icon = ".sprTreeDoc3",
    //            Thumbnail = "doc.png",
    //            SortOrder = 1,
    //            CreatorId = 0,
    //            Trashed = false
    //        };

    // var contentCollection = new PropertyTypeCollection();
    //    contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
    //    contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
    //    contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

    // contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

    // return contentType;
    // }

    // private IEnumerable<IContentType> CreateContentTypeHierarchy()
    // {
    //    //create the master type
    //    var masterContentType = MemberTypeBuilder.CreateSimpleContentType("masterContentType", "MasterContentType");
    //    masterContentType.Key = new Guid("C00CA18E-5A9D-483B-A371-EECE0D89B4AE");
    //    ContentTypeService.Save(masterContentType);

    // //add the one we just created
    //    var list = new List<IContentType> { masterContentType };

    // for (var i = 0; i < 10; i++)
    //    {
    //        var contentType = MemberTypeBuilder.CreateSimpleContentType("childType" + i, "ChildType" + i,
    //                                                                     //make the last entry in the list, this one's parent
    //                                                                     list.Last());

    // list.Add(contentType);
    //    }

    // return list;
    // }
}
