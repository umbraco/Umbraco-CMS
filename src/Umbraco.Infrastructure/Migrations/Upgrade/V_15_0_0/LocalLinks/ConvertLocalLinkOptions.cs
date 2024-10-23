using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class ConvertLocalLinkOptions
{
    public List<ProcessorInformation> Processors { get; } = new();
}

[Obsolete("Will be removed in V18")]
public class ProcessorInformation
{
    public Type PropertyEditorValueType { get; init; }

    public IEnumerable<string> PropertyEditorAliases { get; init; }

    public Func<object?, bool> Process { get; init; }

    public ProcessorInformation(Type propertyEditorValueType, IEnumerable<string> propertyEditorAliases, Func<object?, bool> process)
    {
        PropertyEditorValueType = propertyEditorValueType;
        PropertyEditorAliases = propertyEditorAliases;
        Process = process;
    }
}

[Obsolete("Will be removed in V18")]
public class ConvertLocalLinkProcessorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<LocalLinkProcessor>(provider => new LocalLinkProcessor(provider.GetRequiredService<HtmlLocalLinkParser>(), provider.GetRequiredService<IdKeyMap>()));
        builder.Services.AddUnique<LocalLinkBlocksProcessor>(provider => new LocalLinkBlocksProcessor(provider.GetRequiredService<HtmlLocalLinkParser>(), provider.GetRequiredService<IdKeyMap>()));
        builder.Services.AddUnique<LocalLinkRteProcessor>(provider => new LocalLinkRteProcessor(provider.GetRequiredService<HtmlLocalLinkParser>(), provider.GetRequiredService<IdKeyMap>()));
        builder.Services.ConfigureOptions<ConfigureConvertLocalLinkOptions>();
    }
}

[Obsolete("Will be removed in V18")]
public class ConfigureConvertLocalLinkOptions : IConfigureOptions<ConvertLocalLinkOptions>
{
    private readonly LocalLinkBlocksProcessor _localLinkBlocksProcessor;
    private readonly LocalLinkRteProcessor _linkLinkRteProcessor;

    public ConfigureConvertLocalLinkOptions(
        LocalLinkBlocksProcessor localLinkBlocksProcessor,
        LocalLinkRteProcessor localLinkLinkRteProcessor)
    {
        _localLinkBlocksProcessor = localLinkBlocksProcessor;
        _linkLinkRteProcessor = localLinkLinkRteProcessor;
    }

    public void Configure(ConvertLocalLinkOptions options)
    {
        options.Processors.Add(new ProcessorInformation(
            typeof(RichTextEditorValue),
            [Constants.PropertyEditors.Aliases.TinyMce, Constants.PropertyEditors.Aliases.RichText],
            _linkLinkRteProcessor.ProcessRichText));

        options.Processors.Add(new ProcessorInformation(
            typeof(RichTextEditorValue),
            [Constants.PropertyEditors.Aliases.BlockList, Constants.PropertyEditors.Aliases.BlockGrid],
            _localLinkBlocksProcessor.ProcessBlocks));
    }
}
