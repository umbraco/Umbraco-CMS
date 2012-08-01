using System;
using System.Drawing;
using System.IO;
using System.Web;
using umbraco.editorControls.imagecropper;
using umbraco.IO;

namespace umbraco.editorControls.imagecropper
{
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

        public ImageInfo(string relativePath)
        {
            RelativePath = relativePath;
            Path = IOHelper.MapPath(relativePath);
            if (File.Exists(Path))
            {
                string fileName = Path.Substring(Path.LastIndexOf('\\') + 1);
                Name = fileName.Substring(0, fileName.LastIndexOf('.'));

                byte[] buffer = null;

                using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int) fs.Length);
                    fs.Close();
                }

                try
                {
                    image = Image.FromStream(new MemoryStream(buffer));

                    Width = image.Width;
                    Height = image.Height;
                    Aspect = (float) Width/Height;
                    DateStamp = File.GetLastWriteTime(Path);
                }
                catch (Exception)
                {
                    Width = 0;
                    Height = 0;
                    Aspect = 0;
                }

            }
            else
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

        public string Directory
        {
            get { return Path.Substring(0, Path.LastIndexOf('\\')); }
        }

        public void GenerateThumbnails(SaveData saveData, Config config)
        {
            if (config.GenerateImages)
            {
                for (int i = 0; i < config.presets.Count; i++)
                {
                    Crop crop = (Crop) saveData.data[i];
                    Preset preset = (Preset) config.presets[i];

                    // Crop rectangle bigger than actual image
                    if(crop.X2 - crop.X > Width || crop.Y2 - crop.Y > Height)
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