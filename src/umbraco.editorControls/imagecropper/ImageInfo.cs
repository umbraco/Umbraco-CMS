using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using Umbraco.Core.IO;
//using Umbraco.Core.IO;


namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ImageInfo
    {
        public Image image { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Aspect { get; set; }
        public DateTime DateStamp { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }

        private readonly MediaFileSystem _fs;

        public ImageInfo(string relativePath)
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            try
            {
                RelativePath = relativePath;

                //This get's the IFileSystem's path based on the URL (i.e. /media/blah/blah.jpg )
                Path = _fs.GetRelativePath(relativePath);

                using (var stream = _fs.OpenFile(Path))                
                using (image = Image.FromStream(stream))
                {
                    var fileName = _fs.GetFileName(Path);
                    Name = fileName.Substring(0, fileName.LastIndexOf('.'));

                    DateStamp = _fs.GetLastModified(Path).Date;
                    Width = image.Width;
                    Height = image.Height;
                    Aspect = (float)Width / Height;        
                }
                
            }
            catch (Exception)
            {
                Width = 0;
                Height = 0;
                Aspect = 0;
            }

        }

        public bool Exists
        {
            get { return Width > 0 && Height > 0; }
        }

        //public string Directory
        //{
        //    get { return Path.Substring(0, Path.LastIndexOf('\\')); }
        //}

        public void GenerateThumbnails(SaveData saveData, Config config)
        {
            if (config.GenerateImages)
            {
                for (int i = 0; i < config.presets.Count; i++)
                {
                    Crop crop = (Crop)saveData.data[i];
                    Preset preset = (Preset)config.presets[i];

                    // Crop rectangle bigger than actual image
                    if (crop.X2 - crop.X > Width || crop.Y2 - crop.Y > Height)
                    {
                        crop = preset.Fit(this);
                    }
                    
                    ImageTransform.Execute(
                        Path,
                        String.Format("{0}_{1}", Name, preset.Name),
                        crop.X,
                        crop.Y,
                        crop.X2 - crop.X,
                        crop.Y2 - crop.Y,
                        preset.TargetWidth,
                        preset.TargetHeight,
                        config.Quality
                        );
                    //BasePage bp = new BasePage();
                    //bp.speechBubble(BasePage.speechBubbleIcon.error, "Error",
                    //                "One or more crops are out of bounds. Please correct and try again.");
                }
            }
        }
    }
}