using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal static class VersionableRepositoryBaseAliasRegex
    {
        private static readonly Dictionary<Type, Regex> Regexes = new Dictionary<Type, Regex>();

        public static Regex For(ISqlSyntaxProvider sqlSyntax)
        {
            var type = sqlSyntax.GetType();
            Regex aliasRegex;
            if (Regexes.TryGetValue(type, out aliasRegex))
                return aliasRegex;

            var col = Regex.Escape(sqlSyntax.GetQuotedColumnName("column")).Replace("column", @"\w+");
            var fld = Regex.Escape(sqlSyntax.GetQuotedTableName("table") + ".").Replace("table", @"\w+") + col;
            aliasRegex = new Regex("(" + fld + @")\s+AS\s+(" + col + ")", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Regexes[type] = aliasRegex;
            return aliasRegex;
        }
    }
}
