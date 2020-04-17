using System;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithLastLoginDateBuilder
    {
        DateTime? LastLoginDate { get; set; }
    }
}
