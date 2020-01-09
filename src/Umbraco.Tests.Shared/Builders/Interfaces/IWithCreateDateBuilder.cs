using System;

namespace Umbraco.Tests.Shared.Builders.Markers
{
    public interface IWithCreateDateBuilder
    {
        DateTime? CreateDate { get; set; }
    }
}
