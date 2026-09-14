// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class ContentSettingsValidatorTests
    {
        private Mock<ILogger<ContentSettingsValidator>> _loggerMock;

        [SetUp]
        public void SetUp() => _loggerMock = new Mock<ILogger<ContentSettingsValidator>>();

        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Error404Collection_Due_To_Empty_Culture()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings(culture: string.Empty);
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_AutoFillImageProperties_Collection()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings(culture: string.Empty);
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public void Does_Not_Log_Warning_For_Default_PreviewBadge()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings();

            validator.Validate("settings", options);

            Assert.That(GetWarningLogCount(), Is.EqualTo(0));
        }

        [TestCase(@"<script{3} src=""{0}/website/preview.js""></script>")]
        [TestCase("")]
        [TestCase(null)]
        public void Does_Not_Log_Warning_For_PreviewBadge_Not_Needing_A_Nonce(string? previewBadge)
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings();
            options.PreviewBadge = previewBadge!;

            validator.Validate("settings", options);

            Assert.That(GetWarningLogCount(), Is.EqualTo(0));
        }

        [Test]
        public void Returns_Success_But_Logs_Warning_For_Customised_PreviewBadge_Without_Nonce_Placeholder()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings();
            options.PreviewBadge = @"<script src=""{0}/website/preview.js""></script>";

            ValidateOptionsResult result = validator.Validate("settings", options);

            Assert.Multiple(() =>
            {
                Assert.That(result.Succeeded, Is.True);
                Assert.That(GetWarningLogCount(), Is.EqualTo(1));
            });
        }

        [Test]
        public void Logs_Warning_Once_When_PreviewBadge_Is_Validated_Repeatedly()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings();
            options.PreviewBadge = @"<script src=""{0}/website/preview.js""></script>";

            validator.Validate("settings", options);
            validator.Validate("settings", options);
            validator.Validate(Options.DefaultName, BuildContentSettings());

            Assert.That(GetWarningLogCount(), Is.EqualTo(1));
        }

        [Test]
        public void Logs_Warning_Again_When_PreviewBadge_Changes()
        {
            var validator = CreateValidator();
            ContentSettings options = BuildContentSettings();

            options.PreviewBadge = @"<script src=""{0}/website/preview.js""></script>";
            validator.Validate("settings", options);

            options.PreviewBadge = @"<script defer src=""{0}/website/preview.js""></script>";
            validator.Validate("settings", options);

            Assert.That(GetWarningLogCount(), Is.EqualTo(2));
        }

        private ContentSettingsValidator CreateValidator() => new(_loggerMock.Object);

        private int GetWarningLogCount() =>
            _loggerMock.Invocations
                .Count(invocation =>
                    invocation.Method.Name == nameof(ILogger.Log) &&
                    invocation.Arguments.OfType<LogLevel>().Any(level => level == LogLevel.Warning));

        private static ContentSettings BuildContentSettings(string culture = "en-US", string autoFillImagePropertyAlias = "testAlias") =>
            new ContentSettings
            {
                Error404Collection =
                [
                    new() { Culture = culture, ContentId = 1 },
                ],
                Imaging =
                {
                    AutoFillImageProperties =
                    {
                        new() { Alias = autoFillImagePropertyAlias, WidthFieldAlias = "w", HeightFieldAlias = "h", LengthFieldAlias = "l", ExtensionFieldAlias = "e" },
                    },
                },
            };
    }
}
