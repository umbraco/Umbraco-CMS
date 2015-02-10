using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Media.Exif
{
    internal class JpegInfo
    {
        /// <summary>
        /// The Jpeg file name (excluding path).
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// The Jpeg file size, in bytes.
        /// </summary>
        public int FileSize
        {
            get;
            set;
        }

        /// <summary>
        /// True if the provided Stream was detected to be a Jpeg image, False otherwise.
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Image height in pixels.
        /// </summary>
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// Image width in pixels.
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// True if the image data is expressed in 3 components (RGB), False otherwise.
        /// </summary>
        public bool IsColor
        {
            get;
            set;
        }

        /// <summary>
        /// Orientation of the image.
        /// </summary>
        public ExifOrientation Orientation
        {
            get;
            set;
        }

        /// <summary>
        /// The X resolution of the image, expressed in ResolutionUnit.
        /// </summary>
        public double XResolution
        {
            get;
            set;
        }

        /// <summary>
        /// The Y resolution of the image, expressed in ResolutionUnit.
        /// </summary>
        public double YResolution
        {
            get;
            set;
        }

        /// <summary>
        /// Resolution unit of the image.
        /// </summary>
        public ExifUnit ResolutionUnit
        {
            get;
            set;
        }

        /// <summary>
        /// Date at which the image was taken.
        /// </summary>
        public string DateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Date at which the image was taken. Created by Lumia devices.
        /// </summary>
        public string DateTimeOriginal
        {
            get;
            set;
        }

        /// <summary>
        /// Description of the image.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Camera manufacturer.
        /// </summary>
        public string Make
        {
            get;
            set;
        }

        /// <summary>
        /// Camera model.
        /// </summary>
        public string Model
        {
            get;
            set;
        }

        /// <summary>
        /// Software used to create the image.
        /// </summary>
        public string Software
        {
            get;
            set;
        }

        /// <summary>
        /// Image artist.
        /// </summary>
        public string Artist
        {
            get;
            set;
        }

        /// <summary>
        /// Image copyright.
        /// </summary>
        public string Copyright
        {
            get;
            set;
        }

        /// <summary>
        /// Image user comments.
        /// </summary>
        public string UserComment
        {
            get;
            set;
        }

        /// <summary>
        /// Exposure time, in seconds.
        /// </summary>
        public double ExposureTime
        {
            get;
            set;
        }

        /// <summary>
        /// F-number (F-stop) of the camera lens when the image was taken.
        /// </summary>
        public double FNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Flash settings of the camera when the image was taken.
        /// </summary>
        public ExifFlash Flash
        {
            get;
            set;
        }

        /// <summary>
        /// GPS latitude reference (North, South).
        /// </summary>
        public ExifGpsLatitudeRef GpsLatitudeRef
        {
            get;
            set;
        }

        /// <summary>
        /// GPS latitude (degrees, minutes, seconds).
        /// </summary>
        public double[] GpsLatitude = new double[3];

        /// <summary>
        /// GPS longitude reference (East, West).
        /// </summary>
        public ExifGpsLongitudeRef GpsLongitudeRef
        {
            get;
            set;
        }

        /// <summary>
        /// GPS longitude (degrees, minutes, seconds).
        /// </summary>
        public double[] GpsLongitude = new double[3];

        /// <summary>
        /// Byte offset of the thumbnail data within the Exif section of the image file.
        /// Used internally.
        /// </summary>
        public int ThumbnailOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Byte size of the thumbnail data within the Exif section of the image file.
        /// Used internally.
        /// </summary>
        public int ThumbnailSize
        {
            get;
            set;
        }

        /// <summary>
        /// Thumbnail data found in the Exif section.
        /// </summary>
        public byte[] ThumbnailData
        {
            get;
            set;
        }

        /// <summary>
        /// Time taken to load the image information.
        /// </summary>
        public TimeSpan LoadTime
        {
            get;
            set;
        }
    }
}
