using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public static class ModelsBuilderConfigExtensions
    {
        public static string ModelsDirectoryAbsolute(this IModelsBuilderConfig modelsBuilderConfig, IIOHelper ioHelper)
        {
            return ioHelper.MapPath(modelsBuilderConfig.ModelsDirectory);
        }
    }
}
