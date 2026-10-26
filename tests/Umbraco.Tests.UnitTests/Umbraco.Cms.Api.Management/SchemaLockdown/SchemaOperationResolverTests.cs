using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaOperationResolverTests
{
    private const string DocumentType = Constants.UdiEntityType.DocumentType;

    private const string Template = Constants.UdiEntityType.Template;

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

    [Test]
    public void Every_Entity_Type_Falls_Back_To_The_Verb_When_Nothing_Is_Declared()
        => Assert.That(
            Resolve(HttpMethods.Delete),
            Is.EqualTo(new[]
            {
                (DocumentType, SchemaOperation.Delete),
                (Template, SchemaOperation.Delete),
            }));

    [Test]
    public void An_Unrecognised_Verb_Leaves_Every_Entity_Type_Unknown()
        => Assert.That(
            Resolve("LOCK"),
            Is.EqualTo(new[]
            {
                (DocumentType, SchemaOperation.Unknown),
                (Template, SchemaOperation.Unknown),
            }));

    // Creating a template for a document type is the case in the codebase: the verb describes what happens to the
    // template, and only the document type needs saying.
    [Test]
    public void A_Declaration_Naming_An_Entity_Type_Applies_To_That_One_Alone()
        => Assert.That(
            Resolve(HttpMethods.Post, new SchemaOperationAttribute(DocumentType, SchemaOperation.Update)),
            Is.EqualTo(new[]
            {
                (DocumentType, SchemaOperation.Update),
                (Template, SchemaOperation.Create),
            }));

    [Test]
    public void A_Declaration_Naming_No_Entity_Type_Applies_To_All_Of_Them()
        => Assert.That(
            Resolve(HttpMethods.Post, new SchemaOperationAttribute(SchemaOperation.Read)),
            Is.EqualTo(new[]
            {
                (DocumentType, SchemaOperation.Read),
                (Template, SchemaOperation.Read),
            }));

    [Test]
    public void A_Declaration_Naming_An_Entity_Type_Beats_One_Naming_None()
        => Assert.That(
            Resolve(
                HttpMethods.Post,
                new SchemaOperationAttribute(SchemaOperation.Read),
                new SchemaOperationAttribute(DocumentType, SchemaOperation.Update)),
            Is.EqualTo(new[]
            {
                (DocumentType, SchemaOperation.Update),
                (Template, SchemaOperation.Read),
            }));

    [Test]
    public void A_Declaration_Matches_Its_Entity_Type_Whatever_The_Casing()
        => Assert.That(
            Resolve(HttpMethods.Post, new SchemaOperationAttribute("Document-Type", SchemaOperation.Update)),
            Is.EqualTo(new[]
            {
                (DocumentType, SchemaOperation.Update),
                (Template, SchemaOperation.Create),
            }));

    // A SchemaOperation declaration only decides an operation. The name comes from the controller's declaration,
    // which is the one thing that says what the controller manages, so the two disagreeing about casing cannot
    // change what a denial is reported as.
    [Test]
    public void An_Entity_Type_Is_Named_As_The_Controller_Declared_It_Not_As_An_Operation_Did()
        => Assert.That(
            SchemaOperationResolver.Resolve(
                ["Document-Type"],
                [new SchemaOperationAttribute("DOCUMENT-TYPE", SchemaOperation.Update)],
                HttpMethods.Post),
            Is.EqualTo(new[] { ("Document-Type", SchemaOperation.Update) }));

    // The composition pickers are POST because the query needs a request body. Nothing here is deniable, which is
    // what lets the OpenAPI document leave the denial header off them.
    [Test]
    public void A_Post_Declared_As_A_Read_Leaves_Nothing_Deniable()
    {
        (string EntityType, SchemaOperation Operation)[] governed =
            Resolve(HttpMethods.Post, new SchemaOperationAttribute(SchemaOperation.Read));

        Assert.That(Array.FindAll(governed, x => x.Operation != SchemaOperation.Read), Is.Empty);
    }

    private static (string EntityType, SchemaOperation Operation)[] Resolve(
        string httpMethod,
        params SchemaOperationAttribute[] declared)
        => SchemaOperationResolver.Resolve([DocumentType, Template], declared, httpMethod);
}
