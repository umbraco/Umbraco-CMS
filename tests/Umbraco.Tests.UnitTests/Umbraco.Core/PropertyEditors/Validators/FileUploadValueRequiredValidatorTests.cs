using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

[TestFixture]
public class FileUploadValueRequiredValidatorTests
{
    [Test]
    public void Validates_Empty_File_Upload_As_Not_Provided()
    {
        var validator = new FileUploadValueRequiredValidator();

        var value = JsonNode.Parse("{ \"src\": \"\", \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Valdiates_File_Upload_As_Provided()
    {
        var validator = new FileUploadValueRequiredValidator();

        var value = JsonNode.Parse("{ \"src\": \"fakePath\", \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.IsEmpty(result);
    }
}
