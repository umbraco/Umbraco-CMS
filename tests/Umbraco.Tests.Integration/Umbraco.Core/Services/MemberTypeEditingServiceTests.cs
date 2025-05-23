using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Tests for the member type editing service. Please notice that a lot of functional test is covered by the content type
/// editing service tests, since these services share the same base implementation.
/// </summary>
internal sealed class MemberTypeEditingServiceTests : ContentTypeEditingServiceTestsBase
{
    private IMemberTypeEditingService MemberTypeEditingService => GetRequiredService<IMemberTypeEditingService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task Can_Create_Sensitive_Properties_With_Sensitive_Data_Access()
    {
        // arrange
        var createModel = MemberTypeCreateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(isSensitive: true) });

        // act
        var result = await MemberTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // assert
        var memberType = await MemberTypeService.GetAsync(result.Result.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsNotEmpty(memberType.PropertyTypes);
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.IsSensitiveProperty(propertyType.Alias)));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Change_Property_Sensitivity_With_Sensitive_Data_Access(bool initialIsSensitiveValue)
    {
        // arrange
        var createModel = MemberTypeCreateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(isSensitive: initialIsSensitiveValue) });
        IMemberType memberType = (await MemberTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var newIsSensitiveValue = initialIsSensitiveValue is false;
        var updateModel = MemberTypeUpdateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(isSensitive: newIsSensitiveValue) });

        // act
        var result = await MemberTypeEditingService.UpdateAsync(memberType, updateModel, Constants.Security.SuperUserKey);

        // assert
        memberType = await MemberTypeService.GetAsync(memberType.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.Success, result.Status);
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.IsSensitiveProperty(propertyType.Alias) == newIsSensitiveValue));
        });
    }

    [Test]
    public async Task Cannot_Create_Sensitive_Properties_Without_Sensitive_Data_Access()
    {
        // arrange
        // this user does NOT have access to sensitive data
        var user = UserBuilder.CreateUser();
        UserService.Save(user);
        var createModel = MemberTypeCreateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(isSensitive: true) });

        // act
        var result = await MemberTypeEditingService.CreateAsync(createModel, user.Key);

        // assert
        var memberType = await MemberTypeService.GetAsync(result.Result.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.IsNull(memberType);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Change_Property_Sensitivity_Without_Sensitive_Data_Access(bool initialIsSensitiveValue)
    {
        // arrange
        // this user does NOT have access to sensitive data
        var user = UserBuilder.CreateUser();
        UserService.Save(user);

        var createModel = MemberTypeCreateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(isSensitive: initialIsSensitiveValue) });
        IMemberType memberType = (await MemberTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var newIsSensitiveValue = initialIsSensitiveValue is false;
        var updateModel = MemberTypeUpdateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(isSensitive: newIsSensitiveValue) });

        // act
        var result = await MemberTypeEditingService.UpdateAsync(memberType, updateModel, user.Key);

        // assert
        memberType = await MemberTypeService.GetAsync(memberType.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.NotAllowed, result.Status);
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.IsSensitiveProperty(propertyType.Alias) == initialIsSensitiveValue));
        });

    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public async Task Can_Define_Property_Visibility_When_Creating(bool memberCanView, bool memberCanEdit)
    {
        // arrange
        var createModel = MemberTypeCreateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(memberCanView: memberCanView, memberCanEdit: memberCanEdit) });

        // act
        var result = await MemberTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // assert
        var memberType = await MemberTypeService.GetAsync(result.Result.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsNotEmpty(memberType.PropertyTypes);
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.MemberCanViewProperty(propertyType.Alias) == memberCanView));
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.MemberCanEditProperty(propertyType.Alias) == memberCanEdit));
        });
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public async Task Can_Update_Property_Visibility(bool memberCanView, bool memberCanEdit)
    {
        // arrange
        var createModel = MemberTypeCreateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(memberCanView: memberCanView is false, memberCanEdit: memberCanEdit is false) });
        IMemberType memberType = (await MemberTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var updateModel = MemberTypeUpdateModel(propertyTypes: new[] { MemberTypePropertyTypeModel(memberCanView: memberCanView, memberCanEdit: memberCanEdit) });

        // act
        var result = await MemberTypeEditingService.UpdateAsync(memberType, updateModel, Constants.Security.SuperUserKey);

        // assert
        memberType = await MemberTypeService.GetAsync(result.Result.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(memberType.PropertyTypes);
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.MemberCanViewProperty(propertyType.Alias) == memberCanView));
            Assert.IsTrue(memberType.PropertyTypes.All(propertyType => memberType.MemberCanEditProperty(propertyType.Alias) == memberCanEdit));
        });
    }

    private MemberTypeCreateModel MemberTypeCreateModel(
        string name = "Test",
        string? alias = null,
        Guid? key = null,
        IEnumerable<MemberTypePropertyTypeModel>? propertyTypes = null)
    {
        var model = CreateContentEditingModel<MemberTypeCreateModel, MemberTypePropertyTypeModel, MemberTypePropertyContainerModel>(
            name,
            alias,
            isElement: false,
            propertyTypes);
        model.Key = key ?? Guid.NewGuid();
        model.Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name);
        return model;
    }

    private MemberTypeUpdateModel MemberTypeUpdateModel(
        string name = "Test",
        string? alias = null,
        IEnumerable<MemberTypePropertyTypeModel>? propertyTypes = null)
        => CreateContentEditingModel<MemberTypeUpdateModel, MemberTypePropertyTypeModel, MemberTypePropertyContainerModel>(
            name,
            alias,
            isElement: false,
            propertyTypes);

    private MemberTypePropertyTypeModel MemberTypePropertyTypeModel(
        string name = "Title",
        string? alias = null,
        Guid? key = null,
        Guid? dataTypeKey = null,
        bool isSensitive = false,
        bool memberCanView = false,
        bool memberCanEdit = false)
    {
        var propertyType = CreatePropertyType<MemberTypePropertyTypeModel>(name, alias, key, containerKey: null, dataTypeKey);
        propertyType.IsSensitive = isSensitive;
        propertyType.MemberCanView = memberCanView;
        propertyType.MemberCanEdit = memberCanEdit;
        return propertyType;
    }
}
