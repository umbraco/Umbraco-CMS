using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaOperationResolverTests
{
    [TestCase("POST", Constants.Permissions.DocumentType.Create)]
    [TestCase("PUT", Constants.Permissions.DocumentType.Update)]
    [TestCase("PATCH", Constants.Permissions.DocumentType.Update)]
    [TestCase("DELETE", Constants.Permissions.DocumentType.Delete)]
    public void Infers_The_Permission_From_The_Verb(string verb, string expected)
        => Assert.That(ResolveForDocumentType(verb), Is.EqualTo(new[] { expected }));

    [TestCase("TRACE")]
    [TestCase("LOCK")]
    [TestCase(null)]
    public void An_Unrecognised_Or_Missing_Verb_Needs_The_Update_Permission(string? verb)
        => Assert.That(ResolveForDocumentType(verb), Is.EqualTo(new[] { Constants.Permissions.DocumentType.Update }));

    [Test]
    public void Without_A_Declaration_Every_Entity_Type_Needs_The_Http_Verbs_Permission()
        => Assert.That(
            Resolve(HttpMethods.Delete),
            Is.EqualTo(new[] { Constants.Permissions.DocumentType.Delete, Constants.Permissions.Template.Delete }));

    [TestCase("GET")]
    [TestCase("HEAD")]
    [TestCase("OPTIONS")]
    public void A_Safe_Verb_Needs_No_Permission(string httpMethod)
        => Assert.That(Resolve(httpMethod), Is.Empty);

    [Test]
    public void Declared_Permissions_Replace_The_Http_Verbs_Permissions()
        => Assert.That(
            Resolve(
                HttpMethods.Post,
                new SchemaOperationAttribute([Constants.Permissions.Template.Create, Constants.Permissions.DocumentType.Update])),
            Is.EqualTo(new[] { Constants.Permissions.Template.Create, Constants.Permissions.DocumentType.Update }));

    [Test]
    public void A_Permission_Declared_Twice_Is_Needed_Once()
        => Assert.That(
            Resolve(
                HttpMethods.Post,
                new SchemaOperationAttribute([Constants.Permissions.DocumentType.Update, Constants.Permissions.DocumentType.Update])),
            Is.EqualTo(new[] { Constants.Permissions.DocumentType.Update }));

    // The composition pickers are POST because the query needs a request body.
    [Test]
    public void Declaring_No_Permissions_Needs_None()
        => Assert.That(Resolve(HttpMethods.Post, new SchemaOperationAttribute([])), Is.Empty);

    [Test]
    public void The_Entity_Types_Casing_Does_Not_Change_The_Permission()
        => Assert.That(
            SchemaOperationResolver.ResolvePermissions(["Document-Type"], null, HttpMethods.Post),
            Is.EqualTo(new[] { Constants.Permissions.DocumentType.Create }));

    private static string[] ResolveForDocumentType(string? httpMethod)
        => SchemaOperationResolver.ResolvePermissions([Constants.UdiEntityType.DocumentType], null, httpMethod);

    private static string[] Resolve(string httpMethod, SchemaOperationAttribute? declared = null)
        => SchemaOperationResolver.ResolvePermissions(
            [Constants.UdiEntityType.DocumentType, Constants.UdiEntityType.Template],
            declared,
            httpMethod);
}
