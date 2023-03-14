// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces;

public interface IWithCultureInfoBuilder
{
    CultureInfo CultureInfo { get; set; }
}
