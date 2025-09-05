using System.Data.SqlTypes;
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
[Test]
[TestCase("01/01/0001 10:00", true)]
[TestCase("10/10/1000 10:00", true)]
[TestCase("01/01/2000 10:00", false)]
public void Ensure_Correct_DateTime_Conversion(DateTime dateTime, bool shouldConvert)
    {
        var dateTimePropertyEditor = new DateTimePropertyEditor.DateTimePropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("Alias"));

        Dictionary<string, object> dictionary = new Dictionary<string, object> { { "Format", "HH:mm" } };
        ContentPropertyData propertyData = new ContentPropertyData(dateTime, dictionary);
        var value = dateTimePropertyEditor.FromEditor(propertyData, null);

        DateTime converted = DateTime.Parse(value as string ?? string.Empty);
        Assert.IsInstanceOf<DateTime>(converted);
        if (shouldConvert)
        {
            Assert.AreEqual(converted.Year, SqlDateTime.MinValue.Value.Year);
        }
        else
        {
            if (value is DateTime dateTime2)
            {
                Assert.AreEqual(converted.Year, dateTime2.Year);
            }
        }
    }
}
