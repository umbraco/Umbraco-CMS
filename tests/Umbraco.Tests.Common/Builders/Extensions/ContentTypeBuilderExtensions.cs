// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders.Extensions;

public static class ContentTypeBuilderExtensions
{
    public static ContentType BuildSimpleContentType(this ContentTypeBuilder builder) =>
        (ContentType)builder
            .WithId(10)
            .WithAlias("textPage")
            .WithName("Text Page")
            .WithPropertyTypeIdsIncrementingFrom(200)
            .AddPropertyGroup()
            .WithName("Content")
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias("bodyText")
            .WithName("Body text")
            .WithSortOrder(2)
            .WithDataTypeId(Constants.DataTypes.RichtextEditor)
            .WithDataTypeKey(Constants.DataTypes.Guids.RichtextEditorGuid)
            .Done()
            .Done()
            .AddPropertyGroup()
            .WithName("Meta")
            .WithSortOrder(2)
            .AddPropertyType()
            .WithAlias("keywords")
            .WithName("Keywords")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("description")
            .WithName("description")
            .WithSortOrder(1)
            .Done()
            .Done()
            .AddAllowedTemplate()
            .WithId(200)
            .WithAlias("textPage")
            .WithName("Text Page")
            .Done()
            .AddAllowedTemplate()
            .WithId(201)
            .WithAlias("textPage2")
            .WithName("Text Page 2")
            .Done()
            .WithDefaultTemplateId(200)
            .AddAllowedContentType()
            .WithKey(new Guid("0793C350-04C4-475C-AC09-1D4B7CFD4DF4"))
            .WithAlias("sub")
            .WithSortOrder(8)
            .Done()
            .AddAllowedContentType()
            .WithKey(new Guid("9A485133-BCD8-4996-8D1C-64B5600B6A19"))
            .WithAlias("sub2")
            .WithSortOrder(9)
            .Done()
            .Build();

    public static MediaType BuildImageMediaType(this MediaTypeBuilder builder) =>
        (MediaType)builder
            .WithId(10)
            .WithAlias(Constants.Conventions.MediaTypes.Image)
            .WithName("Image")
            .WithDescription("test")
            .WithIcon("icon-picture")
            .WithPropertyTypeIdsIncrementingFrom(200)
            .WithMediaPropertyGroup()
            .Build();

    public static MemberType BuildSimpleMemberType(this MemberTypeBuilder builder) =>
        (MemberType)builder
            .WithId(10)
            .WithAlias("memberType")
            .WithName("Member type")
            .WithIcon("icon-user-female")
            .WithPropertyTypeIdsIncrementingFrom(200)
            .AddPropertyGroup()
            .WithName("Content")
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias("bodyText")
            .WithName("Body text")
            .WithSortOrder(2)
            .WithDataTypeId(Constants.DataTypes.RichtextEditor)
            .WithDataTypeKey(Constants.DataTypes.Guids.RichtextEditorGuid)
            .Done()
            .Done()
            .WithMemberCanEditProperty("title", true)
            .WithMemberCanViewProperty("bodyText", true)
            .Build();
}
