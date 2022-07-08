namespace Umbraco.Cms.Core.OAuth
{
    public interface ITokenStorage
    {
        Token? GetToken(string serviceAlias);

        void SaveToken(string serviceAlias, Token token);
    }
}
