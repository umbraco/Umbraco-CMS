using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaOperationResolverTests
{
    [TestCase("GET", SchemaOperation.Read)]
    [TestCase("HEAD", SchemaOperation.Read)]
    [TestCase("OPTIONS", SchemaOperation.Read)]
    [TestCase("POST", SchemaOperation.Create)]
    [TestCase("PUT", SchemaOperation.Update)]
    [TestCase("PATCH", SchemaOperation.Update)]
    [TestCase("DELETE", SchemaOperation.Delete)]
    public void Infers_Operation_From_Verb(string verb, SchemaOperation expected)
        => Assert.That(SchemaOperationResolver.Resolve(verb), Is.EqualTo(expected));

    [TestCase("TRACE")]
    [TestCase("LOCK")]
    public void Unrecognised_Verb_Is_Unknown(string verb)
        => Assert.That(SchemaOperationResolver.Resolve(verb), Is.EqualTo(SchemaOperation.Unknown));

    [Test]
    public void No_Verb_Is_Unknown()
        => Assert.That(SchemaOperationResolver.Resolve(httpMethod: null), Is.EqualTo(SchemaOperation.Unknown));
}
