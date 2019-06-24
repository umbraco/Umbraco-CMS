using System.IO;
using Umbraco.Core.Composing;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.Web.Cache;

namespace Umbraco.ModelsBuilder.Umbraco
{
    public sealed class OutOfDateModelsStatus
    {
        private static Config Config => Current.Configs.ModelsBuilder();
        
        internal static void Install()
        {
            // just be sure
            if (Config.FlagOutOfDateModels == false)
                return;

            ContentTypeCacheRefresher.CacheUpdated += (sender, args) => Write();
            DataTypeCacheRefresher.CacheUpdated += (sender, args) => Write();
        }

        private static string GetFlagPath()
        {
            var modelsDirectory = Config.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);
            return Path.Combine(modelsDirectory, "ood.flag");
        }

        private static void Write()
        {
            var path = GetFlagPath();
            if (path == null || File.Exists(path)) return;
            File.WriteAllText(path, "THIS FILE INDICATES THAT MODELS ARE OUT-OF-DATE\n\n");
        }

        public static void Clear()
        {
            if (Config.FlagOutOfDateModels == false) return;
            var path = GetFlagPath();
            if (path == null || !File.Exists(path)) return;
            File.Delete(path);
        }

        public static bool IsEnabled => Config.FlagOutOfDateModels;

        public static bool IsOutOfDate
        {
            get
            {
                if (Config.FlagOutOfDateModels == false) return false;
                var path = GetFlagPath();
                return path != null && File.Exists(path);
            }
        }
    }
}
