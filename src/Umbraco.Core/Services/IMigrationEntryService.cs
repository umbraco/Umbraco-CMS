using System;
using System.Collections.Generic;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMigrationEntryService
    {
        /// <summary>
        /// Creates a migration entry, will throw an exception if it already exists
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        IMigrationEntry CreateEntry(string migrationName, SemVersion version);

        /// <summary>
        /// Finds a migration by name and version, returns null if not found
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        IMigrationEntry FindEntry(string migrationName, SemVersion version);

        /// <summary>
        /// Gets all entries for a given migration name
        /// </summary>
        /// <param name="migrationName"></param>
        /// <returns></returns>
        IEnumerable<IMigrationEntry> GetAll(string migrationName);
    }
}