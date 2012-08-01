using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.editorControls.PickerRelations
{
    /// <summary>
    /// Data Class, used to store the configuration options for the PickerRelationsPreValueEditor
    /// </summary>
    internal class PickerRelationsOptions
    {
        /// <summary>
        /// Alias of the pickerProperty to get a csv value of IDs from //TODO: a known format for xml fragments would be good too
        /// </summary>
        public string PropertyAlias { get; set; }

        /// <summary>
        /// The Id of the RelationType to use 
        /// </summary>
        public int RelationTypeId { get; set; }

        /// <summary>
        /// only relevant with parent-child 
        /// </summary>
        public bool ReverseIndexing { get; set; }

		/// <summary>
		/// if true then the property is hidden
		/// </summary>
		public bool HideDataEditor { get; set; }

        /// <summary>
        /// Initializes an instance of PickerRelationsOptions
        /// </summary>
        public PickerRelationsOptions()
        {
            // Default values
            this.PropertyAlias = string.Empty;
            this.RelationTypeId = -1;
            this.ReverseIndexing = false;
			this.HideDataEditor = false;
        }
    }
}
