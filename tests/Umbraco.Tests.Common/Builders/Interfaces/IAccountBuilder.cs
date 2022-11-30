// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces;

public interface IAccountBuilder : IWithLoginBuilder,
    IWithEmailBuilder,
    IWithFailedPasswordAttemptsBuilder,
    IWithIsApprovedBuilder,
    IWithIsLockedOutBuilder,
    IWithLastLoginDateBuilder,
    IWithLastPasswordChangeDateBuilder
{
}
