// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Common.Testing;

public class TestHostEnvironment : IHostEnvironment
{
    public string ApplicationName { get; set; }

    public IFileProvider ContentRootFileProvider { get; set; }

    public string ContentRootPath { get; set; }

    public string EnvironmentName { get; set; }
}
