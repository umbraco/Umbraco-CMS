using System;
using System.Globalization;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a registered server in a multiple-servers environment.
    /// </summary>
    public class ServerRegistration : Entity, IServerAddress, IAggregateRoot
    {
        private string _serverAddress;
        private string _serverIdentity;
        private bool _isActive;

        private static readonly PropertyInfo ServerAddressSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, string>(x => x.ServerAddress);
        private static readonly PropertyInfo ServerIdentitySelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, string>(x => x.ServerIdentity);
        private static readonly PropertyInfo IsActiveSelector = ExpressionHelper.GetPropertyInfo<ServerRegistration, bool>(x => x.IsActive);

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
        public ServerRegistration(int id, string serverAddress, string serverIdentity, DateTime registered, DateTime accessed, bool isActive)
        {
            UpdateDate = accessed;
            CreateDate = registered;
            Key = id.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            Id = id;
            ServerAddress = serverAddress;
            ServerIdentity = serverIdentity;
            IsActive = isActive;
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _serverAddress = value;
                    return _serverAddress;
                }, _serverAddress, ServerAddressSelector);
            }
        }

        /// <summary>
        /// Gets or sets the server unique identity.
        /// </summary>
        public string ServerIdentity
        {
            get { return _serverIdentity; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _serverIdentity = value;
                    return _serverIdentity;
                }, _serverIdentity, ServerIdentitySelector);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is active.
        /// </summary>
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
            return string.Format("{{\"{0}\", \"{1}\", {2}active}}", ServerAddress, ServerIdentity, IsActive ? "" : "!");
        }
    }
}