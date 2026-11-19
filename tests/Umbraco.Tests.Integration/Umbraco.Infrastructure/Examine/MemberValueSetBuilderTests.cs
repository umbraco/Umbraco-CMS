using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Examine;

[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[TestFixture]
internal sealed class MemberValueSetBuilderTests : UmbracoIntegrationTest
{
    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IValueSetBuilder<IMember> MemberValueSetBuilder => GetRequiredService<IValueSetBuilder<IMember>>();

    [Test]
    public async Task Value_Sets_Exclude_Sensitive_Property_Values()
    {
        MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.SetIsSensitiveProperty("title", true);
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        IMember member = MemberService.CreateMember("member", "member@example.com", "Member", memberType);
        member.SetValue("title", "Sensitive value");
        member.SetValue("author", "Non-sensitive value");
        MemberService.Save(member);

        ValueSet valueSet = MemberValueSetBuilder.GetValueSets(member).Single();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(valueSet.Values.ContainsKey("title"));
            Assert.IsTrue(valueSet.Values.ContainsKey("author"));
            Assert.AreEqual("Non-sensitive value", valueSet.GetValue("author"));
        });
    }
}
