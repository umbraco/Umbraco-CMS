using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Api.Management.Mapping.Member;

/// <summary>
/// Provides mapping configuration for members within the Umbraco CMS Management API.
/// </summary>
public class MemberMapDefinition : ContentMapDefinition<IMember, MemberValueResponseModel, MemberVariantResponseModel>, IMapDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Mapping.Member.MemberMapDefinition"/> class.
    /// </summary>
    /// <param name="propertyEditorCollection">A <see cref="PropertyEditorCollection"/> containing the available property editors.</param>
    /// <param name="dataValueEditorFactory">An <see cref="IDataValueEditorFactory"/> used to create data value editors.</param>
    public MemberMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory)
        : base(propertyEditorCollection, dataValueEditorFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Mapping.Member.MemberMapDefinition"/> class with the specified property editor collection.
    /// </summary>
    /// <param name="propertyEditorCollection">
    /// The <see cref="PropertyEditorCollection"/> representing the collection of property editors to be used.
    /// </param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 18.")]
    public MemberMapDefinition(
        PropertyEditorCollection propertyEditorCollection)
        : this(
            propertyEditorCollection,
            StaticServiceProvider.Instance.GetRequiredService<IDataValueEditorFactory>())
    {
    }

    /// <summary>
    /// Configures the mapping between <see cref="IMember"/> entities and <see cref="MemberResponseModel"/> response models.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mapping configuration.</param>
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMember, MemberResponseModel>((_, _) => new MemberResponseModel(), Map);

    // Umbraco.Code.MapAll -IsTwoFactorEnabled -Groups -Kind -Flags
    private void Map(IMember source, MemberResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MemberType = context.Map<MemberTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties);
        target.Variants = MapVariantViewModels(source);

        target.IsApproved = source.IsApproved;
        target.IsLockedOut = source.IsLockedOut;
        target.Email = source.Email;
        target.Username = source.Username;
        target.FailedPasswordAttempts = source.FailedPasswordAttempts;
        target.LastLoginDate = source.LastLoginDate;
        target.LastLockoutDate = source.LastLockoutDate;
        target.LastPasswordChangeDate = source.LastPasswordChangeDate;
    }
}
