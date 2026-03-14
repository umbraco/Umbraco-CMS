// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

    /// <summary>
    /// Unit tests for the EmailValidator class.
    /// </summary>
[TestFixture]
public class EmailValidatorTests
{
    /// <summary>
    /// Tests the email validator to ensure it correctly determines whether a given email address is valid.
    /// </summary>
    /// <param name="email">The email address to validate. Can be <c>null</c>, empty, or a string value.</param>
    /// <param name="expectedSuccess">True if the email is expected to be considered valid; otherwise, false.</param>
    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase(" ", false)]
    [TestCase("test@test.com", true)]
    [TestCase("invalid", false)]
    public void Validates_Email_Address(object? email, bool expectedSuccess)
    {
        var validator = CreateValidator();
        var result = validator.Validate(email, ValueTypes.String, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_invalidEmail", validationResult.ErrorMessage);
        }
    }

    private static EmailValidator CreateValidator()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");
        return new EmailValidator(localizedTextServiceMock.Object);
    }
}
