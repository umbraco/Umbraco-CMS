using System;

namespace Umbraco.Web.Media.Exif
{
    /// <summary>
    /// The exception that is thrown when the format of the JPEG/Exif file
    /// could not be understood.
    /// </summary>
    internal class NotValidExifFileException : Exception
    {
        public NotValidExifFileException()
            : base("Not a valid JPEG/Exif file.")
        {
            ;
        }

        public NotValidExifFileException(string message)
            : base(message)
        {
            ;
        }
    }

    /// <summary>
    /// The exception that is thrown when an invalid enum type is given to an
    /// ExifEnumProperty.
    /// </summary>
    internal class UnknownEnumTypeException : Exception
    {
        public UnknownEnumTypeException()
            : base("Unknown enum type.")
        {
            ;
        }

        public UnknownEnumTypeException(string message)
            : base(message)
        {
            ;
        }
    }
}
