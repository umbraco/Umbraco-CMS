using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping functionality for queries related to the Log Viewer in Umbraco.
/// This class is responsible for translating between database representations and domain models
/// used by the Log Viewer feature.
/// </summary>
[MapperFor(typeof(ILogViewerQuery))]
[MapperFor(typeof(LogViewerQuery))]
public sealed class LogViewerQueryMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerQueryMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazy-loaded <see cref="ISqlContext"/> used for database operations.</param>
    /// <param name="maps">The <see cref="MapperConfigurationStore"/> containing mapper configurations.</param>
    public LogViewerQueryMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<ILogViewerQuery, LogViewerQueryDto>(nameof(ILogViewerQuery.Id), nameof(LogViewerQueryDto.Id));
        DefineMap<ILogViewerQuery, LogViewerQueryDto>(nameof(ILogViewerQuery.Name), nameof(LogViewerQueryDto.Name));
        DefineMap<ILogViewerQuery, LogViewerQueryDto>(nameof(ILogViewerQuery.Query), nameof(LogViewerQueryDto.Query));
    }
}
