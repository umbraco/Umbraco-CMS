using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="DateTimePropertyEditor"/> class in the Umbraco CMS infrastructure.
/// </summary>
public class DateTimePropertyEditorTests
{
    // Various time formats with years below the minimum, so we expect to increase the date to the minimum supported by SQL Server.
    /// <summary>
    /// Verifies that a <see cref="DateTime"/> value with a time-only format is persisted correctly.
    /// If the date component is below the minimum supported by SQL Server (1753-01-01),
    /// the date is adjusted to the minimum; otherwise, the value is persisted as-is.
    /// Date formats that include a full date are not adjusted, even if the year is below the minimum.
    /// </summary>
    /// <param name="actualDateTime">The input <see cref="DateTime"/> value to test.</param>
    /// <param name="expectedDateTime">The expected <see cref="DateTime"/> value after processing.</param>
    /// <param name="format">The format string indicating whether the value is time-only or includes a date.</param>
    [TestCase("01/01/0001 10:00", "01/01/1753 10:00", "hh:mm")]
    [TestCase("01/01/0001 10:00", "01/01/1753 10:00", "HH:mm")]
    [TestCase("01/01/0001 10:00", "01/01/1753 10:00", "hh mm")]
    [TestCase("10/10/1000 10:00", "10/10/1753 10:00", "hh:mm:ss")]
    [TestCase("10/10/1000 10:00", "10/10/1753 10:00", "hh-mm-ss")]

    // Time format with year above the minimum, so we expect to not convert.
    [TestCase("01/01/2000 10:00", "01/01/2000 10:00", "HH:mm")]

    // Date formats, so we don't convert even if the year is below the minimum.
    [TestCase("01/01/0001 10:00", "01/01/0001 10:00", "dd-MM-yyyy hh:mm")]
    [TestCase("01/01/0001 10:00", "01/01/0001 10:00", "dd-MM-yyyy")]
        [TestCase("01/01/0001 10:00", "01/01/0001 10:00", "yyyy-MM-d")]
    public void Time_Only_Format_Ensures_DateTime_Can_Be_Persisted(DateTime actualDateTime, DateTime expectedDateTime, string format)
    {
        var dateTimePropertyEditor = new DateTimePropertyEditor.DateTimePropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("Alias") { ValueType = ValueTypes.DateTime.ToString() });

        Dictionary<string, object> dictionary = new Dictionary<string, object> { { DateTimePropertyEditor.DateTimePropertyValueEditor.DateTypeConfigurationFormatKey, format } };
        ContentPropertyData propertyData = new ContentPropertyData(actualDateTime, dictionary);
        var value = (DateTime)dateTimePropertyEditor.FromEditor(propertyData, null);

        Assert.AreEqual(expectedDateTime, value);
    }
}
