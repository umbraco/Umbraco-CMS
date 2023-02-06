using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.LogViewer;

public class LogViewerViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<LogLevelCounts, LogLevelCountsViewModel>((source, context) => new LogLevelCountsViewModel(), Map);
        mapper.Define<KeyValuePair<string, string?>, LogMessagePropertyViewModel>(
            (source, context) => new LogMessagePropertyViewModel() { Name = string.Empty }, Map);
        mapper.Define<KeyValuePair<string, LogLevel>, LoggerViewModel>((source, context) => new LoggerViewModel() { Name = string.Empty }, Map);
        mapper.Define<ILogViewerQuery, SavedLogSearchViewModel>(
            (source, context) => new SavedLogSearchViewModel()
            {
                Name = string.Empty,
                Query = string.Empty
            },
            Map);
        mapper.Define<LogTemplate, LogTemplateViewModel>((source, context) => new LogTemplateViewModel(), Map);
        mapper.Define<ILogEntry, LogMessageViewModel>((source, context) => new LogMessageViewModel(), Map);
        mapper.Define<IEnumerable<KeyValuePair<string, LogLevel>>, PagedViewModel<LoggerViewModel>>((source, context) => new PagedViewModel<LoggerViewModel>(), Map);
        mapper.Define<IEnumerable<ILogViewerQuery>, PagedViewModel<SavedLogSearchViewModel>>((source, context) => new PagedViewModel<SavedLogSearchViewModel>(), Map);
        mapper.Define<IEnumerable<LogTemplate>, PagedViewModel<LogTemplateViewModel>>((source, context) => new PagedViewModel<LogTemplateViewModel>(), Map);
        mapper.Define<PagedModel<ILogEntry>, PagedViewModel<LogMessageViewModel>>((source, context) => new PagedViewModel<LogMessageViewModel>(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(LogLevelCounts source, LogLevelCountsViewModel target, MapperContext context)
    {
        target.Information = source.Information;
        target.Debug = source.Debug;
        target.Warning = source.Warning;
        target.Error = source.Error;
        target.Fatal = source.Fatal;
    }

    // Umbraco.Code.MapAll
    private static void Map(KeyValuePair<string, string?> source, LogMessagePropertyViewModel target, MapperContext context)
    {
        target.Name = source.Key;
        target.Value = source.Value;
    }

    // Umbraco.Code.MapAll
    private static void Map(KeyValuePair<string, LogLevel> source, LoggerViewModel target, MapperContext context)
    {
        target.Name = source.Key;
        target.Level = source.Value;
    }

    // Umbraco.Code.MapAll
    private static void Map(ILogViewerQuery source, SavedLogSearchViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Query = source.Query;
    }

    // Umbraco.Code.MapAll
    private static void Map(LogTemplate source, LogTemplateViewModel target, MapperContext context)
    {
        target.MessageTemplate = source.MessageTemplate;
        target.Count = source.Count;
    }


    // Umbraco.Code.MapAll
    private static void Map(ILogEntry source, LogMessageViewModel target, MapperContext context)
    {
        target.Timestamp = source.Timestamp;
        target.Level = source.Level;
        target.MessageTemplate = source.MessageTemplateText;
        target.RenderedMessage = source.RenderedMessage;
        target.Properties = context.MapEnumerable<KeyValuePair<string, string?>, LogMessagePropertyViewModel>(source.Properties);
        target.Exception = source.Exception;
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<KeyValuePair<string, LogLevel>> source, PagedViewModel<LoggerViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<KeyValuePair<string, LogLevel>, LoggerViewModel>(source);
        target.Total = source.Count();
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<ILogViewerQuery> source, PagedViewModel<SavedLogSearchViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<ILogViewerQuery, SavedLogSearchViewModel>(source);
        target.Total = source.Count();
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<LogTemplate> source, PagedViewModel<LogTemplateViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<LogTemplate, LogTemplateViewModel>(source);
        target.Total = source.Count();
    }

    // Umbraco.Code.MapAll
    private static void Map(PagedModel<ILogEntry> source, PagedViewModel<LogMessageViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<ILogEntry, LogMessageViewModel>(source.Items);
        target.Total = source.Total;
    }
}
