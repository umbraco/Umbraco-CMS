using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

/// <summary>
/// Contains unit tests for the <see cref="FileUploadValueRequiredValidator"/> class, verifying its validation logic and behavior.
/// </summary>
[TestFixture]
public class FileUploadValueRequiredValidatorTests
{
    /// <summary>
    /// Tests that an empty file upload is validated as not provided.
    /// </summary>
    [Test]
    public void Validates_Empty_File_Upload_As_Not_Provided()
    {
        var validator = new FileUploadValueRequiredValidator();

        var value = JsonNode.Parse("{ \"src\": \"\", \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Tests that the <see cref="FileUploadValueRequiredValidator"/> correctly validates when a file upload value is provided.
    /// Ensures that no validation errors are returned for a valid file upload JSON value.
    /// </summary>
    [Test]
    public void Valdiates_File_Upload_As_Provided()
    {
        var validator = new FileUploadValueRequiredValidator();

        var value = JsonNode.Parse("{ \"src\": \"fakePath\", \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.IsEmpty(result);
    }
}
