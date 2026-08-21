// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class ImagingMemorySettingsTests
{
    private const long OneMegabyte = 1024 * 1024;

    [Test]
    public void ResolveMaximumPoolSizeMegabytes_WhenConfigured_UsesConfiguredValue()
    {
        var settings = new ImagingMemorySettings { MaximumPoolSizeMegabytes = 256 };

        Assert.That(settings.ResolveMaximumPoolSizeMegabytes(512 * OneMegabyte), Is.EqualTo(256));
    }

    [TestCase(512, 16)] // Clamped to the minimum.
    [TestCase(2048, 64)]
    [TestCase(16384, 64)] // Clamped to the maximum.
    public void ResolveMaximumPoolSizeMegabytes_WhenNotConfigured_DerivesFromAvailableMemory(
        int availableMegabytes,
        int expected)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(settings.ResolveMaximumPoolSizeMegabytes(availableMegabytes * OneMegabyte), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveMaximumPoolSizeMegabytes_StaysWellBelowTheImageSharpDefault()
    {
        // ImageSharp defaults to an eighth of available memory, which is what leaves a container
        // sitting far above its working set at rest.
        const long available = 2048 * OneMegabyte;
        var imageSharpDefaultMegabytes = (int)(available / 8 / OneMegabyte);

        var resolved = new ImagingMemorySettings().ResolveMaximumPoolSizeMegabytes(available);

        Assert.That(resolved, Is.LessThan(imageSharpDefaultMegabytes));
    }

    [Test]
    public void ResolveMaximumConcurrentProcessing_WhenConfigured_UsesConfiguredValue()
    {
        var settings = new ImagingMemorySettings { MaximumConcurrentProcessing = 12 };

        Assert.That(settings.ResolveMaximumConcurrentProcessing(512 * OneMegabyte, 4), Is.EqualTo(12));
    }

    [TestCase(512, 32, 4)]
    [TestCase(1024, 32, 8)]
    [TestCase(2048, 32, 16)]
    public void ResolveMaximumConcurrentProcessing_WhenNotConfigured_DerivesFromAvailableMemory(
        int availableMegabytes,
        int processorCount,
        int expected)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(
            settings.ResolveMaximumConcurrentProcessing(availableMegabytes * OneMegabyte, processorCount),
            Is.EqualTo(expected));
    }

    [Test]
    public void ResolveMaximumConcurrentProcessing_IsCappedByProcessorCount()
    {
        var settings = new ImagingMemorySettings();

        Assert.That(settings.ResolveMaximumConcurrentProcessing(64L * 1024 * OneMegabyte, 4), Is.EqualTo(4));
    }

    [Test]
    public void ResolveMaximumConcurrentProcessing_NeverReturnsLessThanOne()
    {
        var settings = new ImagingMemorySettings();

        Assert.That(settings.ResolveMaximumConcurrentProcessing(16 * OneMegabyte, 1), Is.EqualTo(1));
    }
}
