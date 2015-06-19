using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Private class to handle preview insert/update based on standard principles and units of work with transactions
    /// </summary>
    internal class ContentPreviewRepository<TContent> : PetaPocoRepositoryBase<int, ContentPreviewEntity<TContent>> 
        where TContent : IContentBase
    {
        public ContentPreviewRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        #region Not implemented (don't need to for the purposes of this repo)
        protected override ContentPreviewEntity<TContent> PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContentPreviewEntity<TContent>> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContentPreviewEntity<TContent>> PerformGetByQuery(IQuery<ContentPreviewEntity<TContent>> query)
        {
            throw new NotImplementedException();
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            throw new NotImplementedException();
        }

        protected override string GetBaseWhereClause()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistDeletedItem(ContentPreviewEntity<TContent> entity)
        {
            throw new NotImplementedException();
        }

        //NOTE: Not implemented because all ContentPreviewEntity will always return false for having an Identity
        protected override void PersistUpdatedItem(ContentPreviewEntity<TContent> entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override void PersistNewItem(ContentPreviewEntity<TContent> entity)
        {
            if (entity.Content.HasIdentity == false)
            {
                throw new InvalidOperationException("Cannot insert or update a preview for a content item that has no identity");
            }

            var previewPoco = new PreviewXmlDto
            {
                NodeId = entity.Id,
                Timestamp = DateTime.Now,
                VersionId = entity.Version,
                Xml = entity.Xml.ToString(SaveOptions.None)
            };

            //We need to do a special InsertOrUpdate here because we know that the PreviewXmlDto table has a composite key and thus
            // a unique constraint which can be violated if 2+ threads try to execute the same insert sql at the same time.
            Database.InsertOrUpdate(previewPoco,
                //Since the table has a composite key, we need to specify an explit update statement
                "SET xml = @Xml, timestamp = @Timestamp WHERE nodeId=@NodeId AND versionId=@VersionId",
                new {NodeId = previewPoco.NodeId, VersionId = previewPoco.VersionId, Xml = previewPoco.Xml, Timestamp = previewPoco.Timestamp});
        }

    }
}