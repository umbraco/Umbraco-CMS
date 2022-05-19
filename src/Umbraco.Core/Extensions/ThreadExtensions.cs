// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Extensions;

public static class ThreadExtensions
{
    public static void SanitizeThreadCulture(this Thread thread)
    {
        // get the current culture
        CultureInfo currentCulture = CultureInfo.CurrentCulture;

        // at the top of any culture should be the invariant culture - find it
        // doing an .Equals comparison ensure that we *will* find it and not loop
        // endlessly
        CultureInfo invariantCulture = currentCulture;
        while (invariantCulture.Equals(CultureInfo.InvariantCulture) == false)
        {
            invariantCulture = invariantCulture.Parent;
        }

        // now that invariant culture should be the same object as CultureInfo.InvariantCulture
        // yet for some reasons, sometimes it is not - and this breaks anything that loops on
        // culture.Parent until a reference equality to CultureInfo.InvariantCulture. See, for
        // example, the following code in PerformanceCounterLib.IsCustomCategory:
        //
        // CultureInfo culture = CultureInfo.CurrentCulture;
        // while (culture != CultureInfo.InvariantCulture)
        // {
        //     library = GetPerformanceCounterLib(machine, culture);
        //     if (library.IsCustomCategory(category))
        //         return true;
        //     culture = culture.Parent;
        // }
        //
        // The reference comparisons never succeeds, hence the loop never ends, and the
        // application hangs.
        //
        // granted, that comparison should probably be a .Equals comparison, but who knows
        // how many times the framework assumes that it can do a reference comparison? So,
        // better fix the cultures.
        if (ReferenceEquals(invariantCulture, CultureInfo.InvariantCulture))
        {
            return;
        }

        // if we do not have equality, fix cultures by replacing them with a culture with
        // the same name, but obtained here and now, with a proper invariant top culture
        thread.CurrentCulture = CultureInfo.GetCultureInfo(thread.CurrentCulture.Name);
        thread.CurrentUICulture = CultureInfo.GetCultureInfo(thread.CurrentUICulture.Name);
    }
}
