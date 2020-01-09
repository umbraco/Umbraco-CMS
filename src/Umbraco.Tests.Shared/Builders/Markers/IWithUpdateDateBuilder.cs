using System;

namespace Umbraco.Tests.Shared.Builders
{
    public interface IWithUpdateDateBuilder
    {
        DateTime? UpdateDate { get; set; }
    }
}
