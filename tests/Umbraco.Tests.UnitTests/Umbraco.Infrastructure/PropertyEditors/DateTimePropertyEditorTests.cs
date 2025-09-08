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
    [TestCase("01/01/0001 10:00", "01/01/1753 10:00", "hh:mm")]
    [TestCase("01/01/0001 10:00", "01/01/1753 10:00", "hh mm")]
    [TestCase("10/10/1000 10:00", "10/10/1753 10:00", "hh:mm:ss")]
    [TestCase("10/10/1000 10:00", "10/10/1753 10:00", "hh-mm-ss")]
    [TestCase("10/10/1000 10:00", "10/10/1753 10:00", "hh/mm/ss")]
    [TestCase("10/10/1000 10:00", "10/10/1753 10:00", "hh%mm%ss")] // Weird format separators customers potentially might use.
    [TestCase("01/01/2000 10:00", "01/01/2000 10:00", "HH:mm")]
    [TestCase("01/01/0001 10:00", "01/01/0001 10:00", "dd:MM:yyyy hh:mm")] // Inconvertible format doesn't get converted.
    public void Ensure_Correct_DateTime_Conversion(DateTime actualDateTime, DateTime expectedDateTime, string format)
    {
        var dateTimePropertyEditor = new DateTimePropertyEditor.DateTimePropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("Alias"));

        Dictionary<string, object> dictionary = new Dictionary<string, object> { { "Format", format } };
        ContentPropertyData propertyData = new ContentPropertyData(actualDateTime, dictionary);
        var value = dateTimePropertyEditor.FromEditor(propertyData, null);

        DateTime converted = DateTime.Parse(value as string ?? string.Empty);
        Assert.IsInstanceOf<DateTime>(converted);
        Assert.AreEqual(expectedDateTime, converted);
    }
}
