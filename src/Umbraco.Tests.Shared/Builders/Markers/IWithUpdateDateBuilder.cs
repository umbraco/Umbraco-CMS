using System;

namespace Umbraco.Tests.Shared.Builders.Markers
{
    public interface IWithUpdateDateBuilder
    {
        DateTime? UpdateDate { get; set; }
    }
}
