using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// When assigned to a DataEditor it indicates that the values it generates can be compressed
    /// </summary>
    /// <remarks>
    /// Used in conjunction with <see cref="CompressedStoragePropertyEditorCompressionOptions"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CompressedStorageAttribute : Attribute
    {
        public CompressedStorageAttribute(bool isCompressed = true)
        {
            IsCompressed = isCompressed;
        }

        public bool IsCompressed { get; }
    }
}
