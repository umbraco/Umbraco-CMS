using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;

namespace Umbraco.MSBuild.Tasks
{
    public class TimestampOffset : Task
    {
        [Required]
        public int Offset { get; set; }

        public ITaskItem[] Files { get; set; }

        public override bool Execute()
        {
            try
            {
                if (Files != null && Files.Length > 0)
                {
                    foreach (var file in Files)
                    {
                        if (File.Exists(file.ItemSpec))
                        {
                            var creationDate = File.GetCreationTimeUtc(file.ItemSpec);
                            var modifiedDate = File.GetLastWriteTimeUtc(file.ItemSpec);

                            File.SetCreationTimeUtc(file.ItemSpec, creationDate.AddHours(Offset));
                            File.SetLastWriteTimeUtc(file.ItemSpec, modifiedDate.AddHours(Offset));
                        }
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }
    }
}
