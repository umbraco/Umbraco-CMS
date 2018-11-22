using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.interfaces;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// This is used purelty to attempt to maintain some backwards compatibility with new property editors that don't have a 
    /// legacy property editor predecessor when developers are using the legacy APIs
    /// </summary>
    internal class BackwardsCompatibleData : IData, IDataValueSetter
    {
        private readonly string _propertyEditorAlias;
        private bool _valueLoaded = false;
        private object _value;

        public BackwardsCompatibleData(string propertyEditorAlias)
        {
            _propertyEditorAlias = propertyEditorAlias;
        }

        public int PropertyId { set; get; }

        /// <summary>
        /// This returns the value
        /// </summary>
        /// <remarks>
        /// There's code here to load the data from the db just like the legacy DefaultData does but in theory the value of this
        /// IData should always be set using the IDataValueSetter.SetValue which is done externally. Just in case there's some edge
        /// case out there that doesn't set this value, we'll go and get it based on the same logic in DefaultData.
        /// </remarks>
        public virtual object Value
        {
            get
            {
                //Lazy load the value when it is required.
                if (_valueLoaded == false)
                {
                    LoadValueFromDatabase();
                    _valueLoaded = true;
                }
                return _value;
            }
            set
            {
                _value = value;
                _valueLoaded = true;
            }
        }


        public XmlNode ToXMl(XmlDocument data)
        {
            //we'll get the property editor by alias, if it exists (which it absolutely should), then we'll have to create a 
            // fake 'Property' object and pass it to the ConvertDbToXml method so we can get the correct XML fragment that 
            // it needs to make.
            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(_propertyEditorAlias);
            if (propertyEditor != null)
            {
                //create a 'fake' property - we will never know the actual db type here so we'll just make it nvarchar, this shouldn't
                // make any difference for the conversion process though.
                var property = new Property(new PropertyType(_propertyEditorAlias, DataTypeDatabaseType.Nvarchar))
                    {
                        Id = PropertyId,
                        Value = Value
                    };
                var xd = new XmlDocument();
                var xNode = propertyEditor.ValueEditor.ConvertDbToXml(property, property.PropertyType, ApplicationContext.Current.Services.DataTypeService);
                
                //check if this xml fragment can be converted to an XmlNode
                var xContainer = xNode as XContainer;
                if (xContainer != null)
                {
                    //imports to the document
                    xContainer.GetXmlNode(xd);
                    // return the XML node.
                    return data.ImportNode(xd.DocumentElement, true);
                }

                return ReturnCDataElement(data);
            }

            //if for some reason the prop editor wasn't found we'll default to returning the string value in a CDATA block.
            return ReturnCDataElement(data);
        }

        private XmlNode ReturnCDataElement(XmlDocument doc)
        {
            var sValue = Value != null ? Value.ToString() : string.Empty;
            return doc.CreateCDataSection(sValue);
        }
        
        public void MakeNew(int propertyId)
        {
            //DO nothing
        }

        public void Delete()
        {
            throw new NotSupportedException(
                    typeof(IData)
                    + " is a legacy object and is not supported by runtime generated "
                    + " instances to maintain backwards compatibility with the legacy APIs. Consider upgrading your code to use the new Services APIs.");
        }

        /// <summary>
        /// This is here for performance reasons since in some cases we will have already resolved the value from the db
        /// and want to just give this object the value so it doesn't go re-look it up from the database.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="strDbType"></param>
        void IDataValueSetter.SetValue(object val, string strDbType)
        {            
            _value = val;
            _valueLoaded = true;
        }

        /// <summary>
        /// In the case where the value is not set, this will go get it from the db ourselves - this shouldn't really ever be needed,
        /// the value should always be set with IDataValueSetter.SetValue
        /// </summary>
        private void LoadValueFromDatabase()
        {
            var sql = new Sql();
            sql.Select("*")
               .From<PropertyDataDto>()
               .InnerJoin<PropertyTypeDto>()
               .On<PropertyTypeDto, PropertyDataDto>(x => x.Id, y => y.PropertyTypeId)
               .InnerJoin<DataTypeDto>()
               .On<DataTypeDto, PropertyTypeDto>(x => x.DataTypeId, y => y.DataTypeId)
               .Where<PropertyDataDto>(x => x.Id == PropertyId);
            var dto = ApplicationContext.Current.DatabaseContext.Database.Fetch<PropertyDataDto, PropertyTypeDto, DataTypeDto>(sql).FirstOrDefault();

            if (dto != null)
            {
                //get the value for the data type, if null, set it to an empty string
                _value = dto.GetValue;
            }
        }
    }
}