using System;

namespace Umbraco.Tests.Shared.Builders.Interfaces
{
    public interface IWithUpdateDateBuilder
    {
        DateTime? UpdateDate { get; set; }
    }
}
