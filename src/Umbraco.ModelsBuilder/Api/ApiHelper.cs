using System.Collections.Generic;
using System.Text;
using Umbraco.ModelsBuilder.Building;
using Umbraco.ModelsBuilder.Umbraco;

namespace Umbraco.ModelsBuilder.Api
{
    internal static class ApiHelper
    {
        public static Dictionary<string, string> GetModels(UmbracoServices umbracoServices, string modelsNamespace, IDictionary<string, string> files)
        {
            var typeModels = umbracoServices.GetAllTypes();

            var parseResult = new CodeParser().ParseWithReferencedAssemblies(files);
            var builder = new TextBuilder(typeModels, parseResult, modelsNamespace);

            var models = new Dictionary<string, string>();
            foreach (var typeModel in builder.GetModelsToGenerate())
            {
                var sb = new StringBuilder();
                builder.Generate(sb, typeModel);
                models[typeModel.ClrName] = sb.ToString();
            }
            return models;
        }
    }
}
