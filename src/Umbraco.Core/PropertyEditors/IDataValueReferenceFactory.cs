﻿namespace Umbraco.Core.PropertyEditors
{
    public interface IDataValueReferenceFactory
    {
        /// <summary>
        /// Gets a value indicating whether the DataValueReference lookup supports a datatype (data editor).
        /// </summary>
        /// <param name="dataType">The datatype.</param>
        /// <returns>A value indicating whether the converter supports a datatype.</returns>
        bool IsForEditor(IDataEditor dataEditor);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDataValueReference GetDataValueReference();
    }
}
