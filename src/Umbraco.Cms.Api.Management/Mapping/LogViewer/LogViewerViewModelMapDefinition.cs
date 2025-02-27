using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.LogViewer;

public class LogViewerViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<LogLevelCounts, LogLevelCountsReponseModel>((_, _) => new LogLevelCountsReponseModel(), Map);
        mapper.Define<KeyValuePair<string, string?>, LogMessagePropertyPresentationModel>(
            (_, _) => new LogMessagePropertyPresentationModel { Name = string.Empty }, Map);
        mapper.Define<KeyValuePair<string, LogLevel>, LoggerResponseModel>((_, _) => new LoggerResponseModel { Name = string.Empty }, Map);
        mapper.Define<ILogViewerQuery, SavedLogSearchResponseModel>(
            (_, _) => new SavedLogSearchResponseModel
            {
                Name = string.Empty,
                Query = string.Empty
            },
            Map);
        mapper.Define<LogTemplate, LogTemplateResponseModel>((_, _) => new LogTemplateResponseModel(), Map);
        mapper.Define<ILogEntry, LogMessageResponseModel>((_, _) => new LogMessageResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(LogLevelCounts source, LogLevelCountsReponseModel target, MapperContext context)
    {
        target.Information = source.Information;
        target.Debug = source.Debug;
        target.Warning = source.Warning;
        target.Error = source.Error;
        target.Fatal = source.Fatal;
    }

    // Umbraco.Code.MapAll
    private static void Map(KeyValuePair<string, string?> source, LogMessagePropertyPresentationModel target, MapperContext context)
    {
        target.Name = source.Key;
        target.Value = source.Value;
    }

    // Umbraco.Code.MapAll
    private static void Map(KeyValuePair<string, LogLevel> source, LoggerResponseModel target, MapperContext context)
    {
        target.Name = source.Key;
        target.Level = source.Value;
    }

    // Umbraco.Code.MapAll
    private static void Map(ILogViewerQuery source, SavedLogSearchResponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Query = source.Query;
    }

    // Umbraco.Code.MapAll
    private static void Map(LogTemplate source, LogTemplateResponseModel target, MapperContext context)
    {
        target.MessageTemplate = source.MessageTemplate;
        target.Count = source.Count;
    }


    // Umbraco.Code.MapAll
    private static void Map(ILogEntry source, LogMessageResponseModel target, MapperContext context)
    {
        target.Timestamp = source.Timestamp;
        target.Level = source.Level;
        target.MessageTemplate = source.MessageTemplateText;
        target.RenderedMessage = source.RenderedMessage;
        target.Properties = context.MapEnumerable<KeyValuePair<string, string?>, LogMessagePropertyPresentationModel>(source.Properties);
        target.Exception = source.Exception;
    }
}
