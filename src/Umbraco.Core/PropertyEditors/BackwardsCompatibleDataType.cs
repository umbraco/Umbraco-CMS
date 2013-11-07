using System;
using umbraco.interfaces;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// This is used purelty to attempt to maintain some backwards compatibility with new property editors that don't have a 
    /// legacy property editor predecessor when developers are using the legacy APIs
    /// </summary>
    internal class BackwardsCompatibleDataType : IDataType
    {
        public Guid Id { get; private set; }
        public string DataTypeName { get; private set; }
        public IData Data { get; private set; }
        public int DataTypeDefinitionId { get; set; }

        /// <summary>
        /// Creates a runtime instance
        /// </summary>
        /// <param name="propEdAlias"></param>
        /// <param name="legacyId"></param>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        internal static BackwardsCompatibleDataType Create(string propEdAlias, Guid legacyId, int dataTypeDefId)
        {
            var dt = new BackwardsCompatibleDataType
            {
                Id = legacyId,
                DataTypeName = propEdAlias,
                DataTypeDefinitionId = dataTypeDefId,
                Data = new BackwardsCompatibleData(propEdAlias)
            };

            return dt;
        }

        public IDataEditor DataEditor
        {
            get
            {
                throw new NotSupportedException(
                    typeof(IDataEditor)
                    + " is a legacy object and is not supported by runtime generated "
                    + typeof(IDataType)
                    + " instances to maintain backwards compatibility with the legacy APIs. Consider upgrading your code to use the new Services APIs.");
            }
        }
        public IDataPrevalue PrevalueEditor
        {
            get
            {
                throw new NotSupportedException(
                    typeof(IDataPrevalue)
                    + " is a legacy object and is not supported by runtime generated "
                    + typeof(IDataType)
                    + " instances to maintain backwards compatibility with the legacy APIs. Consider upgrading your code to use the new Services APIs.");
            }
        }
        
    }
}