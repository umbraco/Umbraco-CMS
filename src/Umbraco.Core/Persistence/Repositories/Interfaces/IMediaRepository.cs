using System;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IRepositoryVersionable<int, IMedia>
    {

        /// <summary>
        /// Used to add/update published xml for the media item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdateContentXml(IMedia content, Func<IMedia, XElement> xml);

    }
}