using System;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Used if a migration has executed but the whole process has failed and cannot be rolled back
    /// </summary>
    internal class CatastrophicDataLossException : Exception
    {
        public CatastrophicDataLossException(string msg)
            : base(msg)
        {
            
        }

        public CatastrophicDataLossException(string msg, Exception inner)
            : base(msg, inner)
        {
            
        }
    }
}