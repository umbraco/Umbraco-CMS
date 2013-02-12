using System;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Models
{
    internal class ServerRegistration : Entity, IServerAddress, IAggregateRoot
    {
        public ServerRegistration()
        {
            
        }

        /// <summary>
        /// Creates an item with pre-filled properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serverAddress"></param>
        /// <param name="createDate"></param>
        /// <param name="updateDate"></param>
        public ServerRegistration(int id, string serverAddress, DateTime createDate, DateTime updateDate)
        {
            UpdateDate = updateDate;
            CreateDate = createDate;
            Key = Id.ToString().EncodeAsGuid();
            Id = id;
            ServerAddress = serverAddress;
        }

        /// <summary>
        /// Creates a new instance for persisting a new item
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="createDate"></param>
        public ServerRegistration(string serverAddress, DateTime createDate)
        {
            CreateDate = createDate;
            UpdateDate = createDate;
            Key = 0.ToString().EncodeAsGuid();
            ServerAddress = serverAddress;
        }

        public string ServerAddress { get; set; }        
        public bool IsActive { get; set; }
    }
}