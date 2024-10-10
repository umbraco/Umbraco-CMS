namespace Umbraco.Cms.Core.Preview;

public class NoopPreviewTokenGenerator : IPreviewTokenGenerator
{
    public Task<Attempt<string?>> GenerateTokenAsync(Guid userKey) => Task.FromResult(Attempt.Fail(string.Empty));

    public Task<Attempt<Guid?>> VerifyAsync(string token) => Task.FromResult(Attempt.Fail<Guid?>(null));
}
