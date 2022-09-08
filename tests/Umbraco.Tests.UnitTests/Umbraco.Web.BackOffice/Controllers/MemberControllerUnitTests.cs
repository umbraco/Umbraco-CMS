using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Common;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Mapping;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Security;
using MemberMapDefinition = Umbraco.Cms.Web.BackOffice.Mapping.MemberMapDefinition;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class MemberControllerUnitTests
{
    private IUmbracoMapper _mapper;

    [Test]
    [AutoMoqData]
    public void PostSaveMember_WhenMemberIsNull_ExpectFailureResponse(
        MemberController sut)
    {
        // arrange
        // act
        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => sut.PostSave(null));

        // assert
        Assert.That(
            exception.Message,
            Is.EqualTo("Value cannot be null. (Parameter 'The member content item was null')"));
    }

    [Test]
    [AutoMoqData]
    public void PostSaveMember_WhenModelStateIsNotValid_ExpectFailureResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        SetupMemberTestData(out var fakeMemberData, out _, ContentSaveAction.SaveNew);
        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);
        sut.ModelState.AddModelError("key", "Invalid model state");

        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);

        // act
        var result = sut.PostSave(fakeMemberData).Result;
        var validation = result.Result as ValidationErrorResult;

        // assert
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Value);
        Assert.AreEqual(StatusCodes.Status400BadRequest, validation?.StatusCode);
    }

    [Test]
    [AutoMoqData]
    public async Task PostSaveMember_SaveNew_NoCustomField_WhenAllIsSetupCorrectly_ExpectSuccessResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeSecurity backOfficeSecurity,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        var member = SetupMemberTestData(out var fakeMemberData, out var memberDisplay, ContentSaveAction.SaveNew);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.GetRolesAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => Array.Empty<string>());
        Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
        Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
        Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
            .Returns(() => null)
            .Returns(() => member);
        Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);

        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);

        // act
        var result = await sut.PostSave(fakeMemberData);

        // assert
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Value);
        AssertMemberDisplayPropertiesAreEqual(memberDisplay, result.Value);
    }

    [Test]
    [AutoMoqData]
    public async Task PostSaveMember_SaveNew_CustomField_WhenAllIsSetupCorrectly_ExpectSuccessResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeSecurity backOfficeSecurity,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        var member = SetupMemberTestData(out var fakeMemberData, out var memberDisplay, ContentSaveAction.SaveNew);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.GetRolesAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => Array.Empty<string>());
        Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
        Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
        Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
            .Returns(() => null)
            .Returns(() => member);
        Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);

        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);

        // act
        var result = await sut.PostSave(fakeMemberData);

        // assert
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Value);
        AssertMemberDisplayPropertiesAreEqual(memberDisplay, result.Value);
    }

    [Test]
    [AutoMoqData]
    public async Task PostSaveMember_SaveExisting_WhenAllIsSetupCorrectly_ExpectSuccessResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeSecurity backOfficeSecurity,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        var member = SetupMemberTestData(out var fakeMemberData, out var memberDisplay, ContentSaveAction.Save);
        var membersIdentityUser = new MemberIdentityUser(123);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => membersIdentityUser);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);

        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.UpdateAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.GetRolesAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => Array.Empty<string>());
        Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
        Mock.Get(globalSettings);

        SetupUserAccess(backOfficeSecurityAccessor, backOfficeSecurity, user);
        SetupPasswordSuccess(umbracoMembersUserManager, passwordChanger);

        Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);
        Mock.Get(memberService).Setup(x => x.GetById(It.IsAny<int>())).Returns(() => member);
        Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
            .Returns(() => null)
            .Returns(() => member);

        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);

        // act
        var result = await sut.PostSave(fakeMemberData);

        // assert
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Value);
        AssertMemberDisplayPropertiesAreEqual(memberDisplay, result.Value);
    }

    [Test]
    [AutoMoqData]
    public async Task PostSaveMember_SaveExisting_WhenAllIsSetupWithPasswordIncorrectly_ExpectFailureResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeSecurity backOfficeSecurity,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        var member = SetupMemberTestData(out var fakeMemberData, out _, ContentSaveAction.Save);
        var membersIdentityUser = new MemberIdentityUser(123);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => membersIdentityUser);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);

        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.UpdateAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
        Mock.Get(globalSettings);

        SetupUserAccess(backOfficeSecurityAccessor, backOfficeSecurity, user);
        SetupPasswordSuccess(umbracoMembersUserManager, passwordChanger, false);

        Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);
        Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
            .Returns(() => null)
            .Returns(() => member);

        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);

        // act
        var result = await sut.PostSave(fakeMemberData);

        // assert
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Value);
    }

    private static void SetupUserAccess(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IBackOfficeSecurity backOfficeSecurity, IUser user)
    {
        Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
        Mock.Get(user).Setup(x => x.AllowedSections).Returns(new[] { "member" });
        Mock.Get(backOfficeSecurity).Setup(x => x.CurrentUser).Returns(user);
    }

    private static void SetupPasswordSuccess(
        IMemberManager umbracoMembersUserManager,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        bool successful = true)
    {
        var passwordChanged = new PasswordChangedModel { ChangeError = null, ResetPassword = null };
        if (!successful)
        {
            var attempt = Attempt.Fail(passwordChanged);
            Mock.Get(passwordChanger)
                .Setup(x => x.ChangePasswordWithIdentityAsync(
                    It.IsAny<ChangingPasswordModel>(),
                    umbracoMembersUserManager))
                .ReturnsAsync(() => attempt);
        }
        else
        {
            var attempt = Attempt.Succeed(passwordChanged);
            Mock.Get(passwordChanger)
                .Setup(x => x.ChangePasswordWithIdentityAsync(
                    It.IsAny<ChangingPasswordModel>(),
                    umbracoMembersUserManager))
                .ReturnsAsync(() => attempt);
        }
    }

    [Test]
    [AutoMoqData]
    public void PostSaveMember_SaveNew_WhenMemberEmailAlreadyExists_ExpectFailResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeSecurity backOfficeSecurity,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        var member = SetupMemberTestData(out var fakeMemberData, out _, ContentSaveAction.SaveNew);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
        Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.AddToRolesAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(() => IdentityResult.Success);

        Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
            .Returns(() => member);

        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);

        // act
        var result = sut.PostSave(fakeMemberData).Result;
        var validation = result.Result as ValidationErrorResult;

        // assert
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Value);
        Assert.AreEqual(StatusCodes.Status400BadRequest, validation?.StatusCode);
    }

    [Test]
    [AutoMoqData]
    public async Task PostSaveMember_SaveExistingMember_WithNoRoles_Add1Role_ExpectSuccessResponse(
        [Frozen] IMemberManager umbracoMembersUserManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeSecurity backOfficeSecurity,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings,
        IUser user)
    {
        // arrange
        var roleName = "anyrole";
        IMember member = SetupMemberTestData(out var fakeMemberData, out var memberDisplay, ContentSaveAction.Save);
        fakeMemberData.Groups = new List<string> { roleName };
        var membersIdentityUser = new MemberIdentityUser(123);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => membersIdentityUser);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.UpdateAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.AddToRolesAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(() => IdentityResult.Success);
        Mock.Get(umbracoMembersUserManager)
            .Setup(x => x.GetRolesAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(() => Array.Empty<string>());

        Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
        Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
        Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);
        Mock.Get(memberService).Setup(x => x.GetById(It.IsAny<int>())).Returns(() => member);

        SetupUserAccess(backOfficeSecurityAccessor, backOfficeSecurity, user);
        SetupPasswordSuccess(umbracoMembersUserManager, passwordChanger);

        Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
            .Returns(() => null)
            .Returns(() => member);
        var sut = CreateSut(
            memberService,
            memberTypeService,
            memberGroupService,
            umbracoMembersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            passwordChanger,
            globalSettings);

        // act
        var result = await sut.PostSave(fakeMemberData);

        // assert
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Value);
        Mock.Get(umbracoMembersUserManager)
            .Verify(u => u.GetRolesAsync(membersIdentityUser));
        Mock.Get(umbracoMembersUserManager)
            .Verify(u => u.AddToRolesAsync(membersIdentityUser, new[] { roleName }));
        Mock.Get(umbracoMembersUserManager)
            .Verify(x => x.GetRolesAsync(It.IsAny<MemberIdentityUser>()));
        Mock.Get(memberService)
            .Verify(m => m.Save(It.IsAny<Member>()));
        AssertMemberDisplayPropertiesAreEqual(memberDisplay, result.Value);
    }

    /// <summary>
    ///     Create member controller to test
    /// </summary>
    /// <param name="memberService">Member service</param>
    /// <param name="memberTypeService">Member type service</param>
    /// <param name="memberGroupService">Member group service</param>
    /// <param name="membersUserManager">Members user manager</param>
    /// <param name="dataTypeService">Data type service</param>
    /// <param name="backOfficeSecurityAccessor">Back office security accessor</param>
    /// <param name="passwordChanger">Password changer class</param>
    /// <param name="globalSettings">The global settings</param>
    /// <returns>A member controller for the tests</returns>
    private MemberController CreateSut(
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        IMemberGroupService memberGroupService,
        IUmbracoUserManager<MemberIdentityUser> membersUserManager,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IPasswordChanger<MemberIdentityUser> passwordChanger,
        IOptions<GlobalSettings> globalSettings)
    {
        var httpContextAccessor = new HttpContextAccessor();

        var mockShortStringHelper = new MockShortStringHelper();
        var textService = new Mock<ILocalizedTextService>();
        var contentTypeBaseServiceProvider = new Mock<IContentTypeBaseServiceProvider>();
        contentTypeBaseServiceProvider.Setup(x => x.GetContentTypeOf(It.IsAny<IContentBase>()))
            .Returns(new ContentType(mockShortStringHelper, 123));
        var contentAppFactories = new Mock<List<IContentAppFactory>>();
        var mockContentAppFactoryCollection = new Mock<ILogger<ContentAppFactoryCollection>>();
        var hybridBackOfficeSecurityAccessor = new BackOfficeSecurityAccessor(httpContextAccessor);
        var contentAppFactoryCollection = new ContentAppFactoryCollection(
            () => contentAppFactories.Object,
            mockContentAppFactoryCollection.Object,
            hybridBackOfficeSecurityAccessor);
        var mockUserService = new Mock<IUserService>();
        var commonMapper = new CommonMapper(
            mockUserService.Object,
            contentTypeBaseServiceProvider.Object,
            contentAppFactoryCollection,
            textService.Object);
        var mockCultureDictionary = new Mock<ICultureDictionary>();

        var mockPasswordConfig = new Mock<IOptions<MemberPasswordConfigurationSettings>>();
        mockPasswordConfig.Setup(x => x.Value).Returns(() => new MemberPasswordConfigurationSettings());
        var dataEditor = Mock.Of<IDataEditor>(
            x => x.Type == EditorType.PropertyValue
                 && x.Alias == Constants.PropertyEditors.Aliases.Label);
        Mock.Get(dataEditor).Setup(x => x.GetValueEditor()).Returns(new TextOnlyValueEditor(
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox, "Test Textbox", "textbox"),
            textService.Object,
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>()));

        var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(() => new[] { dataEditor }));

        IMapDefinition memberMapDefinition = new MemberMapDefinition(
            commonMapper,
            new CommonTreeNodeMapper(Mock.Of<LinkGenerator>()),
            new MemberTabsAndPropertiesMapper(
                mockCultureDictionary.Object,
                backOfficeSecurityAccessor,
                textService.Object,
                memberTypeService,
                memberService,
                memberGroupService,
                mockPasswordConfig.Object,
                contentTypeBaseServiceProvider.Object,
                propertyEditorCollection));

        var map = new MapDefinitionCollection(() => new List<IMapDefinition>
        {
            new global::Umbraco.Cms.Core.Models.Mapping.MemberMapDefinition(),
            memberMapDefinition,
            new ContentTypeMapDefinition(
                commonMapper,
                propertyEditorCollection,
                dataTypeService,
                new Mock<IFileService>().Object,
                new Mock<IContentTypeService>().Object,
                new Mock<IMediaTypeService>().Object,
                memberTypeService,
                new Mock<ILoggerFactory>().Object,
                mockShortStringHelper,
                globalSettings,
                new Mock<IHostingEnvironment>().Object,
                new Mock<IOptionsMonitor<ContentSettings>>().Object),
        });
        var scopeProvider = Mock.Of<ICoreScopeProvider>(x => x.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher>(),
            It.IsAny<IScopedNotificationPublisher>(),
            It.IsAny<bool?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()) == Mock.Of<ICoreScope>());

        _mapper = new UmbracoMapper(map, scopeProvider);

        return new MemberController(
            new DefaultCultureDictionary(
                new Mock<ILocalizationService>().Object,
                NoAppCache.Instance),
            new LoggerFactory(),
            mockShortStringHelper,
            new DefaultEventMessagesFactory(
                new Mock<IEventMessagesAccessor>().Object),
            textService.Object,
            propertyEditorCollection,
            _mapper,
            memberService,
            memberTypeService,
            (IMemberManager)membersUserManager,
            dataTypeService,
            backOfficeSecurityAccessor,
            new ConfigurationEditorJsonSerializer(),
            passwordChanger,
            scopeProvider);
    }

    /// <summary>
    ///     Setup all standard member data for test
    /// </summary>
    private Member SetupMemberTestData(
        out MemberSave fakeMemberData,
        out MemberDisplay memberDisplay,
        ContentSaveAction contentAction)
    {
        // arrange
        var memberType = MemberTypeBuilder.CreateSimpleMemberType();
        var member = MemberBuilder.CreateSimpleMember(memberType, "Test Member", "test@example.com", "123", "test");
        var memberId = 123;
        member.Id = memberId;

        // TODO: replace with builder for MemberSave and MemberDisplay
        fakeMemberData = new MemberSave
        {
            Id = memberId,
            SortOrder = member.SortOrder,
            ContentTypeId = memberType.Id,
            Key = member.Key,
            Password = new ChangingPasswordModel { Id = 456, NewPassword = member.RawPasswordValue, OldPassword = null },
            Name = member.Name,
            Email = member.Email,
            Username = member.Username,
            PersistedContent = member,
            PropertyCollectionDto = new ContentPropertyCollectionDto(),
            Groups = new List<string>(),

            // Alias = "fakeAlias",
            ContentTypeAlias = member.ContentTypeAlias,
            Action = contentAction,
            Icon = "icon-document",
            Path = member.Path,
        };

        memberDisplay = new MemberDisplay
        {
            Id = memberId,
            SortOrder = member.SortOrder,
            ContentTypeId = memberType.Id,
            Key = member.Key,
            Name = member.Name,
            Email = member.Email,
            Username = member.Username,

            // Alias = "fakeAlias",
            ContentTypeAlias = member.ContentTypeAlias,
            ContentType = new ContentTypeBasic(),
            ContentTypeName = member.ContentType.Name,
            Icon = fakeMemberData.Icon,
            Path = member.Path,
            Tabs = new List<Tab<ContentPropertyDisplay>>
            {
                new()
                {
                    Alias = "test",
                    Id = 77,
                    Properties = new List<ContentPropertyDisplay>
                    {
                        new() { Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login" },
                        new() { Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}email" },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}password",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}membergroup",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}failedPasswordAttempts",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}approved",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lockedOut",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lastLockoutDate",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lastLoginDate",
                        },
                        new()
                        {
                            Alias =
                                $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lastPasswordChangeDate",
                        },
                    },
                },
            },
        };

        return member;
    }

    /// <summary>
    ///     Check all member properties are equal
    /// </summary>
    /// <param name="memberDisplay"></param>
    /// <param name="resultValue"></param>
    private void AssertMemberDisplayPropertiesAreEqual(MemberDisplay memberDisplay, MemberDisplay resultValue)
    {
        Assert.AreNotSame(memberDisplay, resultValue);
        Assert.AreEqual(memberDisplay.Id, resultValue.Id);
        Assert.AreEqual(memberDisplay.Alias, resultValue.Alias);
        Assert.AreEqual(memberDisplay.Username, resultValue.Username);
        Assert.AreEqual(memberDisplay.Email, resultValue.Email);
        Assert.AreEqual(memberDisplay.AdditionalData, resultValue.AdditionalData);
        Assert.AreEqual(memberDisplay.ContentApps, resultValue.ContentApps);
        Assert.AreEqual(memberDisplay.ContentType.Alias, resultValue.ContentType.Alias);
        Assert.AreEqual(memberDisplay.ContentTypeAlias, resultValue.ContentTypeAlias);
        Assert.AreEqual(memberDisplay.ContentTypeName, resultValue.ContentTypeName);
        Assert.AreEqual(memberDisplay.ContentTypeId, resultValue.ContentTypeId);
        Assert.AreEqual(memberDisplay.Icon, resultValue.Icon);
        Assert.AreEqual(memberDisplay.Errors, resultValue.Errors);
        Assert.AreEqual(memberDisplay.Key, resultValue.Key);
        Assert.AreEqual(memberDisplay.Name, resultValue.Name);
        Assert.AreEqual(memberDisplay.Path, resultValue.Path);
        Assert.AreEqual(memberDisplay.SortOrder, resultValue.SortOrder);
        Assert.AreEqual(memberDisplay.Trashed, resultValue.Trashed);
        Assert.AreEqual(memberDisplay.TreeNodeUrl, resultValue.TreeNodeUrl);

        // TODO: can we check create/update dates when saving?
        // Assert.AreEqual(memberDisplay.CreateDate, resultValue.CreateDate);
        // Assert.AreEqual(memberDisplay.UpdateDate, resultValue.UpdateDate);

        // TODO: check all properties
        Assert.AreEqual(memberDisplay.Properties.Count(), resultValue.Properties.Count());
        Assert.AreNotSame(memberDisplay.Properties, resultValue.Properties);
        for (var index = 0; index < resultValue.Properties.Count(); index++)
        {
            Assert.AreNotSame(
                memberDisplay.Properties.GetItemByIndex(index),
                resultValue.Properties.GetItemByIndex(index));

            // Assert.AreEqual(memberDisplay.Properties.GetItemByIndex(index), resultValue.Properties.GetItemByIndex(index));
        }
    }
}
