using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Common;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Infrastructure.Security;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.UnitTests.AutoFixture;
using Umbraco.Tests.UnitTests.Umbraco.Core.ShortStringHelper;
using Umbraco.Web;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Mapping;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.PropertyEditors;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class MemberControllerUnitTests
    {
        private UmbracoMapper _mapper;

        [Test]
        [AutoMoqData]
        public void PostSaveMember_WhenMemberIsNull_ExpectFailureResponse(
            MemberController sut)
        {
            // arrange
            // act
            ArgumentNullException exception = Assert.ThrowsAsync<ArgumentNullException>(() => sut.PostSave(null));

            // assert
            Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'The member content item was null')"));
        }

        [Test]
        [AutoMoqData]
        public void PostSaveMember_WhenModelStateIsNotValid_ExpectFailureResponse(
            [Frozen] IMemberManager umbracoMembersUserManager,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IMemberGroupService memberGroupService,
            IDataTypeService dataTypeService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            // arrange
            Member member = SetupMemberTestData(out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            MemberController sut = CreateSut(memberService, memberTypeService, memberGroupService, umbracoMembersUserManager, dataTypeService, backOfficeSecurityAccessor);
            sut.ModelState.AddModelError("key", "Invalid model state");

            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);

            var value = new MemberDisplay();
            string reason = "Validation failed";

            // act
            ActionResult<MemberDisplay> result = sut.PostSave(fakeMemberData).Result;
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
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => null)
                .Returns(() => member);
            Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);

            MemberController sut = CreateSut(memberService, memberTypeService, memberGroupService, umbracoMembersUserManager, dataTypeService, backOfficeSecurityAccessor);

            // act
            ActionResult<MemberDisplay> result = await sut.PostSave(fakeMemberData);

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
        IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => null)
                .Returns(() => member);
            Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);

            MemberController sut = CreateSut(memberService, memberTypeService, memberGroupService, umbracoMembersUserManager, dataTypeService, backOfficeSecurityAccessor);

            // act
            ActionResult<MemberDisplay> result = await sut.PostSave(fakeMemberData);

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
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.Save);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new MembersIdentityUser());
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);

            string password = "fakepassword9aw89rnyco3938cyr^%&*()i8Y";
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns(password);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.UpdateAsync(It.IsAny<MembersIdentityUser>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);
            Mock.Get(memberService).SetupSequence(
                    x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => null)
                .Returns(() => member);

            MemberController sut = CreateSut(memberService, memberTypeService, memberGroupService, umbracoMembersUserManager, dataTypeService, backOfficeSecurityAccessor);

            // act
            ActionResult<MemberDisplay> result = await sut.PostSave(fakeMemberData);

            // assert
            Assert.IsNull(result.Result);
            Assert.IsNotNull(result.Value);
            AssertMemberDisplayPropertiesAreEqual(memberDisplay, result.Value);
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
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);

            Mock.Get(memberService).SetupSequence(
                    x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => member);

            MemberController sut = CreateSut(memberService, memberTypeService, memberGroupService, umbracoMembersUserManager, dataTypeService, backOfficeSecurityAccessor);
            string reason = "Validation failed";

            // act
            ActionResult<MemberDisplay> result = sut.PostSave(fakeMemberData).Result;
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
           IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            string password = "fakepassword9aw89rnyco3938cyr^%&*()i8Y";
            var roleName = "anyrole";
            IMember member = SetupMemberTestData(out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.Save);
            fakeMemberData.Groups = new List<string>()
            {
                roleName
            };
            var membersIdentityUser = new MembersIdentityUser();
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => membersIdentityUser);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns(password);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.UpdateAsync(It.IsAny<MembersIdentityUser>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);

            Mock.Get(memberService).SetupSequence(
                    x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => null)
                .Returns(() => member);
            Mock.Get(memberService).Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(() => member);
            MemberController sut = CreateSut(memberService, memberTypeService, memberGroupService, umbracoMembersUserManager, dataTypeService, backOfficeSecurityAccessor);

            // act
            ActionResult<MemberDisplay> result = await sut.PostSave(fakeMemberData);

            // assert
            Assert.IsNull(result.Result);
            Assert.IsNotNull(result.Value);
            Mock.Get(umbracoMembersUserManager)
                .Verify(u => u.GetRolesAsync(membersIdentityUser));
             Mock.Get(umbracoMembersUserManager)
                .Verify(u => u.AddToRolesAsync(membersIdentityUser, new[] { roleName }));
            Mock.Get(memberService)
                .Verify(m => m.Save(It.IsAny<Member>(), true));
            AssertMemberDisplayPropertiesAreEqual(memberDisplay, result.Value);
        }

        /// <summary>
        /// Create member controller to test
        /// </summary>
        /// <param name="memberService">Member service</param>
        /// <param name="memberTypeService">Member type service</param>
        /// <param name="memberGroupService">Member group service</param>
        /// <param name="membersUserManager">Members user manager</param>
        /// <param name="dataTypeService">Data type service</param>
        /// <param name="backOfficeSecurityAccessor">Back office security accessor</param>
        /// <returns>A member controller for the tests</returns>
        private MemberController CreateSut(
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IMemberGroupService memberGroupService,
            IMemberManager membersUserManager,
            IDataTypeService dataTypeService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            var mockShortStringHelper = new MockShortStringHelper();

            var textService = new Mock<ILocalizedTextService>();
            var contentTypeBaseServiceProvider = new Mock<IContentTypeBaseServiceProvider>();
            contentTypeBaseServiceProvider.Setup(x => x.GetContentTypeOf(It.IsAny<IContentBase>())).Returns(new ContentType(mockShortStringHelper, 123));
            var contentAppFactories = new Mock<List<IContentAppFactory>>();
            var mockContentAppFactoryCollection = new Mock<ILogger<ContentAppFactoryCollection>>();
            var hybridBackOfficeSecurityAccessor = new HybridBackofficeSecurityAccessor(new DictionaryAppCache());
            var contentAppFactoryCollection = new ContentAppFactoryCollection(
                contentAppFactories.Object,
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
            IDataEditor dataEditor = Mock.Of<IDataEditor>(
                x => x.Type == EditorType.PropertyValue
                     && x.Alias == Constants.PropertyEditors.Aliases.Label);
            Mock.Get(dataEditor).Setup(x => x.GetValueEditor()).Returns(new TextOnlyValueEditor(Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(), new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox, "Test Textbox", "textbox"), textService.Object, Mock.Of<IShortStringHelper>(), Mock.Of<IJsonSerializer>()));

            var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(new[] { dataEditor }));

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
                    propertyEditorCollection),
                new HttpContextAccessor());

            var map = new MapDefinitionCollection(new List<IMapDefinition>()
            {
                new global::Umbraco.Core.Models.Mapping.MemberMapDefinition(),
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
                    new Mock<IOptions<GlobalSettings>>().Object,
                    new Mock<IHostingEnvironment>().Object)
            });
            _mapper = new UmbracoMapper(map);

            return new MemberController(
                new DefaultCultureDictionary(
                    new Mock<ILocalizationService>().Object,
                    new HttpRequestAppCache(() => null)),
                new LoggerFactory(),
                mockShortStringHelper,
                new DefaultEventMessagesFactory(
                    new Mock<IEventMessagesAccessor>().Object),
                textService.Object,
                propertyEditorCollection,
                _mapper,
                memberService,
                memberTypeService,
                membersUserManager,
                dataTypeService,
                backOfficeSecurityAccessor,
                new ConfigurationEditorJsonSerializer());
        }


        /// <summary>
        /// Setup all standard member data for test
        /// </summary>
        private Member SetupMemberTestData(
            out MemberSave fakeMemberData,
            out MemberDisplay memberDisplay,
            ContentSaveAction contentAction)
        {
            // arrange
            MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            Member member = MemberBuilder.CreateSimpleMember(memberType, "Test Member", "test@example.com", "123", "test");
            int memberId = 123;
            member.Id = memberId;

            //TODO: replace with builder for MemberSave and MemberDisplay
            fakeMemberData = new MemberSave()
            {
                Id = memberId,
                SortOrder = member.SortOrder,
                ContentTypeId = memberType.Id,
                Key = member.Key,
                Password = new ChangingPasswordModel()
                {
                    Id = 456,
                    NewPassword = member.RawPasswordValue,
                    OldPassword = null
                },
                Name = member.Name,
                Email = member.Email,
                Username = member.Username,
                PersistedContent = member,
                PropertyCollectionDto = new ContentPropertyCollectionDto()
                {
                },
                Groups = new List<string>(),
                //Alias = "fakeAlias",
                ContentTypeAlias = member.ContentTypeAlias,
                Action = contentAction,
                Icon = "icon-document",
                Path = member.Path
            };

            memberDisplay = new MemberDisplay()
            {
                Id = memberId,
                SortOrder = member.SortOrder,
                ContentTypeId = memberType.Id,
                Key = member.Key,
                Name = member.Name,
                Email = member.Email,
                Username = member.Username,
                //Alias = "fakeAlias",
                ContentTypeAlias = member.ContentTypeAlias,
                ContentType = new ContentTypeBasic(),
                ContentTypeName = member.ContentType.Name,
                Icon = fakeMemberData.Icon,
                Path = member.Path,
                Tabs = new List<Tab<ContentPropertyDisplay>>()
                {
                    new Tab<ContentPropertyDisplay>()
                    {
                        Alias = "test",
                        Id = 77,
                        Properties = new List<ContentPropertyDisplay>()
                        {
                            new ContentPropertyDisplay()
                            {
                                Alias = "_umb_id",
                                View = "idwithguid",
                                Value = new []
                                {
                                    "123",
                                    "guid"
                                }
                            },
                            new ContentPropertyDisplay()
                            {
                                Alias = "_umb_doctype"
                            },
                            new ContentPropertyDisplay()
                            {
                                Alias = "_umb_login"
                            },
                            new ContentPropertyDisplay()
                            {
                                Alias= "_umb_email"
                            },
                            new ContentPropertyDisplay()
                            {
                                Alias = "_umb_password"
                            },
                            new ContentPropertyDisplay()
                            {
                                Alias = "_umb_membergroup"
                            }
                        }
                    }
                }
            };

            return member;
        }

        /// <summary>
        /// Check all member properties are equal
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

            //TODO: can we check create/update dates when saving?
            //Assert.AreEqual(memberDisplay.CreateDate, resultValue.CreateDate);
            //Assert.AreEqual(memberDisplay.UpdateDate, resultValue.UpdateDate);

            //TODO: check all properties
            Assert.AreEqual(memberDisplay.Properties.Count(), resultValue.Properties.Count());
            Assert.AreNotSame(memberDisplay.Properties, resultValue.Properties);
            for (var index = 0; index < resultValue.Properties.Count(); index++)
            {
                Assert.AreNotSame(memberDisplay.Properties.GetItemByIndex(index), resultValue.Properties.GetItemByIndex(index));
                //Assert.AreEqual(memberDisplay.Properties.GetItemByIndex(index), resultValue.Properties.GetItemByIndex(index));
            }
        }
    }
}
