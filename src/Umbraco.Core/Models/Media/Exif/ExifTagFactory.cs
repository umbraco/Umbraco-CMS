using System;

namespace Umbraco.Core.Media.Exif
{
    internal static class ExifTagFactory
    {
        #region Static Methods
        /// <summary>
        /// Returns the ExifTag corresponding to the given tag id.
        /// </summary>
        public static ExifTag GetExifTag(IFD ifd, ushort tagid)
        {
            return (ExifTag)(ifd + tagid);
        }

        /// <summary>
        /// Returns the tag id corresponding to the given ExifTag.
        /// </summary>
        public static ushort GetTagID(ExifTag exiftag)
        {
            IFD ifd = GetTagIFD(exiftag);
            return (ushort)((int)exiftag - (int)ifd);
        }

        /// <summary>
        /// Returns the IFD section containing the given tag.
        /// </summary>
        public static IFD GetTagIFD(ExifTag tag)
        {
            return (IFD)(((int)tag / 100000) * 100000);
        }

        /// <summary>
        /// Returns the string representation for the given exif tag.
        /// </summary>
        public static string GetTagName(ExifTag tag)
        {
            string name = Enum.GetName(typeof(ExifTag), tag);
            if (name == null)
                return "Unknown";
            else
                return name;
        }

        /// <summary>
        /// Returns the string representation for the given tag id.
        /// </summary>
        public static string GetTagName(IFD ifd, ushort tagid)
        {
            return GetTagName(GetExifTag(ifd, tagid));
        }

        /// <summary>
        /// Returns the string representation for the given exif tag including 
        /// IFD section and tag id.
        /// </summary>
        public static string GetTagLongName(ExifTag tag)
        {
            string ifdname = Enum.GetName(typeof(IFD), GetTagIFD(tag));
            string name = Enum.GetName(typeof(ExifTag), tag);
            if (name == null)
                name = "Unknown";
            string tagidname = GetTagID(tag).ToString();
            return ifdname + ": " + name + " (" + tagidname + ")";
        }
        #endregion
    }
}
