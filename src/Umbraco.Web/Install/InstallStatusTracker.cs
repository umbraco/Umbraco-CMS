using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Collections;
using Umbraco.Core.IO;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// An internal in-memory status tracker for the current installation
    /// </summary>
    internal static class InstallStatusTracker
    {
        private static readonly object _locker = new object();

        private static Dictionary<string, InstallTrackingItem> _steps = new Dictionary<string, InstallTrackingItem>();

        private static string GetFile(Guid installId)
        {
            var file = IOHelper.MapPath(SystemDirectories.TempData.EnsureEndsWith('/') + "Install/"
                                        + "install_"
                                        + installId.ToString("N")
                                        + ".txt");
            return file;
        }

        public static void Reset()
        {
            lock (_locker)
            {
                _steps.Clear();
                ClearFiles();
            }
        }

        public static void ClearFiles()
        {
            lock (_locker)
            {
                var dir = IOHelper.MapPath(SystemDirectories.TempData.EnsureEndsWith('/') + "Install/");
                if (Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir);
                    foreach (var f in files)
                    {
                        File.Delete(f);
                    }
                }
                else
                {
                    Directory.CreateDirectory(dir);
                }
            }            
        }

        private static IReadOnlyList<InstallTrackingItem> InitializeFromFile(Guid installId)
        {
            lock (_locker)
            {
                //check if we have our persisted file and read it
                var file = GetFile(installId);
                if (File.Exists(file))
                {
                    var deserialized = JsonConvert.DeserializeObject<IEnumerable<InstallTrackingItem>>(
                        File.ReadAllText(file));

                    _steps = deserialized.ToDictionary(x => x.Name);
                }
                else
                {
                    throw new InvalidOperationException("Cannot initialize from file, the installation file with id " + installId + " does not exist");
                }

                return new List<InstallTrackingItem>(_steps.Values.OrderBy(x => x.ServerOrder));
            }   
        }

        public static IReadOnlyList<InstallTrackingItem> Initialize(Guid installId, IEnumerable<InstallSetupStep> steps)
        {
            lock (_locker)
            {
                //if there are no steps in memory
                if (_steps.Count == 0)
                {
                    //check if we have our persisted file and read it
                    var file = GetFile(installId);
                    if (File.Exists(file))
                    {
                        var deserialized = JsonConvert.DeserializeObject<IEnumerable<InstallTrackingItem>>(
                            File.ReadAllText(file));

                        _steps = deserialized.ToDictionary(x => x.Name);
                    }
                    else
                    {
                        ClearFiles();

                        //otherwise just create the steps in memory (brand new install)
                        _steps = steps.ToDictionary(step => step.Name, step => new InstallTrackingItem(step.Name, step.ServerOrder));

                        //save the file
                        var serialized = JsonConvert.SerializeObject(_steps.Values);
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        File.WriteAllText(file, serialized);
                    }
                }
                else
                {
                    //ensure that the file exists with the current install id
                    var file = GetFile(installId);
                    if (File.Exists(file) == false)
                    {
                        ClearFiles();

                        //save the correct file
                        var serialized = JsonConvert.SerializeObject(_steps.Values);
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        File.WriteAllText(file, serialized);
                    }
                }

                return new List<InstallTrackingItem>(_steps.Values.OrderBy(x => x.ServerOrder));
            }  
        }

        public static void SetComplete(Guid installId, string name, IDictionary<string, object> additionalData = null)
        {
            lock (_locker)
            {
                var trackingItem = _steps[name];
                if (additionalData != null)
                {
                    trackingItem.AdditionalData = additionalData;
                }
                trackingItem.IsComplete = true;

                //save the file
                var file = GetFile(installId);
                var serialized = JsonConvert.SerializeObject(new List<InstallTrackingItem>(_steps.Values));
                File.WriteAllText(file, serialized);
            }
        }

        public static IReadOnlyList<InstallTrackingItem> GetOrderedStatus(Guid installId)
        {
            lock (_locker)
            {
                IReadOnlyList<InstallTrackingItem> items = new List<InstallTrackingItem>(_steps.Values.OrderBy(x => x.ServerOrder));
                //there won't be any statuses returned if the app pool has restarted so we need to re-read from file.
                if (_steps.Count == 0)
                {
                    items = InitializeFromFile(installId);
                }
                return items;
            }   
        }

        public static InstallTrackingItem GetRequiredStep(string stepName)
        {
            lock (_locker)
            {
                if (!_steps.TryGetValue(stepName, out var step))
                {
                    throw new InvalidOperationException("No step found with name " + stepName);
                }
                return step;
            }
        }

        public static InstallTrackingItem GetStep(string stepName)
        {
            lock (_locker)
            {
                if (!_steps.TryGetValue(stepName, out var step))
                {
                    return null;
                }
                return step;
            }
        }
    }
}
