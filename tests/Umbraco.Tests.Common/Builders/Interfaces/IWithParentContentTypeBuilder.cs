// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces;

public interface IWithParentContentTypeBuilder
{
    IContentTypeComposition Parent { get; set; }
}
