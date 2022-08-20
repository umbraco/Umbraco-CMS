using Umbraco.Cms.Core.OAuth;

namespace Umbraco.Cms.Infrastructure.OAuth
{
    internal class InMemoryTokenStorage : ITokenStorage
    {
        private static readonly Dictionary<string, Token> _tokens = new Dictionary<string, Token>();

        public Token? GetToken(string serviceAlias)
        {
            if (_tokens.ContainsKey(serviceAlias))
            {
                return _tokens[serviceAlias];
            }

            return null;
        }

        public void SaveToken(string serviceAlias, Token token)
        {
            if (_tokens.ContainsKey(serviceAlias))
            {
                _tokens[serviceAlias] = token;
            }
            else
            {
                _tokens.Add(serviceAlias, token);
            }
        }

        public void DeleteToken(string serviceAlias) => _tokens.Remove(serviceAlias);
    }
}
