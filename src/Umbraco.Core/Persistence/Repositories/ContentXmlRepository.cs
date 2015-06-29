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
    /// Internal class to handle content/published xml insert/update based on standard principles and units of work with transactions
    /// </summary>
    internal class ContentXmlRepository<TContent> : PetaPocoRepositoryBase<int, ContentXmlEntity<TContent>> 
        where TContent : IContentBase
    {
        public ContentXmlRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        #region Not implemented (don't need to for the purposes of this repo)
        protected override ContentXmlEntity<TContent> PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContentXmlEntity<TContent>> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContentXmlEntity<TContent>> PerformGetByQuery(IQuery<ContentXmlEntity<TContent>> query)
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

        //NOTE: Not implemented because all ContentXmlEntity will always return false for having an Identity
        protected override void PersistUpdatedItem(ContentXmlEntity<TContent> entity)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        protected override void PersistDeletedItem(ContentXmlEntity<TContent> entity)
        {
            //Remove 'published' xml from the cmsContentXml table for the unpublished content
            Database.Delete<ContentXmlDto>("WHERE nodeId = @Id", new { Id = entity.Id });
        }

        protected override void PersistNewItem(ContentXmlEntity<TContent> entity)
        {
            if (entity.Content.HasIdentity == false)
            {
                throw new InvalidOperationException("Cannot insert or update an xml entry for a content item that has no identity");
            }

            var poco = new ContentXmlDto
            {
                NodeId = entity.Id, 
                Xml = entity.Xml.ToString(SaveOptions.None)
            };

            //We need to do a special InsertOrUpdate here because we know that the ContentXmlDto table has a 1:1 relation
            // with the content table and a record may or may not exist so the 
            // unique constraint which can be violated if 2+ threads try to execute the same insert sql at the same time.
            Database.InsertOrUpdate(poco);
            
        }

    }
}