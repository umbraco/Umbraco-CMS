using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class ConsentFactory
    {
        public static IConsent BuildEntity(ConsentDto dto)
        {
            var consent = new Consent
            {
                Id = dto.Id,
                UpdateDate = dto.UpdateDate,
                Source = dto.Source,
                Action = dto.Action,
                // ActionType derives from Action
                State = (ConsentState) dto.State, // assume value is valid
                Comment = dto.Comment
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            consent.ResetDirtyProperties(false);
            return consent;
        }

        public static ConsentDto BuildDto(IConsent entity)
        {
            return new ConsentDto
            {
                Id = entity.Id,
                UpdateDate = entity.UpdateDate,
                Source = entity.Source,
                Action = entity.Action,
                ActionType = entity.ActionType,
                State = (int) entity.State,
                Comment = entity.Comment
            };
        }
    }
}
