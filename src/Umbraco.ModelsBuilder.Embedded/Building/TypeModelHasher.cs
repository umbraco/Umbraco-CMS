using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Umbraco.ModelsBuilder.Embedded.Building
{
    internal class TypeModelHasher
    {
        public static string Hash(IEnumerable<TypeModel> typeModels)
        {
            var builder = new StringBuilder();

            // see Umbraco.ModelsBuilder.Umbraco.Application for what's important to hash
            // ie what comes from Umbraco (not computed by ModelsBuilder) and makes a difference

            foreach (var typeModel in typeModels.OrderBy(x => x.Alias))
            {
                builder.Append("--- CONTENT TYPE MODEL ---");
                builder.Append(typeModel.Id);
                builder.Append(typeModel.Alias);
                builder.Append(typeModel.ClrName);
                builder.Append(typeModel.ParentId);
                builder.Append(typeModel.Name);
                builder.Append(typeModel.Description);
                builder.Append(typeModel.ItemType.ToString());
                builder.Append("MIXINS:" + string.Join(",", typeModel.MixinTypes.OrderBy(x => x.Id).Select(x => x.Id)));

                foreach (var prop in typeModel.Properties.OrderBy(x => x.Alias))
                {
                    builder.Append("--- PROPERTY ---");
                    builder.Append(prop.Alias);
                    builder.Append(prop.ClrName);
                    builder.Append(prop.Name);
                    builder.Append(prop.Description);
                    builder.Append(prop.ModelClrType.ToString()); // see ModelType tests, want ToString() not FullName
                }
            }

            // Include the MB version in the hash so that if the MB version changes, models are rebuilt
            builder.Append(ApiVersion.Current.Version.ToString());

            return GenerateHash(builder.ToString());
        }

        private static string GenerateHash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder();
                foreach(var b in hashBytes)
                {
                    builder.Append(b.ToString("X"));
                }

                return builder.ToString();
            }
        }
    }
}
