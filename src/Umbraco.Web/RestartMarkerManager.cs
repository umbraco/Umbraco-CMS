using System;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web
{
    internal class RestartMarkerManager
    {
        /// <summary>
        /// Check if a restart marker exists, if so it means the appdomain is still restarting
        /// </summary>
        /// <returns></returns>
        internal static bool RestartMarkerExists()
        {
            var dir = new DirectoryInfo(IOHelper.MapPath("~/App_Data/TEMP/Install"));
            if (dir.Exists == false) return false;
            return dir.GetFiles("restart_*.txt").Length > 0;
        }

        /// <summary>
        /// Used during package installation in the website in order to determine if the site is marked for restarting so subsequent requests know if restart has been successful
        /// </summary>
        internal static Guid? CreateRestartMarker()
        {
            try
            {
                //we need to store a restart marker, this is because even though files are installed and the app domain should
                //restart, in may not be immediate because we might have waitChangeNotification and maxWaitChangeNotification
                //configured. In which case the response and the next request to 'InstallData' will occur on the same app domain.
                var restartId = Guid.NewGuid();
                Directory.CreateDirectory(IOHelper.MapPath("~/App_Data/TEMP/Install"));
                File.CreateText(IOHelper.MapPath("~/App_Data/TEMP/Install/" + "restart_" + restartId + "_" + AppDomain.CurrentDomain.Id + ".txt")).Close();
                return restartId;
            }
            catch (Exception ex)
            {
                //swallow! we don't want to prevent the app from shutting down which is generally when this is called
                LogHelper.Error<WebBootManager>("Could not create restart markers", ex);
                return null;
            }
        }



        /// <summary>
        /// Used during package installation in the website in order to determine if the site is marked for restarting so subsequent requests know if restart has been successful
        /// </summary>
        /// <remarks>
        /// The restart markers are cleared when the app is booted up
        /// </remarks>
        internal static void ClearRestartMarkers()
        {
            try
            {
                var dir = new DirectoryInfo(IOHelper.MapPath("~/App_Data/TEMP/Install"));
                if (dir.Exists == false) return;
                foreach (var f in dir.GetFiles("restart_*.txt"))
                {
                    f.Delete();
                }
            }
            catch (Exception ex)
            {
                //swallow! we don't want to prevent the app from starting
                LogHelper.Error<WebBootManager>("Could not clear restart markers", ex);
            }
        }
    }
}