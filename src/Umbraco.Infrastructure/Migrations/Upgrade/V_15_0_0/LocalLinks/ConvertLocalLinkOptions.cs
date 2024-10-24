namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class ConvertLocalLinkOptions
{
    public List<ProcessorInformation> Processors { get; } = [];
}

[Obsolete("Will be removed in V18")]
public class ProcessorInformation
{
    public Type PropertyEditorValueType { get; }

    public IEnumerable<string> PropertyEditorAliases { get; }

    public Func<object?, bool> Process { get; }

    public ProcessorInformation(Type propertyEditorValueType, IEnumerable<string> propertyEditorAliases, Func<object?, bool> process)
    {
        PropertyEditorValueType = propertyEditorValueType;
        PropertyEditorAliases = propertyEditorAliases;
        Process = process;
    }
}
