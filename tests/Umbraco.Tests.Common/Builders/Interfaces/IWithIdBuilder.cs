// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces;

public interface IWithIdBuilder
{
    int? Id { get; set; }
}

public interface IWithIdBuilder<TId>
{
    TId Id { get; set; }
}
