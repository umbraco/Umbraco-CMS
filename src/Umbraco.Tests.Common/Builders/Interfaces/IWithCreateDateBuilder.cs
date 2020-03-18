using System;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithCreateDateBuilder
    {
        DateTime? CreateDate { get; set; }
    }
}
