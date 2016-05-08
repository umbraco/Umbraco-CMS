using System.Diagnostics;
using UmbracoExamine;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.UmbracoExamine
{
    public class TestLogService : ILogService
    {
        #region ILogService Members

        public string ProviderName { get; set; }

        public void AddErrorLog(int nodeId, string msg)
        {
            Trace.WriteLine("ERROR: (" + nodeId + ") " + msg);
        }

        public void AddInfoLog(int nodeId, string msg)
        {
            Trace.WriteLine("INFO: (" + nodeId + ") " + msg);
        }

        public void AddVerboseLog(int nodeId, string msg)
        {
            Trace.WriteLine("VERBOSE: (" + nodeId + ") " + msg);
        }
        #endregion
    }
}