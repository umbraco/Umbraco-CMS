using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;

namespace Umbraco.ModelsBuilder.Embedded
{
    public sealed class ModelsGenerationError
    {
        private readonly ModelsBuilderConfig _config;
        private readonly IIOHelper _ioHelper;

        public ModelsGenerationError(IOptionsSnapshot<ModelsBuilderConfig> config, IIOHelper ioHelper)
        {
            _config = config.Value;
            _ioHelper = ioHelper;
        }

        public void Clear()
        {
            var errFile = GetErrFile();
            if (errFile == null) return;

            // "If the file to be deleted does not exist, no exception is thrown."
            File.Delete(errFile);
        }

        public void Report(string message, Exception e)
        {
            var errFile = GetErrFile();
            if (errFile == null) return;

            var sb = new StringBuilder();
            sb.Append(message);
            sb.Append("\r\n");
            sb.Append(e.Message);
            sb.Append("\r\n\r\n");
            sb.Append(e.StackTrace);
            sb.Append("\r\n");

            File.WriteAllText(errFile, sb.ToString());
        }

        public string GetLastError()
        {
            var errFile = GetErrFile();
            if (errFile == null) return null;

            try
            {
                return File.ReadAllText(errFile);
            }
            catch // accepted
            {
                return null;
            }
        }

        private string GetErrFile()
        {
            var modelsDirectory = _config.ModelsDirectoryAbsolute(_ioHelper);
            if (!Directory.Exists(modelsDirectory))
                return null;

            return Path.Combine(modelsDirectory, "models.err");
        }
    }
}
