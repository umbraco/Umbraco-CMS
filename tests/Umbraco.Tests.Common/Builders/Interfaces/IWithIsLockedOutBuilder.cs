// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces;

public interface IWithIsLockedOutBuilder
{
    bool? IsLockedOut { get; set; }

    DateTime? LastLockoutDate { get; set; }
}
