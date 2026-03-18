using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

/// <summary>
/// Provides methods to compute hash values for type models, typically used to detect changes in model definitions.
/// </summary>
public class TypeModelHasher
{
    /// <summary>
    /// Generates a hash string representing the given collection of <see cref="Umbraco.Cms.Infrastructure.ModelsBuilder.Building.TypeModel"/> instances.
    /// The hash is computed based on important properties of the content type models and their properties,
    /// including the ModelsBuilder API version to ensure changes trigger model rebuilds.
    /// </summary>
    /// <param name="typeModels">The collection of <see cref="Umbraco.Cms.Infrastructure.ModelsBuilder.Building.TypeModel"/> to hash.</param>
    /// <returns>A string hash representing the combined state of the provided type models.</returns>
    public static string Hash(IEnumerable<TypeModel> typeModels)
    {
        var builder = new StringBuilder();

        // see Umbraco.ModelsBuilder.Umbraco.Application for what's important to hash
        // ie what comes from Umbraco (not computed by ModelsBuilder) and makes a difference
        foreach (TypeModel typeModel in typeModels.OrderBy(x => x.Alias))
        {
            builder.AppendLine("--- CONTENT TYPE MODEL ---");
            builder.AppendLine(typeModel.Id.ToString());
            builder.AppendLine(typeModel.Alias);
            builder.AppendLine(typeModel.ClrName);
            builder.AppendLine(typeModel.ParentId.ToString());
            builder.AppendLine(typeModel.Name);
            builder.AppendLine(typeModel.Description);
            builder.AppendLine(typeModel.ItemType.ToString());
            builder.AppendLine("MIXINS:" + string.Join(",", typeModel.MixinTypes.OrderBy(x => x.Id).Select(x => x.Id)));

            foreach (PropertyModel prop in typeModel.Properties.OrderBy(x => x.Alias))
            {
                builder.AppendLine("--- PROPERTY ---");
                builder.AppendLine(prop.Alias);
                builder.AppendLine(prop.ClrName);
                builder.AppendLine(prop.Name);
                builder.AppendLine(prop.Description);
                builder.AppendLine(prop.ModelClrType.ToString()); // see ModelType tests, want ToString() not FullName
            }
        }

        // Include the MB version in the hash so that if the MB version changes, models are rebuilt
        builder.AppendLine(ApiVersion.Current.Version.ToString());

        return builder.ToString().GenerateHash();
    }
}
