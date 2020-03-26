using System;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithUpdateDateBuilder
    {
        DateTime? UpdateDate { get; set; }
    }
}
