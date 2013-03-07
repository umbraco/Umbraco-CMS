using System;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;
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
        /// <param name="computerName"></param>
        /// <param name="createDate"></param>
        /// <param name="updateDate"></param>
        public ServerRegistration(int id, string serverAddress, string computerName, DateTime createDate, DateTime updateDate)
        {
            UpdateDate = updateDate;
            CreateDate = createDate;
            Key = Id.ToString().EncodeAsGuid();
            Id = id;
            ServerAddress = serverAddress;
            ComputerName = computerName;
        }

        /// <summary>
        /// Creates a new instance for persisting a new item
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="computerName"></param>
        /// <param name="createDate"></param>
        public ServerRegistration(string serverAddress, string computerName, DateTime createDate)
        {
            CreateDate = createDate;
            UpdateDate = createDate;
            Key = 0.ToString().EncodeAsGuid();
            ServerAddress = serverAddress;
            ComputerName = computerName;
        }

        public string ServerAddress { get; set; }
        public string ComputerName { get; set; }
        public bool IsActive { get; set; }
    }
}