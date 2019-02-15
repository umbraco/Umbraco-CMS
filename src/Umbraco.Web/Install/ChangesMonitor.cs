using System;
using System.Reflection;
using System.Web;

namespace Umbraco.Web.Install
{
    public class ChangesMonitor : IDisposable
    {
        private readonly object _fileMonitor;
        private readonly FieldInfo _disposedField;
        private readonly bool _wasDisposed;
        private bool _acquired;
        private object _lockDispose;


        //Ref. Option B http://beweb.pbworks.com/w/page/30073098/Prevent%20app%20restarts%20and%20site%20downtime%20when%20deploying%20files
        //Ref. http://stackoverflow.com/questions/613824/how-to-prevent-an-asp-net-application-restarting-when-the-web-config-is-modified/629876#629876
        //
        // HttpRuntime.FilesChangesMonitor is a FileChangesMonitor instance
        // which has inner DirectoryMonitor instances to monitor directories
        // FileChangesMonitor has a Stop method which stops monitoring everything - and release all native resources
        // but... then we cannot really re-enable monitoring
        //
        // OTOH if FileChangesMonitor believes it's been disposed, ie its _disposed field is true,
        // its OnSubdirChange and OnCriticaldirChange methods return without doing anything
        // so... we can flip _disposed to true while doing our critical changes
        //
        // potential issues = race conditions,
        // - event triggering too late, after we have re-enabled monitoring
        //   -> ?
        // - monitor being disposed for real while we pretend it's been disposed
        //   -> handled, by locking the file monitor's inner lock
        // - while _disposed is true, will not start monitoring everything either
        //   -> assuming that ... we're using this at a time where no multi-thread thing would run

        /// <summary>
        /// Gets a disposable object representing suspended change monitoring.
        /// </summary>
        /// <remarks>
        /// <para>Dispose the object to re-enable change monitoring.</para>
        /// </remarks>
        public static IDisposable Suspended() => new ChangesMonitor();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangesMonitor"/> class.
        /// </summary>
        private ChangesMonitor()
        {
            // get the FileChangesMonitor property
            var fileMonitorProperty = typeof(HttpRuntime).GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (fileMonitorProperty == null) throw new Exception("Could not get HttpRuntime.FileChangesMonitor property.");

            // get its value, ie the file monitor, a FileChangesMonitor instance
            _fileMonitor = fileMonitorProperty.GetValue(null);
            if (_fileMonitor == null) throw new Exception("Could not get the file monitor.");

            // no! this stops monitoring everything and monitoring cannot be re-enabled
            /*
            // get the Stop method
            var fileMonitorStopMethod = fileMonitor.GetType().GetMethod("Stop", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fileMonitorStopMethod == null) throw new Exception("Could not get the file monitor Stop method.");

            // stop
            fileMonitorStopMethod.Invoke(fileMonitor, new object[] { });
            */

            // get the _disposed field
            _disposedField = _fileMonitor.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_disposedField == null) throw new Exception("Could not get the file monitor _disposed field.");

            // pretend to be disposed, if not already disposed
            try
            {
                AcquireDisposeLock(_fileMonitor);

                _wasDisposed = (bool)_disposedField.GetValue(_fileMonitor);
                if (!_wasDisposed)
                    _disposedField.SetValue(_fileMonitor, true);
            }
            finally
            {
                ReleaseDisposeLock();
            }
        }

        public void Dispose()
        {
            // restore
            if (_disposedField == null || _wasDisposed)
                return;

            // if the FileChangesMonitor is stopped while we have _disposed being true...
            // it will *still* do all the disposing work, but we must detect it and *not*
            // flip the flag back to false!
            //
            // the FileChangesMonitor locks before flipping _disposed to true and releasing
            // anything, so by locking too we should be safe

            try
            {
                AcquireDisposeLock(_fileMonitor);

                var dirMonSubdirsField = _fileMonitor.GetType().GetField("_dirMonSubdirs", BindingFlags.Instance | BindingFlags.NonPublic);
                if (dirMonSubdirsField == null) throw new Exception("Could not get the file monitor _dirMonSubdirs field.");

                var dirMonSpecialDirsField = _fileMonitor.GetType().GetField("_dirMonSpecialDirs", BindingFlags.Instance | BindingFlags.NonPublic);
                if (dirMonSpecialDirsField == null) throw new Exception("Could not get the file monitor _dirMonSpecialDirs field.");

                var dirMonSubdirsIsNull = dirMonSubdirsField.GetValue(_fileMonitor) == null;
                var dirMonSpecialDirsIsNull = dirMonSpecialDirsField.GetValue(_fileMonitor) == null;

                // let's say... this works
                var hasBeenDisposed = dirMonSubdirsIsNull && dirMonSpecialDirsIsNull;

                // if it has been disposed, leave _disposed true, else reset the value
                if (!hasBeenDisposed)
                    _disposedField.SetValue(_fileMonitor, false);
            }
            finally
            {
                ReleaseDisposeLock();
            }
        }

        private void AcquireDisposeLock(object fileMonitor)
        {
            if (_lockDispose == null)
            {
                var lockDisposeField = fileMonitor.GetType().GetField("_lockDispose", BindingFlags.Instance | BindingFlags.NonPublic);
                if (lockDisposeField == null) throw new Exception("Could not get the file monitor _lockDispose field.");

                _lockDispose = lockDisposeField.GetValue(fileMonitor);
                if (_lockDispose == null) throw new Exception("File monitor _lockDispose is null.");
            }

            var aquireMethod = _lockDispose.GetType().GetMethod("AcquireWriterLock", BindingFlags.Instance | BindingFlags.NonPublic);
            if (aquireMethod == null) throw new Exception("Could not get the dispose lock AcquireWriterLock method.");

            aquireMethod.Invoke(_lockDispose, Array.Empty<object>());
            _acquired = true;
        }

        private void ReleaseDisposeLock()
        {
            if (!_acquired) throw new Exception("Lock has not been acquired.");
            if (_lockDispose == null) throw new Exception("File monitor _lockDispose is null.");

            var releaseMethod = _lockDispose.GetType().GetMethod("ReleaseWriterLock", BindingFlags.Instance | BindingFlags.NonPublic);
            if (releaseMethod == null) throw new Exception("Could not get the dispose lock ReleaseWriterLock method.");

            releaseMethod.Invoke(_lockDispose, Array.Empty<object>());
            _acquired = false;
        }
    }
}
