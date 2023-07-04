namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]
internal interface IDeferredAction
{
    void Execute();
}
