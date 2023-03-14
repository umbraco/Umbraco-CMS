namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents an entity that can be managed by the entity service.
/// </summary>
/// <remarks>
///     <para>An IUmbracoEntity can be related to another via the IRelationService.</para>
///     <para>IUmbracoEntities can be retrieved with the IEntityService.</para>
///     <para>An IUmbracoEntity can participate in notifications.</para>
/// </remarks>
public interface IUmbracoEntity : ITreeEntity
{
}
