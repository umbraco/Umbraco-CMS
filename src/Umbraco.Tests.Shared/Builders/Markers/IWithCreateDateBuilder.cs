using System;

namespace Umbraco.Tests.Shared.Builders
{
    public interface IWithCreateDateBuilder
    {
        DateTime? CreateDate { get; set; }
    }
}
