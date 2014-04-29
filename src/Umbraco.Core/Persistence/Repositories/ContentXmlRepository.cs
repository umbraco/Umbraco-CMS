using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Internal class to handle content/published xml insert/update based on standard principles and units of work with transactions
    /// </summary>
    internal class ContentXmlRepository<TContent> : PetaPocoRepositoryBase<int, ContentXmlEntity<TContent>> 
        where TContent : IContentBase
    {
        public ContentXmlRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
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
                throw new InvalidOperationException("Cannot insert an xml entry for a content item that has no identity");
            }

            var poco = new ContentXmlDto { NodeId = entity.Id, Xml = entity.Xml.ToString(SaveOptions.None) };
            Database.Insert(poco);
        }

        protected override void PersistUpdatedItem(ContentXmlEntity<TContent> entity)
        {
            if (entity.Content.HasIdentity == false)
            {
                throw new InvalidOperationException("Cannot update an xml entry for a content item that has no identity");
            }

            var poco = new ContentXmlDto { NodeId = entity.Id, Xml = entity.Xml.ToString(SaveOptions.None) };
            Database.Update(poco);
        }
    }
}