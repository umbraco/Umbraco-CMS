// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

/// <summary>
/// Tests for <see cref="RteBlockHelper"/> — the helper used by the v15 RTE block migration
/// (consumed by both <c>ConvertRichTextEditorProperties</c> and <c>LocalLinkRteProcessor</c>) to
/// rewrite legacy <c>data-content-udi</c> attributes on <c>&lt;umb-rte-block&gt;</c> elements to
/// the v15+ <c>data-content-key</c> form.
/// </summary>
[TestFixture]
#pragma warning disable CS0618 // RteBlockHelper is obsolete (scheduled for removal in Umbraco 18).
public class RteBlockHelperTests
{
    [Test]
    public void ConvertBlockUdisToKeys_SingleBlock_ConvertsUdiToHyphenatedGuidKey()
    {
        var input = @"<p>before</p><umb-rte-block data-content-udi=""umb://element/5e499fc237be4b1d974670526f3b00b7""><!--Umbraco-Block--></umb-rte-block><p>after</p>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(
            @"<p>before</p><umb-rte-block data-content-key=""5e499fc2-37be-4b1d-9746-70526f3b00b7""><!--Umbraco-Block--></umb-rte-block><p>after</p>",
            result);
    }

    /// <summary>
    /// Two direct-sibling blocks must each be converted independently.
    /// </summary>
    [Test]
    public void ConvertBlockUdisToKeys_TwoConsecutiveSiblingBlocks_ConvertsEachIndividually()
    {
        var input =
            @"<umb-rte-block data-content-udi=""umb://element/5e499fc237be4b1d974670526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-udi=""umb://element/9db16c0d251e414c874967090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(
            @"<umb-rte-block data-content-key=""5e499fc2-37be-4b1d-9746-70526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-key=""9db16c0d-251e-414c-8749-67090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>",
            result);
    }

    /// <summary>
    /// Exact reproducer for the markup in https://github.com/umbraco/Umbraco-CMS/issues/22979.
    /// </summary>
    [Test]
    public void ConvertBlockUdisToKeys_ThreeConsecutiveSiblingBlocks_ConvertsEachIndividually()
    {
        var input =
            @"<p>...</p>" +
            @"<umb-rte-block data-content-udi=""umb://element/5e499fc237be4b1d974670526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-udi=""umb://element/9db16c0d251e414c874967090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-udi=""umb://element/c2c956b94a0945f29a1a366ad488545b""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p> </p>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(
            @"<p>...</p>" +
            @"<umb-rte-block data-content-key=""5e499fc2-37be-4b1d-9746-70526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-key=""9db16c0d-251e-414c-8749-67090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-key=""c2c956b9-4a09-45f2-9a1a-366ad488545b""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p> </p>",
            result);
    }

    /// <summary>
    /// Blocks separated by intervening HTML must be converted individually.
    /// </summary>
    [Test]
    public void ConvertBlockUdisToKeys_SiblingBlocksSeparatedByParagraph_ConvertsEachIndividually()
    {
        var input =
            @"<umb-rte-block data-content-udi=""umb://element/5e499fc237be4b1d974670526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>between</p>" +
            @"<umb-rte-block data-content-udi=""umb://element/9db16c0d251e414c874967090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(
            @"<umb-rte-block data-content-key=""5e499fc2-37be-4b1d-9746-70526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>between</p>" +
            @"<umb-rte-block data-content-key=""9db16c0d-251e-414c-8749-67090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>",
            result);
    }

    /// <summary>
    /// A mix of isolated and consecutive sibling blocks must all be converted independently.
    /// </summary>
    [Test]
    public void ConvertBlockUdisToKeys_MixedLayoutWithConsecutiveAndSeparatedBlocks_ConvertsEachIndividually()
    {
        var input =
            @"<umb-rte-block data-content-udi=""umb://element/11111111111111111111111111111111""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>paragraph</p>" +
            @"<umb-rte-block data-content-udi=""umb://element/22222222222222222222222222222222""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-udi=""umb://element/33333333333333333333333333333333""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>another</p>" +
            @"<umb-rte-block data-content-udi=""umb://element/44444444444444444444444444444444""><!--Umbraco-Block--></umb-rte-block>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(
            @"<umb-rte-block data-content-key=""11111111-1111-1111-1111-111111111111""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>paragraph</p>" +
            @"<umb-rte-block data-content-key=""22222222-2222-2222-2222-222222222222""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-key=""33333333-3333-3333-3333-333333333333""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>another</p>" +
            @"<umb-rte-block data-content-key=""44444444-4444-4444-4444-444444444444""><!--Umbraco-Block--></umb-rte-block>",
            result);
    }

    /// <summary>
    /// When a block's UDI fails to parse, the block is dropped from the markup.
    /// </summary>
    [Test]
    public void ConvertBlockUdisToKeys_BlockWithMalformedUdi_IsDroppedFromOutput()
    {
        var input =
            @"<p>before</p>" +
            @"<umb-rte-block data-content-udi=""not-a-valid-udi""><!--Umbraco-Block--></umb-rte-block>" +
            @"<p>after</p>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(
            @"<p>before</p><p>after</p>",
            result);
    }

    [Test]
    public void ConvertBlockUdisToKeys_MarkupWithoutBlocks_ReturnsUnchanged()
    {
        var input = @"<p>Hello</p><p>World</p>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(input, result);
    }

    /// <summary>
    /// Markup already in the new key form (no UDI attribute) is returned unchanged.
    /// </summary>
    [Test]
    public void ConvertBlockUdisToKeys_MarkupAlreadyInNewKeyForm_ReturnsUnchanged()
    {
        var input =
            @"<umb-rte-block data-content-key=""5e499fc2-37be-4b1d-9746-70526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-key=""9db16c0d-251e-414c-8749-67090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>";

        var result = RteBlockHelper.ConvertBlockUdisToKeys(input);

        Assert.AreEqual(input, result);
    }

    /// <summary>
    /// The regex finds one match per block when several appear as direct siblings.
    /// </summary>
    [Test]
    public void BlockRegex_ThreeConsecutiveSiblingBlocks_ProducesOneMatchPerBlock()
    {
        var input =
            @"<umb-rte-block data-content-udi=""umb://element/5e499fc237be4b1d974670526f3b00b7""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-udi=""umb://element/9db16c0d251e414c874967090f2cf3cc""><!--Umbraco-Block--></umb-rte-block>" +
            @"<umb-rte-block data-content-udi=""umb://element/c2c956b94a0945f29a1a366ad488545b""><!--Umbraco-Block--></umb-rte-block>";

        Assert.AreEqual(3, RteBlockHelper.BlockRegex().Matches(input).Count);
    }
}
#pragma warning restore CS0618
