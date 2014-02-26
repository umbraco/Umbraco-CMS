using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core.IO;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// An internal in-memory status tracker for the current installation
    /// </summary>
    internal static class InstallStatusTracker
    {

        private static ConcurrentDictionary<string, InstallTrackingItem> _steps = new ConcurrentDictionary<string, InstallTrackingItem>();

        private static string GetFile()
        {
            var file = IOHelper.MapPath("~/App_Data/TEMP/Install/"
                                        + "status"
                                        //+ dt.ToString("yyyyMMddHHmmss")
                                        + ".txt");
            return file;
        }

        public static void Reset()
        {
            _steps = new ConcurrentDictionary<string, InstallTrackingItem>();
            File.Delete(GetFile());
        }

        public static IDictionary<string, InstallTrackingItem> Initialize(IEnumerable<InstallSetupStep> steps)
        {
            //if there are no steps in memory
            if (_steps.Count == 0)
            {
                //check if we have our persisted file and read it
                var file = GetFile();
                if (File.Exists(file))
                {
                    var deserialized = JsonConvert.DeserializeObject<IDictionary<string, InstallTrackingItem>>(
                        File.ReadAllText(file));
                    foreach (var item in deserialized)
                    {
                        _steps.TryAdd(item.Key, item.Value);
                    }
                }
                else
                {
                    //otherwise just create the steps in memory (brand new install)
                    foreach (var step in steps)
                    {
                        _steps.TryAdd(step.Name, new InstallTrackingItem());                        
                    }
                    //save the file
                    var serialized = JsonConvert.SerializeObject(new Dictionary<string, InstallTrackingItem>(_steps));
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                    File.WriteAllText(file, serialized);
                }
            }

            return new Dictionary<string, InstallTrackingItem>(_steps);
        }

        public static void SetComplete(string name, IDictionary<string, object> additionalData = null)
        {
            var trackingItem = new InstallTrackingItem()
            {
                IsComplete = true
            };
            if (additionalData != null)
            {
                trackingItem.AdditionalData = additionalData;
            }

            _steps[name] = trackingItem;

            //save the file
            var file = GetFile();
            var serialized = JsonConvert.SerializeObject(new Dictionary<string, InstallTrackingItem>(_steps));
            File.WriteAllText(file, serialized);
        }

        public static IDictionary<string, InstallTrackingItem> GetStatus()
        {
            return new Dictionary<string, InstallTrackingItem>(_steps);
        } 
    }
}
