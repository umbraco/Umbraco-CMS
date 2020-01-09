using System;

namespace Umbraco.Tests.Shared.Builders.Interfaces
{
    public interface IWithCreateDateBuilder
    {
        DateTime? CreateDate { get; set; }
    }
}
