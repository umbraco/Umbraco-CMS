using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Web.Services.Protocols;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;
using umbraco.BusinessLogic;
using umbraco.interfaces;

namespace umbraco.presentation.cache
{
    [Obsolete("This class is no longer used, use DistrubutedCache.Instance instead")]
    public class dispatcher
    {                
        public static void Refresh(Guid factoryGuid, int id)
        {
            DistrubutedCache.Instance.Refresh(factoryGuid, id);            
        }
        
        public static void Refresh(Guid factoryGuid, Guid id)
        {
            DistrubutedCache.Instance.Refresh(factoryGuid, id);      
        }
        
        public static void RefreshAll(Guid factoryGuid)
        {
            DistrubutedCache.Instance.RefreshAll(factoryGuid);
        }
        
        public static void Remove(Guid factoryGuid, int id)
        {
            DistrubutedCache.Instance.Remove(factoryGuid, id);
        }
    }
}