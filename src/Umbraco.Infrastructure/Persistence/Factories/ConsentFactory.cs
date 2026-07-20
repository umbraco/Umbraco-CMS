using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class ConsentFactory
{
    /// <summary>
    /// Builds a collection of <see cref="IConsent"/> entities from the given collection of <see cref="ConsentDto"/> data transfer objects.
    /// Consents with the same source, context, and action are grouped together, with additional consents added to the <c>HistoryInternal</c> property of the first entity in each group.
    /// </summary>
    /// <param name="dtos">The collection of <see cref="ConsentDto"/> objects to convert.</param>
    /// <returns>
    /// A collection of <see cref="IConsent"/> entities, each representing the latest consent for a unique combination of source, context, and action, with their history populated as applicable.
    /// </returns>
    public static IEnumerable<IConsent> BuildEntities(IEnumerable<ConsentDto> dtos)
    {
        var ix = new Dictionary<string, Consent>();
        var output = new List<Consent>();

        foreach (ConsentDto dto in dtos)
        {
            var k = dto.Source + "::" + dto.Context + "::" + dto.Action;

            var consent = new Consent
            {
                Id = dto.Id,
                Current = dto.Current,
                CreateDate = dto.CreateDate.EnsureUtc(),
                Source = dto.Source,
                Context = dto.Context,
                Action = dto.Action,
                State = (ConsentState)dto.State, // assume value is valid
                Comment = dto.Comment,
            };

            // on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            consent.ResetDirtyProperties(false);

            if (ix.TryGetValue(k, out Consent? current))
            {
                if (current.HistoryInternal == null)
                {
                    current.HistoryInternal = new List<IConsent>();
                }

                current.HistoryInternal.Add(consent);
            }
            else
            {
                ix[k] = consent;
                output.Add(consent);
            }
        }

        return output;
    }

    /// <summary>
    /// Creates a <see cref="ConsentDto"/> from the specified <see cref="IConsent"/> entity.
    /// </summary>
    /// <param name="entity">The <see cref="IConsent"/> entity to convert.</param>
    /// <returns>A <see cref="ConsentDto"/> representing the provided consent entity.</returns>
    public static ConsentDto BuildDto(IConsent entity) =>
        new ConsentDto
        {
            Id = entity.Id,
            Current = entity.Current,
            CreateDate = entity.CreateDate,
            Source = entity.Source,
            Context = entity.Context,
            Action = entity.Action,
            State = (int)entity.State,
            Comment = entity.Comment,
        };
}
