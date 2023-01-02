// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Testing;

public interface ITestDatabaseConfiguration
{
    ConnectionStrings InitializeConfiguration();

    void Teardown(string key);

    string GetDbKey();
}
