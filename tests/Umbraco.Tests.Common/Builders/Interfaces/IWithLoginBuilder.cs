// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces;

public interface IWithLoginBuilder
{
    string Username { get; set; }

    string RawPasswordValue { get; set; }

    string PasswordConfig { get; set; }
}
