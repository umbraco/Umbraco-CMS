// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FileServiceTests : UmbracoIntegrationTest
{

    [SetUp]
    public void SetUp()
    {
        var fileSystems = GetRequiredService<FileSystems>();
        var viewFileSystem = fileSystems.MvcViewsFileSystem!;
        foreach (var file in viewFileSystem.GetFiles(string.Empty).ToArray())
        {
            viewFileSystem.DeleteFile(file);
        }
    }
}
