// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Core.Models;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithParentContentTypeBuilder
    {
        IContentTypeComposition Parent { get; set; }
    }
}
