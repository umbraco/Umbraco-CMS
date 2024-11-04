using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

[TestFixture]
public class DateTimeValidatorTests
{
    [TestCase("en-US", "yyyy-MM-dd HH:mm:ss", TestName = "US Thread, DateTimeValidator")]
    [TestCase("en-US", "dd-MM-yyyy HH:mm:ss", TestName = "US Thread, DateTimeValidator ar-SA format")]
    [TestCase("ar-SA", "dd-MM-yyyy HH:mm:ss", TestName = "Arabian Saudi Thread, DateTimeValidator")]
    [TestCase("ar-SA", "yyyy-MM-dd HH:mm:ss", TestName = "Arabian Saudi Thread, DateTimeValidator US format")]
    public void DateTimeValidatorIsCultureInvariant(string culture, string format)
    {
        var dateString = DateTime.Now.ToString(format);

        var cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        var validator = new DateTimeValidator();
        var validationResults = validator.Validate(dateString, "DATETIME", new Dictionary<string, object>
        {
            ["format"] = format
        });
        CollectionAssert.IsEmpty(validationResults);
    }
}
