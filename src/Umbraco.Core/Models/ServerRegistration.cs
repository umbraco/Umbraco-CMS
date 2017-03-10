﻿using System;
using System.Globalization;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a registered server in a multiple-servers environment.
    /// </summary>
    public class ServerRegistration : Entity, IServerRegistration
    {
        private string _serverAddress;
        private string _serverIdentity;
        private bool _isActive;
        private bool _isMaster;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ServerAddressSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, string>(x => x.ServerAddress);
            public readonly PropertyInfo ServerIdentitySelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, string>(x => x.ServerIdentity);
            public readonly PropertyInfo IsActiveSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, bool>(x => x.IsActive);
            public readonly PropertyInfo IsMasterSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, bool>(x => x.IsMaster);
        }

        /// <summary>
        /// Initialiazes a new instance of the <see cref="ServerRegistration"/> class.
        /// </summary>
        public ServerRegistration()
        { }

        /// <summary>
        /// Initialiazes a new instance of the <see cref="ServerRegistration"/> class.
        /// </summary>
        /// <param name="id">The unique id of the server registration.</param>
        /// <param name="serverAddress">The server url.</param>
        /// <param name="serverIdentity">The unique server identity.</param>
        /// <param name="registered">The date and time the registration was created.</param>
        /// <param name="accessed">The date and time the registration was last accessed.</param>
        /// <param name="isActive">A value indicating whether the registration is active.</param>
        /// <param name="isMaster">A value indicating whether the registration is master.</param>
        public ServerRegistration(int id, string serverAddress, string serverIdentity, DateTime registered, DateTime accessed, bool isActive, bool isMaster)
        {
            UpdateDate = accessed;
            CreateDate = registered;
            Key = id.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            Id = id;
            ServerAddress = serverAddress;
            ServerIdentity = serverIdentity;
            IsActive = isActive;
            IsMaster = isMaster;
        }

        /// <summary>
        /// Initialiazes a new instance of the <see cref="ServerRegistration"/> class.
        /// </summary>
        /// <param name="serverAddress">The server url.</param>
        /// <param name="serverIdentity">The unique server identity.</param>
        /// <param name="registered">The date and time the registration was created.</param>
        public ServerRegistration(string serverAddress, string serverIdentity, DateTime registered)
        {
            CreateDate = registered;
            UpdateDate = registered;
            Key = 0.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            ServerAddress = serverAddress;
            ServerIdentity = serverIdentity;
        }

        /// <summary>
        /// Gets or sets the server url.
        /// </summary>
        public string ServerAddress
        {
            get { return _serverAddress; }
            set { SetPropertyValueAndDetectChanges(value, ref _serverAddress, Ps.Value.ServerAddressSelector); }
        }

        /// <summary>
        /// Gets or sets the server unique identity.
        /// </summary>
        public string ServerIdentity
        {
            get { return _serverIdentity; }
            set { SetPropertyValueAndDetectChanges(value, ref _serverIdentity, Ps.Value.ServerIdentitySelector); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is active.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set { SetPropertyValueAndDetectChanges(value, ref _isActive, Ps.Value.IsActiveSelector); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is master.
        /// </summary>
        public bool IsMaster
        {
            get { return _isMaster; }
            set { SetPropertyValueAndDetectChanges(value, ref _isMaster, Ps.Value.IsMasterSelector); }
        }

        /// <summary>
        /// Gets the date and time the registration was created.
        /// </summary>
        public DateTime Registered { get { return CreateDate; } set { CreateDate = value; }}

        /// <summary>
        /// Gets the date and time the registration was last accessed.
        /// </summary>
        public DateTime Accessed { get { return UpdateDate; } set { UpdateDate = value; }}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{\"{0}\", \"{1}\", {2}active, {3}master}}", ServerAddress, ServerIdentity, IsActive ? "" : "!", IsMaster ? "" : "!");
        }
    }
}