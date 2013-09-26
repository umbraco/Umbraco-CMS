using System;
using System.Collections;

namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Config
    {
        public string UploadPropertyAlias { get; set; }
        public bool GenerateImages { get; set; }
        public int Quality { get; set; }
        public bool ShowLabel { get; set; }
        public ArrayList presets { get; set; }

        public Config(string configuration)
        {
            presets = new ArrayList();

            string[] configData = configuration.Split('|');

            if (configData.Length != 2) return;

            string[] generalSettings = configData[0].Split(',');

            UploadPropertyAlias = generalSettings[0];
            GenerateImages = generalSettings[1] == "1";
            ShowLabel = generalSettings[2] == "1";

            int _quality;
            if(generalSettings.Length >= 4 && Int32.TryParse(generalSettings[3], out _quality))
            {
                Quality = _quality;    
            }
            else
            {
                Quality = 90;
            }
            
            string[] presetData = configData[1].Split(';');

            for (int i=0; i < presetData.Length; i++)
            {
                string[] p = presetData[i].Split(',');

                int targetWidth, targetHeight;
                
                if (p.Length >= 4 && Int32.TryParse(p[1], out targetWidth) && Int32.TryParse(p[2], out targetHeight))
                {
                    char[] cropPosition = { 'C', 'M' };

                    if(p.Length >= 5)
                    {
                        cropPosition = p[4].ToCharArray();
                    }

                    presets.Add(new Preset(p[0], targetWidth, targetHeight, p[3] == "1" ? true : false, cropPosition[0].ToString(), cropPosition[1].ToString()));
                }
            }
        }
    }
}