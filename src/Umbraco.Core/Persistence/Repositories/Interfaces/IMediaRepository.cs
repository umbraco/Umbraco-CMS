using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IRepositoryVersionable<int, IMedia>, IRecycleBinRepository<IMedia>, IReadRepository<Guid, IMedia>, IDeleteMediaFilesRepository
    {
        IMedia GetMediaByPath(string mediaPath);

        /// <summary>
        /// Used to add/update published xml for the media item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdateContentXml(IMedia content, Func<IMedia, XElement> xml);

        /// <summary>
        /// Used to remove the content xml for a content item
        /// </summary>
        /// <param name="content"></param>
        void DeleteContentXml(IMedia content);

        /// <summary>
        /// Used to add/update preview xml for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdatePreviewXml(IMedia content, Func<IMedia, XElement> xml);        
    }
}