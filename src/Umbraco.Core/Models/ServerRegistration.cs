using System;
using System.Globalization;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Models
{
    internal class ServerRegistration : Entity, IServerAddress, IAggregateRoot
    {
        private string _serverAddress;
        private string _computerName;
        private bool _isActive;

        private static readonly PropertyInfo ServerAddressSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, string>(x => x.ServerAddress);
        private static readonly PropertyInfo ComputerNameSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, string>(x => x.ComputerName);
        private static readonly PropertyInfo IsActiveSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, bool>(x => x.IsActive);

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
        /// <param name="isActive"></param>
        public ServerRegistration(int id, string serverAddress, string computerName, DateTime createDate, DateTime updateDate, bool isActive)
        {
            UpdateDate = updateDate;
            CreateDate = createDate;
            Key = Id.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            Id = id;
            ServerAddress = serverAddress;
            ComputerName = computerName;
            IsActive = isActive;
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
            Key = 0.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            ServerAddress = serverAddress;
            ComputerName = computerName;
        }

        public string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _serverAddress = value;
                    return _serverAddress;
                }, _serverAddress, ServerAddressSelector);
            }
        }

        public string ComputerName
        {
            get { return _computerName; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _computerName = value;
                    return _computerName;
                }, _computerName, ComputerNameSelector);
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _isActive = value;
                    return _isActive;
                }, _isActive, IsActiveSelector);
            }
        }

        public override string ToString()
        {
            return "(" + ServerAddress + ", " + ComputerName + ", IsActive = " + IsActive + ")";
        }
    }
}