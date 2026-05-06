using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Entities;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
public class UserStartNodeEntitiesServiceTests
{
    [Test]
    public void GetAllowedIds_Returns_Next_Child_Ids_For_Paths_Containing_Parent()
    {
        string[] startNodePaths = ["-1,2,3,4,5", "-1,2,3,9,10"];

        var allowed = UserStartNodeEntitiesService.GetAllowedIds(startNodePaths, parentId: 3);

        CollectionAssert.AreEquivalent(new[] { 4, 9 }, allowed);
    }

    [Test]
    public void GetAllowedIds_Returns_Empty_When_No_Path_Contains_Parent()
    {
        string[] startNodePaths = ["-1,2,3,4,5"];

        var allowed = UserStartNodeEntitiesService.GetAllowedIds(startNodePaths, parentId: 99);

        Assert.IsEmpty(allowed);
    }

    // Regression test for https://github.com/umbraco/Umbraco-CMS/issues/22610: under cultures whose
    // NumberFormatInfo.NegativeSign is not the ASCII hyphen-minus, int.Parse("-1") with no
    // IFormatProvider throws because the parser uses CultureInfo.CurrentCulture. Start-node paths
    // persist the root marker as ASCII "-1".
    [Test]
    public void GetAllowedIds_Parses_Root_Marker_Under_Culture_With_Non_Ascii_Negative_Sign()
    {
        var savedCulture = CultureInfo.CurrentCulture;
        try
        {
            // Mirrors ar-EG's NegativeSign under ICU (U+061C ARABIC LETTER MARK + ASCII hyphen).
            // Using an explicit clone keeps the test deterministic regardless of which ICU version
            // is installed on the test host.
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NegativeSign = "؜-";
            CultureInfo.CurrentCulture = culture;

            string[] startNodePaths = ["-1,2,3,4,5"];

            var allowed = UserStartNodeEntitiesService.GetAllowedIds(startNodePaths, parentId: 3);

            CollectionAssert.AreEqual(new[] { 4 }, allowed);
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }
    }
}
