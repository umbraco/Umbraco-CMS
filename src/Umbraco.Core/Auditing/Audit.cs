using System;
using System.ComponentModel;

namespace Umbraco.Core.Auditing
{
    [Obsolete("Use Umbraco.Core.Services.IAuditService instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Audit
    {
        public static void Add(Umbraco.Core.Auditing.AuditTypes type, string comment, int userId, int objectId)
         {
             ApplicationContext.Current.Services.AuditService.Add(
                 Enum<Umbraco.Core.Models.AuditType>.Parse(type.ToString()), 
                 comment, userId, objectId);
         }
    }
}