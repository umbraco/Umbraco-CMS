using System;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Models
{
    internal class Macro : Entity, IAggregateRoot
    {
        /// <summary>
        /// Creates an item with pre-filled properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="useInEditor"></param>
        /// <param name="refreshRate"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="controlType"></param>
        /// <param name="controlAssembly"></param>
        /// <param name="xsltPath"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cachePersonalized"></param>
        /// <param name="dontRender"></param>
        /// <param name="scriptPath"></param>
        public Macro(int id, bool useInEditor, int refreshRate, string @alias, string name, string controlType, string controlAssembly, string xsltPath, bool cacheByPage, bool cachePersonalized, bool dontRender, string scriptPath)
        {
            Id = id;
            UseInEditor = useInEditor;
            RefreshRate = refreshRate;
            Alias = alias;
            Name = name;
            ControlType = controlType;
            ControlAssembly = controlAssembly;
            XsltPath = xsltPath;
            CacheByPage = cacheByPage;
            CachePersonalized = cachePersonalized;
            DontRender = dontRender;
            ScriptPath = scriptPath;
        }

        /// <summary>
        /// Creates an instance for persisting a new item
        /// </summary>
        /// <param name="useInEditor"></param>
        /// <param name="refreshRate"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="controlType"></param>
        /// <param name="controlAssembly"></param>
        /// <param name="xsltPath"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cachePersonalized"></param>
        /// <param name="dontRender"></param>
        /// <param name="scriptPath"></param>
        public Macro(string @alias, string name,
            string controlType = "",
            string controlAssembly = "",
            string xsltPath = "", 
            string scriptPath = "",
            bool cacheByPage = false, 
            bool cachePersonalized = false, 
            bool dontRender = true,
            bool useInEditor = false, 
            int refreshRate = 0)
        {
            UseInEditor = useInEditor;
            RefreshRate = refreshRate;
            Alias = alias;
            Name = name;
            ControlType = controlType;
            ControlAssembly = controlAssembly;
            XsltPath = xsltPath;
            CacheByPage = cacheByPage;
            CachePersonalized = cachePersonalized;
            DontRender = dontRender;
            ScriptPath = scriptPath;
        }

        public bool UseInEditor { get; set; }

        public int RefreshRate { get; set; }
        
        public string Alias { get; set; }
        
        public string Name { get; set; }
        
        public string ControlType { get; set; }

        public string ControlAssembly { get; set; }
        
        public string XsltPath { get; set; }
        
        public bool CacheByPage { get; set; }
        
        public bool CachePersonalized { get; set; }

        public bool DontRender { get; set; }
        
        public string ScriptPath { get; set; }
    }

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
            Key = Id.ToString().EncodeAsGuid();
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
            Key = 0.ToString().EncodeAsGuid();
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