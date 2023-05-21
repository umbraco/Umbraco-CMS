// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Integration.Testing;

public interface ITestDatabase
{
    TestDbMeta AttachEmpty();

    TestDbMeta AttachSchema();

    void Detach(TestDbMeta id);
}
