using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IUserType : IAggregateRoot
    {
        string Alias { get; set; }
        string Name { get; set; }        
        IEnumerable<string> Permissions { get; set; }

    }
}