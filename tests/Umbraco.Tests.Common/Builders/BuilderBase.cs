// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Tests.Common.Builders;

public abstract class BuilderBase<T>
{
    public abstract T Build();

    protected static string RandomAlias(string alias, bool randomizeAliases)
    {
        if (randomizeAliases)
        {
            return string.Concat(alias, Guid.NewGuid().ToString("N"));
        }

        return alias;
    }
}
