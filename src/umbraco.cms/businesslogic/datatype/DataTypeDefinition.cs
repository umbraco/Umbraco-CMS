using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using umbraco.DataLayer;
using System.Xml;
using umbraco.cms.businesslogic.media;
using umbraco.interfaces;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// Datatypedefinitions is the basic buildingblocks of umbraco's documents/medias/members generic datastructure 
    /// 
    /// A datatypedefinition encapsulates an object which implements the interface IDataType, and are used when defining
    /// the properties of a document in the documenttype. This extra layer between IDataType and a documenttypes propertytype
    /// are used amongst other for enabling shared prevalues.
    /// 
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public class DataTypeDefinition : CMSNode
    {
        internal IDataTypeDefinition DataTypeItem;

        #region Constructors

        internal DataTypeDefinition(IDataTypeDefinition dataType)
            : base(dataType)
        {
            DataTypeItem = dataType;
        }

        /// <summary>
        /// Initialization of the datatypedefinition
        /// </summary>
        /// <param name="id">Datattypedefininition id</param>
        public DataTypeDefinition(int id) : base(id) { }

        /// <summary>
        /// Initialization of the datatypedefinition
        /// </summary>
        /// <param name="id">Datattypedefininition id</param>
        public DataTypeDefinition(Guid id) : base(id) { }

        #endregion

        #region Public Properties

        public override string Text
        {
            get { return DataTypeItem.Name; }
            set { DataTypeItem.Name = value; }
        }

        /// <summary>
        /// The associated datatype, which delivers the methods for editing data, editing prevalues see: umbraco.interfaces.IDataType
        /// </summary>
        public IDataType DataType
        {
            get
            {
                if (DataTypeItem.PropertyEditorAlias.IsNullOrWhiteSpace()) 
                    return null;

                //Attempt to resolve a legacy control id from the alias. If one is not found we'll generate one - 
                // the reason one will not be found is if there's a new v7 property editor created that doesn't have a legacy
                // property editor predecessor.
                //So, we'll generate an id for it based on the alias which will remain consistent, but then we'll try to resolve a legacy
                // IDataType which of course will not exist. In this case we'll have to create a new one on the fly for backwards compatibility but 
                // this instance will have limited capabilities and will really only work for saving data so the legacy APIs continue to work.
                var controlId = LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(DataTypeItem.PropertyEditorAlias, LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.GenerateId);

                var dt = DataTypesResolver.Current.GetById(controlId.Value);
                
                if (dt != null)
                {
                    dt.DataTypeDefinitionId = Id;
                }
                else
                {
                    //Ok so it was not found, we can only assume that this is because this is a new property editor that does not have a legacy predecessor.
                    //we'll have to attempt to generate one at runtime.
                    dt = BackwardsCompatibleDataType.Create(DataTypeItem.PropertyEditorAlias, controlId.Value, Id);
                }
                    

                return dt;
            }
            set
            {
                if (value == null)
                    throw new InvalidOperationException("The value passed in is null. The DataType property cannot be set to a null value");

                var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(value.Id, true);

                DataTypeItem.PropertyEditorAlias = alias;
            }
        }

        //internal string DbType { get; private set; }

        #endregion

        #region Public methods
        public override void delete()
        {
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);
            if (e.Cancel == false)
            {
                ApplicationContext.Current.Services.DataTypeService.Delete(DataTypeItem);
               
                FireAfterDelete(e);
            }
        }

        [Obsolete("Use the standard delete() method instead")]
        public void Delete()
        {
            delete();
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            ApplicationContext.Current.Services.DataTypeService.Save(DataTypeItem);

            OnSaving(EventArgs.Empty);
        }

        public XmlElement ToXml(XmlDocument xd)
        {
            var serializer = new EntityXmlSerializer();
            var xml = serializer.Serialize(ApplicationContext.Current.Services.DataTypeService, DataTypeItem);
            return (XmlElement)xml.GetXmlNode(xd);

        }
        #endregion

        #region Static methods

        [Obsolete("Do not use this method, it will not function correctly because legacy property editors are not supported in v7")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static DataTypeDefinition Import(XmlNode xmlData)
        {
            var name = xmlData.Attributes["Name"].Value;
            var id = xmlData.Attributes["Id"].Value;
            var def = xmlData.Attributes["Definition"].Value;


            //Make sure that the dtd is not already present
            if (IsNode(new Guid(def)) == false)
            {
                var u = BusinessLogic.User.GetCurrent() ?? BusinessLogic.User.GetUser(0);

                var dtd = MakeNew(u, name, new Guid(def));
                var dataType = DataTypesResolver.Current.GetById(new Guid(id));
                if (dataType == null)
                    throw new NullReferenceException("Could not resolve a data type with id " + id);

                dtd.DataType = dataType;
                dtd.Save();

                //add prevalues
                foreach (XmlNode xmlPv in xmlData.SelectNodes("PreValues/PreValue"))
                {
                    var val = xmlPv.Attributes["Value"];

                    if (val != null)
                    {
                        var p = new PreValue(0, 0, val.Value);
                        p.DataTypeId = dtd.Id;
                        p.Save();
                    }
                }

                return dtd;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of all datatypedefinitions
        /// </summary>
        /// <returns>A list of all datatypedefinitions</returns>
        public static DataTypeDefinition[] GetAll()
        {
            var result = ApplicationContext.Current.Services.DataTypeService.GetAllDataTypeDefinitions();
            return result.Select(x => new DataTypeDefinition(x)).ToArray();
        }

        /// <summary>
        /// Creates a new datatypedefinition given its name and the user which creates it.
        /// </summary>
        /// <param name="u">The user who creates the datatypedefinition</param>
        /// <param name="Text">The name of the DataTypeDefinition</param>
        /// <returns></returns>
        public static DataTypeDefinition MakeNew(BusinessLogic.User u, string Text)
        {
            return MakeNew(u, Text, Guid.NewGuid());
        }

        /// <summary>
        /// Creates a new datatypedefinition given its name and the user which creates it.
        /// </summary>
        /// <param name="u">The user who creates the datatypedefinition</param>
        /// <param name="Text">The name of the DataTypeDefinition</param>
        /// <param name="UniqueId">Overrides the CMSnodes uniqueID</param>
        /// <returns></returns>
        public static DataTypeDefinition MakeNew(BusinessLogic.User u, string Text, Guid UniqueId)
        {
            var found = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionByName(Text);
            if (found != null)
            {
                throw new DuplicateNameException("A data type with the name " + Text + " already exists");
            }

            var created = new Umbraco.Core.Models.DataTypeDefinition("")
            {
                Name = Text,
                Key = UniqueId
            };

            ApplicationContext.Current.Services.DataTypeService.Save(created);

            var dtd = new DataTypeDefinition(created);
            dtd.OnNew(EventArgs.Empty);

            return dtd;
        }

        /// <summary>
        /// Retrieve a list of datatypedefinitions which share the same IDataType datatype
        /// </summary>
        /// <param name="DataTypeId">The unique id of the IDataType</param>
        /// <returns>A list of datatypedefinitions which are based on the IDataType specified</returns>
        public static DataTypeDefinition GetByDataTypeId(Guid DataTypeId)
        {
            var dfId = 0;
            // When creating a datatype and not saving it, it will be null, so we need this check
            foreach (var df in GetAll().Where(x => x.DataType != null))
            {
                if (df.DataType.Id == DataTypeId)
                {
                    dfId = df.Id;
                    break;
                }
            }

            return dfId == 0 ? null : new DataTypeDefinition(dfId);
        }

        /// <summary>
        /// Analyzes an object to see if its basetype is umbraco.editorControls.DefaultData
        /// </summary>
        /// <param name="Data">The Data object to analyze</param>
        /// <returns>True if the basetype is the DefaultData class</returns>
        public static bool IsDefaultData(object Data)
        {
            Type typeOfData = Data.GetType();

            while (typeOfData.BaseType != new Object().GetType())
            {
                typeOfData = typeOfData.BaseType;
            }

            return (typeOfData.FullName == "umbraco.cms.businesslogic.datatype.DefaultData");
        }

        public static DataTypeDefinition GetDataTypeDefinition(int id)
        {
            var found = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(id);
            return found == null ? null : new DataTypeDefinition(found);
        }

        [Obsolete("Use GetDataTypeDefinition(int id) instead", false)]
        public static DataTypeDefinition GetDataTypeDefinition(Guid id)
        {
            var found = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(id);
            return found == null ? null : new DataTypeDefinition(found);
        }
        #endregion

        #region Protected methods

        protected override void setupNode()
        {
            DataTypeItem = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(Id);
            if (DataTypeItem == null)
            {
                throw new ArgumentException("No dataType with id = " + this.Id.ToString() + " found");
            }

            //base.setupNode();

            //using (var dr = SqlHelper.ExecuteReader("select dbType, propertyEditorAlias from cmsDataType where nodeId = '" + this.Id.ToString() + "'"))
            //{
            //    if (dr.Read())
            //    {
            //        PropertyEditorAlias = dr.GetString("propertyEditorAlias");
            //        DbType = dr.GetString("dbType");
            //    }
            //    else
            //        throw new ArgumentException("No dataType with id = " + this.Id.ToString() + " found");
            //}

        }

        //protected void SetupNode(IDataTypeDefinition dtd)
        //{
        //    base.setupNode();

        //    using (var dr = SqlHelper.ExecuteReader("select dbType, propertyEditorAlias from cmsDataType where nodeId = '" + this.Id.ToString() + "'"))
        //    {
        //        if (dr.Read())
        //        {
        //            PropertyEditorAlias = dr.GetString("propertyEditorAlias");
        //            DbType = dr.GetString("dbType");
        //        }
        //        else
        //            throw new ArgumentException("No dataType with id = " + this.Id.ToString() + " found");
        //    }

        //}

        #endregion

        #region Events
        //EVENTS
        public delegate void SaveEventHandler(DataTypeDefinition sender, EventArgs e);
        public delegate void NewEventHandler(DataTypeDefinition sender, EventArgs e);
        public delegate void DeleteEventHandler(DataTypeDefinition sender, EventArgs e);

        /// <summary>
        /// Occurs when a data type is saved.
        /// </summary>
        public static event SaveEventHandler Saving;
        protected virtual void OnSaving(EventArgs e)
        {
            if (Saving != null)
                Saving(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(EventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        [Obsolete("This event is not used! Use the BeforeDelete or AfterDelete events")]
        public static event DeleteEventHandler Deleting;
        protected virtual void OnDeleting(EventArgs e)
        {
            if (Deleting != null)
                Deleting(this, e);
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public new static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public new static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        #endregion

    }
}