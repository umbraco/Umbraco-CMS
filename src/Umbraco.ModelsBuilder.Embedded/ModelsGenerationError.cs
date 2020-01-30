using System;
using System.IO;
using System.Text;
using Umbraco.ModelsBuilder.Embedded.Configuration;

namespace Umbraco.ModelsBuilder.Embedded
{
    public sealed class ModelsGenerationError
    {
        private readonly IModelsBuilderConfig _config;

        public ModelsGenerationError(IModelsBuilderConfig config)
        {
            _config = config;
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
            var modelsDirectory = _config.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
                return null;

            return Path.Combine(modelsDirectory, "models.err");
        }
    }
}
