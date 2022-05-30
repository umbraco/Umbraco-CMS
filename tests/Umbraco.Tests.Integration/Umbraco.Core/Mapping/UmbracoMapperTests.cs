// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Mapping;

[TestFixture]
[UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UmbracoMapperTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        _sut = Services.GetRequiredService<IUmbracoMapper>();

        _dataTypeService = Services.GetRequiredService<IDataTypeService>();
        _propertyEditorCollection = Services.GetRequiredService<PropertyEditorCollection>();
        _mediaBuilder = new MediaBuilder();
        _contentBuilder = new ContentBuilder();
        _contentTypeBuilder = new ContentTypeBuilder();
        _contentTypeService = Services.GetRequiredService<IContentTypeService>();
        _localizedTextService = Services.GetRequiredService<ILocalizedTextService>();
    }

    private IUmbracoMapper _sut;
    private IDataTypeService _dataTypeService;
    private PropertyEditorCollection _propertyEditorCollection;
    private MediaBuilder _mediaBuilder;
    private ContentBuilder _contentBuilder;
    private ContentTypeBuilder _contentTypeBuilder;
    private IContentTypeService _contentTypeService;
    private ILocalizedTextService _localizedTextService;

    [Test]
    public void To_Media_Item_Simple()
    {
        var content = _mediaBuilder
            .AddMediaType()
            .AddPropertyGroup()
            .AddPropertyType()
            .Done()
            .Done()
            .Done()
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        var result = _sut.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>(content);

        AssertBasics(result, content);

        foreach (var p in content.Properties)
        {
            AssertBasicProperty(result, p);
        }
    }

    [Test]
    public void To_Content_Item_Simple()
    {
        var content = _contentBuilder
            .AddContentType()
            .AddPropertyGroup()
            .AddPropertyType()
            .Done()
            .Done()
            .Done()
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        var result = _sut.Map<IContent, ContentItemBasic<ContentPropertyBasic>>(content);

        AssertBasics(result, content);

        foreach (var p in content.Properties)
        {
            AssertBasicProperty(result, p);
        }
    }

    [Test]
    public void To_Content_Item_Dto()
    {
        var content = _contentBuilder
            .AddContentType()
            .AddPropertyGroup()
            .AddPropertyType()
            .Done()
            .Done()
            .Done()
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        var result = _sut.Map<IContent, ContentPropertyCollectionDto>(content);

        foreach (var p in content.Properties)
        {
            AssertProperty(result, p);
        }
    }

    [Test]
    public void To_Media_Item_Dto()
    {
        var content = _mediaBuilder
            .AddMediaType()
            .AddPropertyGroup()
            .AddPropertyType()
            .Done()
            .Done()
            .Done()
            .Build();

        var result = _sut.Map<IMedia, ContentPropertyCollectionDto>(content);

        Assert.GreaterOrEqual(result.Properties.Count(), 1);
        Assert.AreEqual(content.Properties.Count, result.Properties.Count());
        foreach (var p in content.Properties)
        {
            AssertProperty(result, p);
        }
    }

    private void AssertDisplayProperty<T>(IContentProperties<T> result, IProperty p)
        where T : ContentPropertyBasic
    {
        var pDto = result.Properties.SingleOrDefault(x => x.Alias == p.Alias);
        Assert.IsNotNull(pDto);

        // pDto.Alias = p.Alias;
        // pDto.Description = p.PropertyType.Description;
        // pDto.Label = p.PropertyType.Name;
        // pDto.Config = applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(p.PropertyType.DataTypeDefinitionId);
    }

    [Test]
    public void To_Display_Model()
    {
        var contentType = _contentTypeBuilder
            .WithId(0)
            .AddPropertyGroup()
            .WithId(1)
            .AddPropertyType()
            .Done()
            .Done()
            .AddPropertyGroup()
            .WithId(2)
            .AddPropertyType()
            .Done()
            .Done()
            .AddPropertyGroup()
            .WithId(3)
            .AddPropertyType()
            .Done()
            .Done()
            .AddPropertyGroup()
            .WithId(4)
            .AddPropertyType()
            .Done()
            .Done()
            .Build();
        _contentTypeService.Save(contentType);

        var content = _contentBuilder
            .WithContentType(contentType)
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        var result = _sut.Map<IContent, ContentItemDisplay>(content);

        AssertBasics(result, content);

        var invariantContent = result.Variants.First();
        foreach (var p in content.Properties)
        {
            AssertBasicProperty(invariantContent, p);
            AssertDisplayProperty(invariantContent, p);
        }

        Assert.AreEqual(4, invariantContent.Tabs.Count());
        Assert.IsTrue(invariantContent.Tabs.First().IsActive);
        Assert.IsTrue(invariantContent.Tabs.Except(new[] { invariantContent.Tabs.First() })
            .All(x => x.IsActive == false));
    }

    [Test]
    public void To_Display_Model_No_Tabs()
    {
        var contentType = _contentTypeBuilder
            .WithId(0)
            .Build();
        _contentTypeService.Save(contentType);

        var content = _contentBuilder
            .WithContentType(contentType)
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        var result = _sut.Map<IContent, ContentItemDisplay>(content);

        AssertBasics(result, content);

        var invariantContent = result.Variants.First();
        foreach (var p in content.Properties)
        {
            AssertBasicProperty(invariantContent, p);
            AssertDisplayProperty(invariantContent, p);
        }

        Assert.AreEqual(contentType.CompositionPropertyGroups.Count(), invariantContent.Tabs.Count());
    }

    [Test]
    public void To_Display_Model_With_Non_Grouped_Properties()
    {
        var contentType = _contentTypeBuilder
            .WithId(0)
            .AddPropertyType()
            .WithId(1)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithPropertyEditorAlias("nonGrouped1")
            .WithName("Non Grouped 1")
            .Done()
            .AddPropertyType()
            .WithId(2)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithPropertyEditorAlias("nonGrouped2")
            .WithName("Non Grouped 2")
            .Done()
            .Build();
        _contentTypeService.Save(contentType);

        var content = _contentBuilder
            .WithContentType(contentType)
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        var result = _sut.Map<IContent, ContentItemDisplay>(content);

        AssertBasics(result, content);

        var invariantContent = result.Variants.First();
        foreach (var p in content.Properties)
        {
            AssertBasicProperty(invariantContent, p);
            AssertDisplayProperty(invariantContent, p);
        }

        Assert.AreEqual(contentType.CompositionPropertyGroups.Count(), invariantContent.Tabs.Count() - 1);
        Assert.IsTrue(
            invariantContent.Tabs.Any(x => x.Label == _localizedTextService.Localize("general", "properties")));
        Assert.AreEqual(
            2,
            invariantContent.Tabs.Where(x => x.Label == _localizedTextService.Localize("general", "properties"))
                .SelectMany(x => x.Properties.Where(p => p.Alias.StartsWith("_umb_") == false)).Count());
    }

    private void AssertBasics(ContentItemDisplay result, IContent content)
    {
        Assert.AreEqual(content.Id, result.Id);

        var ownerId = content.CreatorId;
        if (ownerId != 0)
        {
            Assert.IsNotNull(result.Owner);
            Assert.AreEqual(Constants.Security.SuperUserId, result.Owner.UserId);
            Assert.AreEqual("Administrator", result.Owner.Name);
        }
        else
        {
            Assert.IsNull(result.Owner); // because, 0 is no user
        }

        var invariantContent = result.Variants.First();

        Assert.AreEqual(content.ParentId, result.ParentId);
        Assert.AreEqual(content.UpdateDate, invariantContent.UpdateDate);
        Assert.AreEqual(content.CreateDate, invariantContent.CreateDate);
        Assert.AreEqual(content.Name, invariantContent.Name);
        Assert.AreEqual(
            content.Properties.Count(),
            ((IContentProperties<ContentPropertyDisplay>)invariantContent).Properties.Count(x =>
                x.Alias.StartsWith("_umb_") == false));
    }

    private void AssertBasics<T, TPersisted>(ContentItemBasic<T> result, TPersisted content)
        where T : ContentPropertyBasic
        where TPersisted : IContentBase
    {
        Assert.AreEqual(content.Id, result.Id);

        var ownerId = content.CreatorId;
        if (ownerId != 0)
        {
            Assert.IsNotNull(result.Owner);
            Assert.AreEqual(Constants.Security.SuperUserId, result.Owner.UserId);
            Assert.AreEqual("Administrator", result.Owner.Name);
        }
        else
        {
            Assert.IsNull(result.Owner); // because, 0 is no user
        }

        Assert.AreEqual(content.ParentId, result.ParentId);
        Assert.AreEqual(content.UpdateDate, result.UpdateDate);
        Assert.AreEqual(content.CreateDate, result.CreateDate);
        Assert.AreEqual(content.Name, result.Name);
        Assert.AreEqual(content.Properties.Count(), result.Properties.Count(x => x.Alias.StartsWith("_umb_") == false));
    }

    private void AssertBasicProperty<T>(IContentProperties<T> result, IProperty p)
        where T : ContentPropertyBasic
    {
        var pDto = result.Properties.SingleOrDefault(x => x.Alias == p.Alias);
        Assert.IsNotNull(pDto);
        Assert.AreEqual(p.Alias, pDto.Alias);
        Assert.AreEqual(p.Id, pDto.Id);

        if (p.GetValue() == null)
        {
            Assert.AreEqual(pDto.Value, string.Empty);
        }
        else if (p.GetValue() is decimal decimalValue)
        {
            Assert.AreEqual(pDto.Value, decimalValue.ToString(NumberFormatInfo.InvariantInfo));
        }
        else
        {
            Assert.AreEqual(pDto.Value, p.GetValue().ToString());
        }
    }

    private void AssertProperty(IContentProperties<ContentPropertyDto> result, IProperty p)
    {
        AssertBasicProperty(result, p);

        var pDto = result.Properties.SingleOrDefault(x => x.Alias == p.Alias);
        Assert.IsNotNull(pDto);
        Assert.AreEqual(p.PropertyType.Mandatory, pDto.IsRequired);
        Assert.AreEqual(p.PropertyType.ValidationRegExp, pDto.ValidationRegExp);
        Assert.AreEqual(p.PropertyType.Description, pDto.Description);
        Assert.AreEqual(p.PropertyType.Name, pDto.Label);
        Assert.AreEqual(_dataTypeService.GetDataType(p.PropertyType.DataTypeId), pDto.DataType);
        Assert.AreEqual(_propertyEditorCollection[p.PropertyType.PropertyEditorAlias], pDto.PropertyEditor);
    }

    private void AssertContentItem<T>(ContentItemBasic<ContentPropertyDto> result, T content)
        where T : IContentBase
    {
        AssertBasics(result, content);

        foreach (var p in content.Properties)
        {
            AssertProperty(result, p);
        }
    }
}
