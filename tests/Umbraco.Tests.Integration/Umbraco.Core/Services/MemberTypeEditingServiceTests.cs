using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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

    [Test]
    public async Task Removing_Composition_Deletes_The_Composition_Property_Data_Of_The_Composing_Members()
    {
        var compositionProperty = MemberTypePropertyTypeModel("Composition text", "compositionText");
        var compositionCreateModel = MemberTypeCreateModel("Composition", "composition", propertyTypes: [compositionProperty]);
        var composition = (await MemberTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey)).Result!;
        var compositionPropertyTypeIds = composition.PropertyTypes.Select(x => x.Id).ToArray();
        Composition[] compositions = [new() { Key = composition.Key, CompositionType = CompositionType.Composition }];

        var composingProperty = MemberTypePropertyTypeModel("Composing text", "composingText");
        var composingCreateModel = MemberTypeCreateModel("Composing", "composing", propertyTypes: [composingProperty]);
        composingCreateModel.Compositions = compositions;
        var composingType = (await MemberTypeEditingService.CreateAsync(composingCreateModel, Constants.Security.SuperUserKey)).Result!;

        var otherComposingProperty = MemberTypePropertyTypeModel("Other composing text", "otherComposingText");
        var otherComposingCreateModel = MemberTypeCreateModel("Other composing", "otherComposing", propertyTypes: [otherComposingProperty]);
        otherComposingCreateModel.Compositions = compositions;
        var otherComposingType = (await MemberTypeEditingService.CreateAsync(otherComposingCreateModel, Constants.Security.SuperUserKey)).Result!;

        var memberEditingService = GetRequiredService<IMemberEditingService>();
        var superUser = (await UserService.GetAsync(Constants.Security.SuperUserKey))!;

        async Task<IMember> CreateMember(IMemberType memberType, string ownAlias, string name)
        {
            var username = name.Replace(" ", string.Empty);
            var createModel = new MemberCreateModel
            {
                ContentTypeKey = memberType.Key,
                Username = username,
                Email = $"{username}@example.com",
                Password = "SuperSecret123",
                IsApproved = true,
                Variants = [new VariantModel { Name = name }],
                Properties =
                [
                    new PropertyValueModel { Alias = "compositionText", Value = $"{name} compositionText" },
                    new PropertyValueModel { Alias = ownAlias, Value = $"{name} {ownAlias}" },
                ],
            };
            var result = await memberEditingService.CreateAsync(createModel, superUser);
            Assert.That(result.Success, Is.True, result.Status.ToString());
            return result.Result.Content!;
        }

        var composingMembers = new List<IMember>();
        for (var i = 0; i < 3; i++)
        {
            composingMembers.Add(await CreateMember(composingType, "composingText", $"Composing {i}"));
        }

        var otherComposingMembers = new List<IMember>();
        for (var i = 0; i < 2; i++)
        {
            otherComposingMembers.Add(await CreateMember(otherComposingType, "otherComposingText", $"Other composing {i}"));
        }

        List<PropertyDataDto> before = FetchAllPropertyData();

        var updateModel = MemberTypeUpdateModel("Composing", "composing", propertyTypes: [composingProperty]);
        var updateResult = await MemberTypeEditingService.UpdateAsync(composingType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);

        AssertOnlyPropertyDataDeleted(before, compositionPropertyTypeIds, composingMembers.Select(x => x.Id).ToArray());
        var memberService = GetRequiredService<IMemberService>();
        foreach (IMember member in composingMembers)
        {
            Assert.That(memberService.GetById(member.Id)!.Properties.Contains("compositionText"), Is.False);
        }

        foreach (IMember member in otherComposingMembers)
        {
            Assert.That(memberService.GetById(member.Id)!.GetValue<string>("compositionText"), Is.Not.Empty);
        }
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
