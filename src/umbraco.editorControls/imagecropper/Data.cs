using System;

namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    struct Crop
    {
        public int X;
        public int Y;
        public int X2;
        public int Y2;

        public Crop(int x, int y, int x2, int y2)
        {
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
        }
    }

    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    enum DefaultCropPosition
    {
        CenterCenter = 0,
        CenterTop,
        CenterBottom,
        LeftCenter,
        LeftTop,
        LeftBottom,
        RightCenter,
        RightTop,
        RightBottom
    }

    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    struct Preset
    {
        public string Name;
        public int TargetWidth;
        public int TargetHeight;
        public bool KeepAspect;
        public string PositionH;
        public string PositionV;

        public float Aspect
        {
            get { return (float)TargetWidth / TargetHeight; }
        }

        public Crop Fit(ImageInfo imageInfo)
        {
            Crop crop;

            if (Aspect >= imageInfo.Aspect)
            {
                // crop widest            hor    ver
                // relevant positioning: center top, center center, center bottom
                
                float h = ((float)imageInfo.Width / TargetWidth) * TargetHeight;
                
                crop.X = 0;
                crop.X2 = imageInfo.Width;

                switch(PositionV)
                {
                    case "T":
                        crop.Y = 0;
                        crop.Y2 = (int)h;
                        break;
                    case "B":
                        crop.Y = imageInfo.Height - (int)h;
                        crop.Y2 = imageInfo.Height;
                        break;
                    default: // CC
                        crop.Y = (int)(imageInfo.Height - h) / 2;
                        crop.Y2 = (int)(crop.Y + h);
                        break;
                }
            }
            else
            {

                // image widest
                // relevant positioning: left/right center, left/right top, left/right bottom
                
                float w = ((float)imageInfo.Height / TargetHeight) * TargetWidth;
                
                crop.Y = 0;
                crop.Y2 = imageInfo.Height;

                switch (PositionH)
                {
                    case "L":
                        crop.X = 0;
                        crop.X2 = (int)w;
                        break;
                    case "R":
                        crop.X = imageInfo.Width - (int)w;
                        crop.X2 = imageInfo.Width;
                        break;
                    default: // CC
                        crop.X = (int) (imageInfo.Width - w)/2;
                        crop.X2 = (int) (crop.X + w);
                        break;
                }

            }

            return crop;
        }

        public Preset(string name, int targetWidth, int targetHeight, bool keepAspect, string positionH, string positionV)
        {
            Name = name;
            TargetWidth = targetWidth;
            TargetHeight = targetHeight;
            KeepAspect = keepAspect;
            PositionH = positionH;
            PositionV = positionV;
        }

    }
}