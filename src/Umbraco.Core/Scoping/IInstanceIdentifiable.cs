using System;

namespace Umbraco.Core.Scoping
{
    public interface IInstanceIdentifiable
    {
        Guid InstanceId { get; }
    }
}
