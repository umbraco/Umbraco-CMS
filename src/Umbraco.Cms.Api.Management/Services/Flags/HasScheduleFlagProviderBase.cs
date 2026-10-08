using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.Models;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Base class for flag providers that add a "scheduled for publish" flag to items with active release schedules.
/// </summary>
internal abstract class HasScheduleFlagProviderBase : IFlagProvider
{
    protected const string Alias = Constants.Conventions.Flags.Prefix + "ScheduledForPublish";

    private readonly TimeProvider _timeProvider;

    protected HasScheduleFlagProviderBase(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public abstract bool CanProvideFlags<TItem>()
        where TItem : IHasFlags;

    /// <inheritdoc/>
    public Task PopulateFlagsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasFlags
    {
        TItem[] itemsArray = items.ToArray();
        Guid[] keys = itemsArray.Select(i => i.Id).ToArray();
        IDictionary<Guid, IEnumerable<ContentSchedule>> schedulesByKey = GetSchedulesByKeys(keys);

        foreach (TItem item in itemsArray)
        {
            if (!schedulesByKey.TryGetValue(item.Id, out IEnumerable<ContentSchedule>? schedules))
            {
                continue;
            }

            ContentSchedule[] releaseSchedules = schedules
                .Where(s => s.Action == ContentScheduleAction.Release)
                .ToArray();

            if (releaseSchedules.Length == 0)
            {
                continue;
            }

            PopulateItemFlags(item, releaseSchedules);
        }

        return Task.CompletedTask;
    }

    protected abstract IDictionary<Guid, IEnumerable<ContentSchedule>> GetSchedulesByKeys(Guid[] keys);

    protected abstract void PopulateItemFlags<TItem>(TItem item, ContentSchedule[] releaseSchedules)
        where TItem : IHasFlags;

    protected IEnumerable<TVariant> PopulateVariants<TVariant>(
        IEnumerable<TVariant> variants,
        ContentSchedule[] schedules,
        Func<TVariant, string?> getCulture)
        where TVariant : IHasFlags
    {
        TVariant[] variantsArray = variants.ToArray();
        if (variantsArray.Length == 1)
        {
            variantsArray[0].AddFlag(Alias);
            return variantsArray;
        }

        foreach (TVariant variant in variantsArray)
        {
            var culture = getCulture(variant);
            ContentSchedule? schedule = schedules.FirstOrDefault(x => x.Culture == culture);
            if (schedule is not null && schedule.Date > _timeProvider.GetUtcNow().UtcDateTime && string.Equals(schedule.Culture, culture))
            {
                variant.AddFlag(Alias);
            }
        }

        return variantsArray;
    }
}
