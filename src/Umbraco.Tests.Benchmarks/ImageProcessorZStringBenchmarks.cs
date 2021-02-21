using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class ImageProcessorZStringBenchmarks
    {
        private const string MediaPath = "/media/1005/img_0671.jpg";
        private  readonly ImageUrlGenerationOptions.CropCoordinates Crop = new ImageUrlGenerationOptions.CropCoordinates(0.58729977382575338m, 0.055768992440203169m, 0m, 0.32457553600198386m);
        private  readonly ImageUrlGenerationOptions.FocalPointPosition Focus1 = new ImageUrlGenerationOptions.FocalPointPosition(0.80827067669172936m, 0.96m);
        private  readonly ImageUrlGenerationOptions.FocalPointPosition Focus2 = new ImageUrlGenerationOptions.FocalPointPosition(0.41m, 0.4275m);
        private  readonly ImageProcessorImageUrlGenerator Generator = new ImageProcessorImageUrlGenerator();
        private  ImageUrlGenerationOptions Options;

        [IterationSetup]
        public void SetupIteration()
        {
            Options = new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus1, Width = 200, Height = 300, FurtherOptions = "&filter=comic&roundedcorners=radius-26|bgcolor-fff" };

        }
        [Benchmark(Baseline =true)]
        public void UseStringBuilder()
        {
            for (int i = 0; i < 1000; i++)
            {
                var urlString = GetImageUrl(Options);
            }
        }
        [Benchmark()]
        public void UseZStringBuilder()
        {
            for (int i = 0; i < 1000; i++)
            {
                var urlString = Generator.GetImageUrl(Options);
            }
        }

        #region SB
        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            if (options == null) return null;

            var imageProcessorUrl = new StringBuilder(options.ImageUrl ?? string.Empty);

            if (options.FocalPoint != null) AppendFocalPoint(imageProcessorUrl, options);
            else if (options.Crop != null) AppendCrop(imageProcessorUrl, options);
            else if (options.DefaultCrop) imageProcessorUrl.Append("?anchor=center&mode=crop");
            else
            {
                imageProcessorUrl.Append("?mode=").Append((options.ImageCropMode ?? "crop").ToLower());

                if (options.ImageCropAnchor != null) imageProcessorUrl.Append("&anchor=").Append(options.ImageCropAnchor.ToLower());
            }

            var hasFormat = options.FurtherOptions != null && options.FurtherOptions.InvariantContains("&format=");

            //Only put quality here, if we don't have a format specified.
            //Otherwise we need to put quality at the end to avoid it being overridden by the format.
            if (options.Quality != null && hasFormat == false) imageProcessorUrl.Append("&quality=").Append(options.Quality);
            if (options.HeightRatio != null) imageProcessorUrl.Append("&heightratio=").Append(options.HeightRatio.Value.ToString(CultureInfo.InvariantCulture));
            if (options.WidthRatio != null) imageProcessorUrl.Append("&widthratio=").Append(options.WidthRatio.Value.ToString(CultureInfo.InvariantCulture));
            if (options.Width != null) imageProcessorUrl.Append("&width=").Append(options.Width);
            if (options.Height != null) imageProcessorUrl.Append("&height=").Append(options.Height);
            if (options.UpScale == false) imageProcessorUrl.Append("&upscale=false");
            if (options.AnimationProcessMode != null) imageProcessorUrl.Append("&animationprocessmode=").Append(options.AnimationProcessMode);
            if (options.FurtherOptions != null) imageProcessorUrl.Append(options.FurtherOptions);

            //If furtherOptions contains a format, we need to put the quality after the format.
            if (options.Quality != null && hasFormat) imageProcessorUrl.Append("&quality=").Append(options.Quality);
            if (options.CacheBusterValue != null) imageProcessorUrl.Append("&rnd=").Append(options.CacheBusterValue);

            return imageProcessorUrl.ToString();
        }

        private void AppendFocalPoint(StringBuilder imageProcessorUrl, ImageUrlGenerationOptions options)
        {
            imageProcessorUrl.Append("?center=");
            imageProcessorUrl.Append(options.FocalPoint.Top.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.FocalPoint.Left.ToString(CultureInfo.InvariantCulture));
            imageProcessorUrl.Append("&mode=crop");
        }

        private void AppendCrop(StringBuilder imageProcessorUrl, ImageUrlGenerationOptions options)
        {
            imageProcessorUrl.Append("?crop=");
            imageProcessorUrl.Append(options.Crop.X1.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.Crop.Y1.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.Crop.X2.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.Crop.Y2.ToString(CultureInfo.InvariantCulture));
            imageProcessorUrl.Append("&cropmode=percentage");
        }
        #endregion
        // * Summary *

        //        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.19041
        //Intel Core i5-6300U CPU 2.40GHz(Skylake), 1 CPU, 4 logical and 2 physical cores
        // [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4220.0
        //  Job-FTOBUC : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4220.0

        //InvocationCount=1  UnrollFactor=1

        //            Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        //------------------ |---------:|----------:|----------:|---------:|------:|--------:|------------:|------------:|------------:|--------------------:|
        //  UseStringBuilder | 2.867 ms | 0.2656 ms | 0.7830 ms | 2.784 ms |  1.00 |    0.00 |           - |           - |           - |              952 KB |
        // UseZStringBuilder | 1.743 ms | 0.1417 ms | 0.3904 ms | 1.623 ms |  0.66 |    0.23 |           - |           - |           - |              360 KB |
    }
}
