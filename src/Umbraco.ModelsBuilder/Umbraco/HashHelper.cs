using System.Collections.Generic;
using System.Linq;
using Umbraco.ModelsBuilder.Building;

namespace Umbraco.ModelsBuilder.Umbraco
{
    class HashHelper
    {
        public static string Hash(IDictionary<string, string> ourFiles, IEnumerable<TypeModel> typeModels)
        {
            var hash = new HashCombiner();

            foreach (var kvp in ourFiles)
                hash.Add(kvp.Key + "::" + kvp.Value);

            // see Umbraco.ModelsBuilder.Umbraco.Application for what's important to hash
            // ie what comes from Umbraco (not computed by ModelsBuilder) and makes a difference

            foreach (var typeModel in typeModels.OrderBy(x => x.Alias))
            {
                hash.Add("--- CONTENT TYPE MODEL ---");
                hash.Add(typeModel.Id);
                hash.Add(typeModel.Alias);
                hash.Add(typeModel.ClrName);
                hash.Add(typeModel.ParentId);
                hash.Add(typeModel.Name);
                hash.Add(typeModel.Description);
                hash.Add(typeModel.ItemType.ToString());
                hash.Add("MIXINS:" + string.Join(",", typeModel.MixinTypes.OrderBy(x => x.Id).Select(x => x.Id)));

                foreach (var prop in typeModel.Properties.OrderBy(x => x.Alias))
                {
                    hash.Add("--- PROPERTY ---");
                    hash.Add(prop.Alias);
                    hash.Add(prop.ClrName);
                    hash.Add(prop.Name);
                    hash.Add(prop.Description);
                    hash.Add(prop.ModelClrType.ToString()); // see ModelType tests, want ToString() not FullName
                }
            }

            return hash.GetCombinedHashCode();
        }
    }
}
