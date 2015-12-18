using System;
using System.IO;

namespace Umbraco.Core.Services
{
    public class LocalizedTextServiceSupplementaryFileSource
    {
        
        public LocalizedTextServiceSupplementaryFileSource(FileInfo file, bool overwriteCoreKeys)
        {
            if (file == null) throw new ArgumentNullException("file");

            File = file;
            OverwriteCoreKeys = overwriteCoreKeys;
        }

        public FileInfo File { get; private set; }
        public bool OverwriteCoreKeys { get; private set; }
    }
}