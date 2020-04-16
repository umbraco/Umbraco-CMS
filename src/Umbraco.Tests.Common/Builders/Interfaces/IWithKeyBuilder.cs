using System;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithKeyBuilder
    {
        Guid? Key { get; set; }
    }
}
