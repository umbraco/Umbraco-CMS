namespace Umbraco.Cms.Core.Preview;

/// <summary>
/// A no-operation implementation of <see cref="IPreviewTokenGenerator"/> that always fails.
/// </summary>
/// <remarks>
/// This implementation is used as a placeholder when preview functionality is not configured or available.
/// All operations return failed attempts.
/// </remarks>
public class NoopPreviewTokenGenerator : IPreviewTokenGenerator
{
    /// <inheritdoc />
    public Task<Attempt<string?>> GenerateTokenAsync(Guid userKey) => Task.FromResult(Attempt.Fail(string.Empty));

    /// <inheritdoc />
    public Task<Attempt<Guid?>> VerifyAsync(string token) => Task.FromResult(Attempt.Fail<Guid?>(null));
}
