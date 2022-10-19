// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.Common;

public class TestDefaultCultureAccessor : IDefaultCultureAccessor
{
    private string _defaultCulture = string.Empty;

    public string DefaultCulture
    {
        get => _defaultCulture;
        set => _defaultCulture = value ?? string.Empty;
    }
}
