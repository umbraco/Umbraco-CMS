using System;

namespace Umbraco.Tests.Shared.Builders.Interfaces
{
    public interface IWithDeleteDateBuilder
    {
        DateTime? DeleteDate { get; set; }
    }
}
