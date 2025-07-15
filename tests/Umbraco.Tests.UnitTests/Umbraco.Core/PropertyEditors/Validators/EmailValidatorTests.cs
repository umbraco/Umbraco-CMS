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

[TestFixture]
public class EmailValidatorTests
{
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
