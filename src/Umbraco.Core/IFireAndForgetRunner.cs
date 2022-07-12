namespace Umbraco.Cms.Core;

public interface IFireAndForgetRunner
{
    void RunFireAndForget(Func<Task> task);
}
