using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// Unit test fixture for the DateTimeValidator class.
    /// </summary>
    [TestFixture]
    public class DateTimeValidatorTests
    {
        /// <summary>
        /// Tests that the DateTimeValidator is culture invariant.
        /// </summary>
        /// <param name="culture">The culture to set for the current thread.</param>
        /// <param name="format">The date format to use for the test.</param>
        [TestCase("en-US", "yyyy-MM-dd HH:mm:ss", TestName = "US Thread, DateTimeValidator")]
        [TestCase("en-US", "dd-MM-yyyy HH:mm:ss", TestName = "US Thread, DateTimeValidator ar-SA format")]
        [TestCase("ar-SA", "dd-MM-yyyy HH:mm:ss", TestName = "Arabian Saudi Thread, DateTimeValidator")]
        [TestCase("ar-SA", "yyyy-MM-dd HH:mm:ss", TestName = "Arabian Saudi Thread, DateTimeValidator US format")]
        public void DateTimeValidatorIsCultureInvariant(string culture, string format)
        {
            // Generate a date string using the specified format
            var dateString = DateTime.Now.ToString(format);

            // Set the current thread's culture and UI culture
            var cultureInfo = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // Create a new DateTimeValidator instance
            var validator = new DateTimeValidator();

            // Validate the date string
            var validationResults = validator.Validate(
                dateString,
                "DATETIME",
                new Dictionary<string, object>(),
                PropertyValidationContext.Empty());

            // Assert that there are no validation errors
            CollectionAssert.IsEmpty(validationResults);
        }
    }
}
