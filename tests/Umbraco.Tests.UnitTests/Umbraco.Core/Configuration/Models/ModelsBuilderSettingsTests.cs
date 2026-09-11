// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class ModelsBuilderSettingsTests
{
    [Test]
    public void Defaults_To_A_Mode_That_Needs_No_Optional_Package()
    {
        // Generating models at runtime requires a factory that only an optional package supplies. Defaulting to
        // it left a site that expressed no preference asking for something nothing could satisfy; the package
        // now raises the mode instead when it is installed.
        var settings = new ModelsBuilderSettings();

        Assert.That(settings.ModelsMode, Is.EqualTo(Constants.ModelsBuilder.ModelsModes.Nothing));
    }
}
