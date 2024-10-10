namespace Umbraco.Cms.Core.Preview;

public interface IPreviewTokenGenerator
{
    Task<Attempt<string?>> GenerateTokenAsync(Guid userKey);
    Task<Attempt<Guid?>> VerifyAsync(string token);
}
