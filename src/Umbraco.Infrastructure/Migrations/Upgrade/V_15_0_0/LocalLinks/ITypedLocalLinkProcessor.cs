namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public interface ITypedLocalLinkProcessor
{
    public Type PropertyEditorValueType { get; }

    public IEnumerable<string> PropertyEditorAliases { get; }

    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process { get; }
}
