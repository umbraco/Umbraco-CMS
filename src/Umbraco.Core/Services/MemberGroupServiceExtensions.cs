using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Membergroup service extension methods
    /// </summary>
    /// <remarks>
    /// Many of these have to do with UDI lookups but we don't need to add these methods to the service interface since a UDI is just a GUID
    /// and the services already support GUIDs
    /// </remarks>
    public static class MemberGroupServiceExtensions
    {
        public static IEnumerable<IMemberGroup> GetByIds(this IMemberGroupService memberGroupService, IEnumerable<Udi> ids)
        {
            var guids = new List<GuidUdi>();
            foreach (var udi in ids)
            {
                var guidUdi = udi as GuidUdi;
                if (guidUdi == null)
                    throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by member group");
                guids.Add(guidUdi);
            }

            return memberGroupService.GetByIds(guids.Select(x => x.Guid));
        }
    }
}
