using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "task", Namespace = "")]
    [ReadOnly(true)]
    public class TranslationTaskImportSummary
    {
        [DataMember(Name = "successCount")]
        [ReadOnly(true)]
        public int SuccessCount { get; set; }

        [DataMember(Name = "errorCount")]
        [ReadOnly(true)]
        public int ErrorCount { get; set; }

        [DataMember(Name = "outcome")]
        [ReadOnly(true)]
        public Dictionary<int, int> Outcome { get; set; } = new Dictionary<int, int>();

        public void Merge(TranslationTaskImportSummary other)
        {
            SuccessCount += other.SuccessCount;
            ErrorCount += other.ErrorCount;

            foreach (var kvp in other.Outcome)
            {
                Outcome.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
