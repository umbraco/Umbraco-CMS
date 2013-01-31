using System;
using System.IO;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Css
{
    internal static class CssCompactor
    {
        #region CssCompactor.Options

        [Flags]
        public enum Options
        {
            None = 0x00,
            PrettyPrint = 0x01,
            Overwrite = 0x02
        }

        #endregion CssCompactor.Options

        #region Public Methods

        internal static List<ParseException> Compact(string inputFile, string outputFile, string copyright, string timeStamp, CssCompactor.Options options)
        {
            if (!System.IO.File.Exists(inputFile))
            {
                throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", inputFile), inputFile);
            }

            if ((options & CssCompactor.Options.Overwrite) == 0x0 && System.IO.File.Exists(outputFile))
            {
                throw new AccessViolationException(String.Format("File (\"{0}\") already exists.", outputFile));
            }

            if (inputFile.Equals(outputFile, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("Input and output file are set to the same path.");
            }

            FileUtility.PrepSavePath(outputFile);
            using (TextWriter output = System.IO.File.CreateText(outputFile))
            {
                return CssCompactor.Compact(inputFile, null, output, copyright, timeStamp, options);
            }
        }

        internal static List<ParseException> Compact(string inputFile, string inputSource, TextWriter output, string copyright, string timeStamp, CssCompactor.Options options)
        {
            if (output == null)
            {
                throw new NullReferenceException("Output TextWriter was null.");
            }

            // write out header with copyright and timestamp
            CssCompactor.WriteHeader(output, copyright, timeStamp);

            // verify, compact and write out results
            CssParser parser = new CssParser(inputFile, inputSource);
            parser.Write(output, options);

            // return any errors
            return parser.Errors;
        }

        #endregion Public Methods

        #region Private Methods

        private static void WriteHeader(TextWriter writer, string copyright, string timeStamp)
        {
            if (!String.IsNullOrEmpty(copyright) || !String.IsNullOrEmpty(timeStamp))
            {
                int width = 6;
                if (!String.IsNullOrEmpty(copyright))
                {
                    copyright = copyright.Replace("*/", "");// make sure not to nest commments
                    width = Math.Max(copyright.Length + 6, width);
                }
                if (!String.IsNullOrEmpty(timeStamp))
                {
                    timeStamp = DateTime.Now.ToString(timeStamp).Replace("*/", "");// make sure not to nest commments
                    width = Math.Max(timeStamp.Length + 6, width);
                }

                writer.WriteLine("/*".PadRight(width, '-') + "*\\");

                if (!String.IsNullOrEmpty(copyright))
                {
                    writer.WriteLine("\t" + copyright);
                }

                if (!String.IsNullOrEmpty(timeStamp))
                {
                    writer.WriteLine("\t" + timeStamp);
                }

                writer.WriteLine("\\*".PadRight(width, '-') + "*/");
            }
        }

        #endregion Private Methods
    }
}