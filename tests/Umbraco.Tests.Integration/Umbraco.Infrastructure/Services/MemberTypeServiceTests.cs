// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class MemberTypeServiceTests : UmbracoIntegrationTest
{
    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    [Test]
    public async Task Member_Cannot_Edit_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes)
        {
            Assert.IsFalse(memberType.MemberCanEditProperty(p.Alias));
        }
    }

    [Test]
    public async Task Member_Can_Edit_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        var prop = memberType.PropertyTypes.First().Alias;
        memberType.SetMemberCanEditProperty(prop, true);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes.Where(x => x.Alias != prop))
        {
            Assert.IsFalse(memberType.MemberCanEditProperty(p.Alias));
        }

        Assert.IsTrue(memberType.MemberCanEditProperty(prop));
    }

    [Test]
    public async Task Member_Cannot_View_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes)
        {
            Assert.IsFalse(memberType.MemberCanViewProperty(p.Alias));
        }
    }

    [Test]
    public async Task Member_Can_View_Property()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        var prop = memberType.PropertyTypes.First().Alias;
        memberType.SetMemberCanViewProperty(prop, true);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        // re-get
        memberType = MemberTypeService.Get(memberType.Id);
        foreach (var p in memberType.PropertyTypes.Where(x => x.Alias != prop))
        {
            Assert.IsFalse(memberType.MemberCanViewProperty(p.Alias));
        }

        Assert.IsTrue(memberType.MemberCanViewProperty(prop));
    }

    [Test]
    public async Task Deleting_PropertyType_Removes_The_Property_From_Member()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
        MemberService.Save(member);
        var initProps = member.Properties.Count;

        // remove a property (NOT ONE OF THE DEFAULTS)
        var standardProps = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);
        memberType.RemovePropertyType(memberType.PropertyTypes.First(x => standardProps.ContainsKey(x.Alias) == false)
            .Alias);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        // re-load it from the db
        member = MemberService.GetById(member.Id);

        Assert.AreEqual(initProps - 1, member.Properties.Count);
    }

    [Test]
    public async Task Cannot_Create_MemberType_With_Empty_Name()
    {
        // Arrange
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("memberTypeAlias", string.Empty);

        // Act
        var result = await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.NameCannotBeEmpty, result.Result);
    }

    [Test]
    public async Task Empty_Description_Is_Always_Null_After_Saving_Member_Type()
    {
        var memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.Description = null;
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        var memberType2 = MemberTypeBuilder.CreateSimpleMemberType("memberType2", "Member Type 2");
        memberType2.Description = string.Empty;
        await MemberTypeService.CreateAsync(memberType2, Constants.Security.SuperUserKey);

        Assert.IsNull(memberType.Description);
        Assert.IsNull(memberType2.Description);
    }

    [Test]
    public async Task GetAllAllowedAsRootAsync_Returns_All_MemberTypes()
    {
        // Arrange
        PagedModel<IMemberType> baseline = await MemberTypeService.GetAllAllowedAsRootAsync(0, 1000);

        IMemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType("type1", "Type 1");
        await MemberTypeService.CreateAsync(memberType1, Constants.Security.SuperUserKey);

        IMemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType("type2", "Type 2");
        await MemberTypeService.CreateAsync(memberType2, Constants.Security.SuperUserKey);

        // Act
        PagedModel<IMemberType> result = await MemberTypeService.GetAllAllowedAsRootAsync(0, 1000);

        // Assert - members are a flat list, so all member types should be returned.
        Assert.Multiple(() =>
        {
            Assert.AreEqual(baseline.Total + 2, result.Total);
            Assert.IsTrue(result.Items.Any(x => x.Key == memberType1.Key));
            Assert.IsTrue(result.Items.Any(x => x.Key == memberType2.Key));
        });
    }
}
