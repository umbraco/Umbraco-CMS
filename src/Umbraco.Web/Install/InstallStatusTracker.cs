using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// An internal in-memory status tracker for the current installation
    /// </summary>
    internal static class InstallStatusTracker
    {

        private static ConcurrentDictionary<string, bool> _steps = new ConcurrentDictionary<string, bool>();

        public static void Initialize(IEnumerable<InstallStep> steps)
        {
            foreach (var step in steps)
            {
                _steps.TryAdd(step.Name, step.IsComplete);
            }
        }

        public static void SetComplete(string name)
        {
            _steps.TryUpdate(name, true, true);
        }

        public static IDictionary<string, bool> GetStatus()
        {
            return new Dictionary<string, bool>(_steps);
        } 
    }
}
