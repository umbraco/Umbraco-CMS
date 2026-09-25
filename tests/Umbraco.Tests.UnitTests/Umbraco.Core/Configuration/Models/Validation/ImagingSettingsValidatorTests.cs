// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class ImagingSettingsValidatorTests
{
    [Test]
    public void Can_Validate_Default_Configuration()
    {
        ValidateOptionsResult result = Validate(new ImagingMemorySettings());

        Assert.True(result.Succeeded);
    }

    [Test]
    public void Can_Validate_Zero_Values()
    {
        // Zero is the documented way of asking for a derived value, so it must stay valid.
        ValidateOptionsResult result = Validate(new ImagingMemorySettings
        {
            MaximumPoolSizeMegabytes = 0,
            MaximumConcurrentProcessing = 0,
            MaximumDecodedImageMegabytes = 0,
        });

        Assert.True(result.Succeeded);
    }

    [Test]
    public void Can_Validate_Explicit_Values()
    {
        ValidateOptionsResult result = Validate(new ImagingMemorySettings
        {
            MaximumPoolSizeMegabytes = 64,
            MaximumConcurrentProcessing = 4,
            MaximumDecodedImageMegabytes = 512,
        });

        Assert.True(result.Succeeded);
    }

    [Test]
    public void Cannot_Validate_Negative_Maximum_Pool_Size()
    {
        ValidateOptionsResult result = Validate(new ImagingMemorySettings { MaximumPoolSizeMegabytes = -1 });

        AssertFailureNames(result, nameof(ImagingMemorySettings.MaximumPoolSizeMegabytes));
    }

    [Test]
    public void Cannot_Validate_Negative_Maximum_Concurrent_Processing()
    {
        ValidateOptionsResult result = Validate(new ImagingMemorySettings { MaximumConcurrentProcessing = -1 });

        AssertFailureNames(result, nameof(ImagingMemorySettings.MaximumConcurrentProcessing));
    }

    [Test]
    public void Cannot_Validate_Negative_Maximum_Decoded_Image_Size()
    {
        ValidateOptionsResult result = Validate(new ImagingMemorySettings { MaximumDecodedImageMegabytes = -1 });

        AssertFailureNames(result, nameof(ImagingMemorySettings.MaximumDecodedImageMegabytes));
    }

    [Test]
    public void Cannot_Validate_Implausible_Maximum_Pool_Size()
    {
        // 512 MB expressed in bytes: the mistake the ceiling exists to catch, which would otherwise
        // fail the boot with an out of memory error naming nothing useful.
        ValidateOptionsResult result = Validate(new ImagingMemorySettings { MaximumPoolSizeMegabytes = 536870912 });

        AssertFailureNames(result, nameof(ImagingMemorySettings.MaximumPoolSizeMegabytes));
    }

    [Test]
    public void Can_Validate_Maximum_Pool_Size_At_The_Ceiling()
    {
        ValidateOptionsResult result = Validate(new ImagingMemorySettings
        {
            MaximumPoolSizeMegabytes = ImagingMemorySettings.MaximumConfigurablePoolSizeMegabytes,
        });

        Assert.True(result.Succeeded);
    }

    [Test]
    public void Can_Validate_Large_Maximum_Concurrent_Processing()
    {
        // Deliberately unbounded: a large value costs nothing to set up and is a roundabout way of
        // asking for no throttle, so it is not treated as a mistake.
        ValidateOptionsResult result = Validate(new ImagingMemorySettings { MaximumConcurrentProcessing = int.MaxValue });

        Assert.True(result.Succeeded);
    }

    private static void AssertFailureNames(ValidateOptionsResult result, string setting)
    {
        // The message is the whole point of the validator, so it has to identify the setting at
        // fault. Its wording is not asserted.
        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.FailureMessage, Does.Contain(setting));
            Assert.That(result.FailureMessage, Does.Contain(Constants.Configuration.ConfigImaging));
        });
    }

    private static ValidateOptionsResult Validate(ImagingMemorySettings memory)
        => new ImagingSettingsValidator().Validate(null, new ImagingSettings { Memory = memory });
}
