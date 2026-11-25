using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal sealed class EntityPickerValueParserTests
{
    private static readonly Guid _first = new("0f4f9a0e-6b1e-4d0f-9b1c-2a3f6e5d4c3b");
    private static readonly Guid _second = new("8c2d7e61-3a4b-4c5d-8e9f-0a1b2c3d4e5f");

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("[]")]
    public void Reads_No_Keys_From_An_Empty_Value(string? value)
        => Assert.That(Parse(value), Is.Empty);

    [Test]
    public void Reads_A_Json_Array_Of_Keys()
        => Assert.That(Parse($"[\"{_first}\",\"{_second}\"]"), Is.EqualTo(new[] { _first, _second }));

    [Test]
    public void Reads_A_Json_Array_Of_Keys_Held_As_A_Json_Node()
        => Assert.That(Parse(new JsonArray(_first.ToString(), _second.ToString())), Is.EqualTo(new[] { _first, _second }));

    [Test]
    public void Reads_A_Json_Array_Of_Keys_Held_As_A_Json_Element()
        => Assert.That(
            Parse(JsonDocument.Parse($"[\"{_first}\",\"{_second}\"]").RootElement),
            Is.EqualTo(new[] { _first, _second }));

    [TestCase("N")]
    [TestCase("B")]
    [TestCase("P")]
    public void Reads_A_Json_Array_Of_Keys_In_Any_Guid_Format(string format)
        => Assert.That(
            Parse($"[\"{_first.ToString(format)}\",\"{_second.ToString(format).ToUpperInvariant()}\"]"),
            Is.EqualTo(new[] { _first, _second }));

    [Test]
    public void Reads_The_Keys_Of_A_Json_Array_Holding_Other_Entries()
        => Assert.That(
            Parse($"[\"{_first}\",\"not-a-key\",42,null,{{}},\"{_second}\"]"),
            Is.EqualTo(new[] { _first, _second }));

    [TestCase("{\"key\":\"0f4f9a0e-6b1e-4d0f-9b1c-2a3f6e5d4c3b\"}")]
    [TestCase("[\"0f4f9a0e-6b1e-4d0f-9b1c-2a3f6e5d4c3b\"")]
    public void Reads_No_Keys_From_Json_That_Is_Not_An_Array(string value)
        => Assert.That(Parse(value), Is.Empty);

    [Test]
    public void Reads_A_Single_Key()
        => Assert.That(Parse(_first.ToString()), Is.EqualTo(new[] { _first }));

    [Test]
    public void Reads_A_Single_Udi()
        => Assert.That(Parse(DocumentUdi(_first)), Is.EqualTo(new[] { _first }));

    [Test]
    public void Reads_A_Udi_Held_As_A_Udi_Object()
        => Assert.That(Parse(new GuidUdi(Constants.UdiEntityType.Document, _first)), Is.EqualTo(new[] { _first }));

    [Test]
    public void Reads_A_Comma_Separated_List_Of_Udis_In_Stored_Order()
        => Assert.That(Parse($"{DocumentUdi(_second)},{DocumentUdi(_first)}"), Is.EqualTo(new[] { _second, _first }));

    [Test]
    public void Ignores_Udis_Of_Other_Entity_Types()
        => Assert.That(
            Parse($"{MediaUdi(_first)},{DocumentUdi(_second)}"),
            Is.EqualTo(new[] { _second }));

    [Test]
    public void Ignores_Entries_That_Cannot_Be_Parsed()
        => Assert.That(
            Parse($"not-a-udi,,{DocumentUdi(_first)}, umb://document/not-a-guid"),
            Is.EqualTo(new[] { _first }));

    [Test]
    public void Reads_An_Entity_Picked_Twice_Twice()
        => Assert.That(
            Parse($"{DocumentUdi(_first)},{DocumentUdi(_second)},{DocumentUdi(_first)}"),
            Is.EqualTo(new[] { _first, _second, _first }));

    private static IReadOnlyList<Guid> Parse(object? value)
        => EntityPickerValueParser.ParseKeys(value, Constants.UdiEntityType.Document);

    private static string DocumentUdi(Guid key) => new GuidUdi(Constants.UdiEntityType.Document, key).ToString();

    private static string MediaUdi(Guid key) => new GuidUdi(Constants.UdiEntityType.Media, key).ToString();
}
