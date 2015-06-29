using System;

namespace Umbraco.Core.Auditing
{
    internal interface IAuditWriteProvider
    {
        /// <summary>
        /// Writes an audit entry to the underlaying datastore.
        /// </summary>
        /// <param name="objectId">Id of the object (Content, ContentType, Media, etc.)</param>
        /// <param name="userId">Id of the user</param>
        /// <param name="date">Datestamp</param>
        /// <param name="header">Audit header</param>
        /// <param name="comment">Audit comment</param>
        void WriteEntry(int objectId, int userId, DateTime date, string header, string comment);
    }
}