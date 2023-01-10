using System.Text.Json;
using Serilog.Events;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Log;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Log;

public class LogViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<LogLevelCounts, LogLevelCountsViewModel>((source, context) => new LogLevelCountsViewModel(), Map);
        mapper.Define<KeyValuePair<string, LogEventLevel?>, LoggerViewModel>((source, context) => new LoggerViewModel() { Name = string.Empty }, Map);
        mapper.Define<SavedLogSearch, SavedLogSearchViewModel>(
            (source, context) => new SavedLogSearchViewModel()
            {
                Name = string.Empty,
                Query = string.Empty
            },
            Map);
        mapper.Define<LogTemplate, LogTemplateViewModel>((source, context) => new LogTemplateViewModel(), Map);
        mapper.Define<LogMessage, LogMessageViewModel>((source, context) => new LogMessageViewModel(), Map);
        mapper.Define<IEnumerable<KeyValuePair<string, LogEventLevel?>>, PagedViewModel<LoggerViewModel>>((source, context) => new PagedViewModel<LoggerViewModel>(), Map);
        mapper.Define<IEnumerable<SavedLogSearch>, PagedViewModel<SavedLogSearchViewModel>>((source, context) => new PagedViewModel<SavedLogSearchViewModel>(), Map);
        mapper.Define<IEnumerable<LogTemplate>, PagedViewModel<LogTemplateViewModel>>((source, context) => new PagedViewModel<LogTemplateViewModel>(), Map);
        mapper.Define<IEnumerable<LogMessage>, PagedViewModel<LogMessageViewModel>>((source, context) => new PagedViewModel<LogMessageViewModel>(), Map);
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
    private static void Map(KeyValuePair<string, LogEventLevel?> source, LoggerViewModel target, MapperContext context)
    {
        target.Name = source.Key;
        target.Level = source.Value;
    }

    // Umbraco.Code.MapAll
    private static void Map(SavedLogSearch source, SavedLogSearchViewModel target, MapperContext context)
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
    private static void Map(LogMessage source, LogMessageViewModel target, MapperContext context)
    {
        target.Timestamp = source.Timestamp;
        target.Level = source.Level;
        target.MessageTemplate = source.MessageTemplateText;
        target.RenderedMessage = source.RenderedMessage;
        target.Properties = MapLogMessageProperties(source.Properties);
        target.Exception = source.Exception;
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<KeyValuePair<string, LogEventLevel?>> source, PagedViewModel<LoggerViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<KeyValuePair<string, LogEventLevel?>, LoggerViewModel>(source);
        target.Total = source.Count();
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<SavedLogSearch> source, PagedViewModel<SavedLogSearchViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<SavedLogSearch, SavedLogSearchViewModel>(source);
        target.Total = source.Count();
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<LogTemplate> source, PagedViewModel<LogTemplateViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<LogTemplate, LogTemplateViewModel>(source);
        target.Total = source.Count();
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<LogMessage> source, PagedViewModel<LogMessageViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<LogMessage, LogMessageViewModel>(source);
        target.Total = source.Count();
    }

    private static IEnumerable<LogMessagePropertyViewModel> MapLogMessageProperties(IReadOnlyDictionary<string, LogEventPropertyValue>? properties)
    {
        var result = new List<LogMessagePropertyViewModel>();

        if (properties is not null)
        {
            foreach (KeyValuePair<string, LogEventPropertyValue> property in properties)
            {
                string? value;

                if (property.Value is ScalarValue scalarValue)
                {
                    value = scalarValue.Value?.ToString();
                }
                else
                {
                    // When polymorphism is implemented, this should be changed
                    value = JsonSerializer.Serialize(property.Value as object);
                }

                var logMessageProperty = new LogMessagePropertyViewModel()
                {
                    Name = property.Key,
                    Value = value
                };

                result.Add(logMessageProperty);
            }
        }

        return result;
    }
}
