using System;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Auditing
{
    public class DataAuditWriteProvider : IAuditWriteProvider
    {
        /// <summary>
        /// Writes an audit entry to the underlaying datastore.
        /// </summary>
        /// <param name="objectId">Id of the object (Content, ContentType, Media, etc.)</param>
        /// <param name="userId">Id of the user</param>
        /// <param name="date">Datestamp</param>
        /// <param name="header">Audit header</param>
        /// <param name="comment">Audit comment</param>
        public void WriteEntry(int objectId, int userId, DateTime date, string header, string comment)
        {
            DatabaseFactory.Current.Database.Insert(new LogDto
                                                        {
                                                            Comment = comment,
                                                            Datestamp = date,
                                                            Header = header,
                                                            NodeId = objectId,
                                                            UserId = userId
                                                        });
        }
    }
}