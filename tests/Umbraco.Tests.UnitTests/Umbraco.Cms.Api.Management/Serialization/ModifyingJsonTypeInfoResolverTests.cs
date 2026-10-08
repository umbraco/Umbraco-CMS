using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class ModifyingJsonTypeInfoResolverTests
{
    [Test]
    public void Delegates_Polymorphism_Lookups_To_The_Inner_Resolver()
    {
        var inner = new Mock<IUmbracoJsonTypeInfoResolver>();
        inner.Setup(x => x.FindSubTypes(typeof(object))).Returns([typeof(string)]);
        inner.Setup(x => x.GetTypeDiscriminatorValue(typeof(string))).Returns("discriminator");

        var resolver = new ModifyingJsonTypeInfoResolver(inner.Object, _ => { });

        Assert.Multiple(() =>
        {
            Assert.That(resolver.FindSubTypes(typeof(object)), Is.EqualTo(new[] { typeof(string) }));
            Assert.That(resolver.GetTypeDiscriminatorValue(typeof(string)), Is.EqualTo("discriminator"));
        });
    }
}
