using System;

namespace Umbraco.Tests.Shared.Builders.Interfaces
{
    public interface IWithKeyBuilder
    {
        Guid? Key { get; set; }
    }
}
