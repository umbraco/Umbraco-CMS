// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Integration.Testing;

public interface ITestDatabase
{
    TestDatabaseInformation AttachEmpty();

    TestDatabaseInformation AttachSchema();

    void Detach(TestDatabaseInformation id);
}
