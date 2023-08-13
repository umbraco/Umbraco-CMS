// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]
public enum LuceneDirectoryFactory
{
    /// <summary>
    ///     The index will operate from the default location: Umbraco/Data/Temp/ExamineIndexes
    /// </summary>
    Default,

    /// <summary>
    ///     The index will operate on a local index created in the processes %temp% location and
    ///     will replicate back to main storage in Umbraco/Data/Temp/ExamineIndexes
    /// </summary>
    SyncedTempFileSystemDirectoryFactory,

    /// <summary>
    ///     The index will operate only in the processes %temp% directory location
    /// </summary>
    TempFileSystemDirectoryFactory,
}
