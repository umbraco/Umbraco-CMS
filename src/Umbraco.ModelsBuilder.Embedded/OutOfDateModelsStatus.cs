using System.IO;
using Umbraco.ModelsBuilder.Embedded.Configuration;
using Umbraco.Web.Cache;

namespace Umbraco.ModelsBuilder.Embedded
{
    public sealed class OutOfDateModelsStatus
    {
        private readonly IModelsBuilderConfig _config;

        public OutOfDateModelsStatus(IModelsBuilderConfig config)
        {
            _config = config;
        }

        internal void Install()
        {
            // just be sure
            if (_config.FlagOutOfDateModels == false)
                return;

            ContentTypeCacheRefresher.CacheUpdated += (sender, args) => Write();
            DataTypeCacheRefresher.CacheUpdated += (sender, args) => Write();
        }

        private string GetFlagPath()
        {
            var modelsDirectory = _config.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);
            return Path.Combine(modelsDirectory, "ood.flag");
        }

        private void Write()
        {
            var path = GetFlagPath();
            if (path == null || File.Exists(path)) return;
            File.WriteAllText(path, "THIS FILE INDICATES THAT MODELS ARE OUT-OF-DATE\n\n");
        }

        public void Clear()
        {
            if (_config.FlagOutOfDateModels == false) return;
            var path = GetFlagPath();
            if (path == null || !File.Exists(path)) return;
            File.Delete(path);
        }

        public bool IsEnabled => _config.FlagOutOfDateModels;

        public bool IsOutOfDate
        {
            get
            {
                if (_config.FlagOutOfDateModels == false) return false;
                var path = GetFlagPath();
                return path != null && File.Exists(path);
            }
        }
    }
}
