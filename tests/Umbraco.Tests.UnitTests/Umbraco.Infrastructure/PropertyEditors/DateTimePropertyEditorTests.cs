using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

public class DateTimePropertyEditorTests
{
    // Various time formats with years below the minimum, so we expect to increase the date to the minimum supported by SQL Server.
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
