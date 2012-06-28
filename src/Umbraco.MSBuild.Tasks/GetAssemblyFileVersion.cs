using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace Umbraco.MSBuild.Tasks
{
    public class GetAssemblyFileVersion : ITask
    {
        [Required]
        public string strFilePathAssemblyInfo { get; set; }
        
        [Output]
        public string strAssemblyFileVersion { get; set; }

        public bool Execute()
        {
            StreamReader streamreaderAssemblyInfo = null;
            Match matchVersion;
            Group groupVersion;
            string strLine;
            strAssemblyFileVersion = String.Empty;
            try
            {
                streamreaderAssemblyInfo = new StreamReader(strFilePathAssemblyInfo);
                while ((strLine = streamreaderAssemblyInfo.ReadLine()) != null)
                {
                    matchVersion = Regex.Match(strLine, @"(?:AssemblyFileVersion\("")(?<ver>(\d*)\.(\d*)(\.(\d*)(\.(\d*))?)?)(?:""\))", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
                    if (matchVersion.Success)
                    {
                        groupVersion = matchVersion.Groups["ver"];
                        if ((groupVersion.Success) && (!String.IsNullOrEmpty(groupVersion.Value)))
                        {
                            strAssemblyFileVersion = groupVersion.Value;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BuildMessageEventArgs args = new BuildMessageEventArgs(e.Message, string.Empty, "GetAssemblyFileVersion", MessageImportance.High);
                BuildEngine.LogMessageEvent(args);
            }
            finally { if (streamreaderAssemblyInfo != null) streamreaderAssemblyInfo.Close(); }
            return (true);
        }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }
    }
}
