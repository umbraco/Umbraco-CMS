using System;

namespace Umbraco.Core
{
    [Obsolete("This is no longer used and will be removed from the core in future versions")]
    public enum IconPickerBehaviour
    {
        /// <summary>
        /// Default umbraco behavior - show duplicates in files and sprites
        /// </summary>
        ShowDuplicates,

        /// <summary>
        /// If a file exists on disk with the same name as one in the sprite
        /// then the file on disk overrules the one in the sprite, the 
        /// sprite icon will not be shown
        /// </summary>
        HideSpriteDuplicates,

        /// <summary>
        /// If a file exists on disk with the same name as one in the sprite
        /// then the file in the sprite overrules the one on disk, the file
        /// on disk will be shown
        /// </summary>
        HideFileDuplicates
    }
}
