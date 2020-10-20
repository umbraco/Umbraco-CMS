using System;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithSupportsPublishing
    {
        bool? SupportsPublishing { get; set; }
    }
}
