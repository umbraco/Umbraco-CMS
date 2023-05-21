// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

public class PropertyTypeDetail
{
    public string Alias { get; set; }

    public string Name { get; set; }

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public int DataTypeId { get; set; }
}
