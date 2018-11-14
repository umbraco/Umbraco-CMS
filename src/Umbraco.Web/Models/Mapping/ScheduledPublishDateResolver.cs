using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ScheduledPublishDateResolver : IValueResolver<IContent, ContentVariantDisplay, DateTime?>
    {
        private readonly ContentScheduleAction _changeType;

        public ScheduledPublishDateResolver(ContentScheduleAction changeType)
        {
            _changeType = changeType;
        }

        public DateTime? Resolve(IContent source, ContentVariantDisplay destination, DateTime? destMember, ResolutionContext context)
        {
            var culture = context.Options.GetCulture();
            var sched = source.ContentSchedule.GetSchedule(culture ?? string.Empty, _changeType);
            foreach(var s in sched)
                return s.Date; // take the first, it's ordered by date

            return null;
        }
    }
}
