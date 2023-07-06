﻿using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ILogViewerRepository
{
    IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, string? filterExpression);
}
