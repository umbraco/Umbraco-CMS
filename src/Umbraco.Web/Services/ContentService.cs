using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.Publishing;

namespace Umbraco.Web.Services
{
    public class ContentService : IContentService
    {
        public IContent CreateContent(string contentTypeAlias)
        {
            throw new NotImplementedException();
        }

        public IContent GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetByLevel(int level)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetChildren(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetVersions(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetRootContent()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetContentForExpiration()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetContentForRelease()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContent> GetContentInRecycleBin()
        {
            throw new NotImplementedException();
        }

        public bool RePublishAll()
        {
            throw new NotImplementedException();
        }

        public bool Publish(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public bool PublishWithChildren(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public bool SaveAndPublish(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public void Save(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public void Save(IEnumerable<IContent> contents, int userId)
        {
            throw new NotImplementedException();
        }

        public void DeleteContentOfType(int contentTypeId)
        {
            throw new NotImplementedException();
        }

        public void Delete(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public void MoveToRecycleBin(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public void Move(IContent content, int parentId, int userId)
        {
            throw new NotImplementedException();
        }

        public IContent Copy(IContent content, int parentId, int userId)
        {
            throw new NotImplementedException();
        }

        public void SendToPublication(IContent content, int userId)
        {
            throw new NotImplementedException();
        }

        public IContent Rollback(int id, Guid versionId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}