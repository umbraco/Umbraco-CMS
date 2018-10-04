using System.Collections.Generic;
using Serilog.Events;

namespace Umbraco.Core.Logging.Viewer
{
    public class MessageTemplateFilter : ILogFilter
    {
        public Dictionary<string, int> counts = new Dictionary<string, int>();

        public bool TakeLogEvent(LogEvent e)
        {
            var templateText = e.MessageTemplate.Text;
            if (counts.TryGetValue(templateText, out var count))
            {
                count++;
            }
            else
            {
                count = 1;
            }

            counts[templateText] = count;

            //Don't add it to the list
            return false;
        }
    }
}
