// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Defines the contract for that allows the parsing of chrontab expressions.
/// </summary>
public interface ICronTabParser
{
    /// <summary>
    ///     Returns a value indicating whether a given chrontab expression is valid.
    /// </summary>
    /// <param name="cronTab">The chrontab expression to parse.</param>
    /// <returns>The <see cref="bool" /> result.</returns>
    bool IsValidCronTab(string cronTab);

    /// <summary>
    ///     Returns the next occurence for the given chrontab expression from the given time.
    /// </summary>
    /// <param name="cronTab">The chrontab expression to parse.</param>
    /// <param name="time">The date and time to start from.</param>
    /// <returns>The <see cref="DateTime" /> representing the next occurence.</returns>
    DateTime GetNextOccurrence(string cronTab, DateTime time);
}
