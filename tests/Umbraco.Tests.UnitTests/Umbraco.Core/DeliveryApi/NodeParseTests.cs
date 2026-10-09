// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Api.Common.Rendering;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// Tests the parsing of the Delivery API <c>expand</c> and <c>fields</c> parameter syntax into a node tree.
/// </summary>
[TestFixture]
public class NodeParseTests
{
    [TestCase("", "root[]", TestName = "Empty value yields a single empty node")]
    [TestCase("$all", "root[$all]")]
    [TestCase("properties[$all]", "root[properties[$all]]")]
    [TestCase("properties[header]", "root[properties[header]]")]
    [TestCase("properties[header,footerGrid,footerLegal]", "root[properties[header,footerGrid,footerLegal]]")]
    [TestCase("properties[contentPicker[properties[title]]]", "root[properties[contentPicker[properties[title]]]]")]
    [TestCase("properties[element[properties[$all]]]", "root[properties[element[properties[$all]]]]")]
    [TestCase(
        "properties[pickerOne[properties[numberOne]],pickerTwo[properties[numberTwo]]]",
        "root[properties[pickerOne[properties[numberOne]],pickerTwo[properties[numberTwo]]]]")]
    [TestCase("header,footerGrid", "root[header,footerGrid]", TestName = "Commas separate nodes at the root level too")]
    public void Can_Parse_Expand_And_Fields_Syntax(string value, string expected)
        => Assert.AreEqual(expected, ParseProbe.ParseToString(value));

    [TestCase("properties[header")]
    [TestCase("properties[contentPicker[properties[title]]")]
    [TestCase("properties[header]]")]
    public void Cannot_Parse_Value_With_Unbalanced_Brackets(string value)
        => Assert.Throws<ArgumentException>(() => ParseProbe.ParseToString(value));

    [TestCase("[properties]")]
    [TestCase("[$all]")]
    public void Cannot_Parse_Value_Starting_With_Bracket(string value)
        => Assert.Throws<ArgumentException>(() => ParseProbe.ParseToString(value));

    [TestCase("properties[]")]
    [TestCase("properties[contentPicker[]]")]
    public void Cannot_Parse_Value_With_Empty_Brackets(string value)
        => Assert.Throws<ArgumentException>(() => ParseProbe.ParseToString(value));

    /// <summary>
    /// <c>Node</c> is protected, so it is reached through a derived type. The tree is rendered back into the
    /// parameter syntax it was parsed from, so expectations read the same way as the input.
    /// </summary>
    private sealed class ParseProbe : ElementOnlyOutputExpansionStrategy
    {
        // Never instantiated - Parse is static. A constructor is only needed because the base class has no
        // parameterless one.
        private ParseProbe()
            : base(null!, null!, null!)
        {
        }

        public static string ParseToString(string value) => Describe(Node.Parse(value));

        private static string Describe(Node node) => node.Items.Count == 0
            ? node.Key
            : $"{node.Key}[{string.Join(",", node.Items.Select(Describe))}]";
    }
}
