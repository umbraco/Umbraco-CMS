﻿using System.Collections.Generic;
using Serilog.Events;

namespace Umbraco.Core.Logging.Viewer
{
    internal class MessageTemplateFilter : ILogFilter
    {
        public readonly Dictionary<string, int> Counts = new Dictionary<string, int>();

        public bool TakeLogEvent(LogEvent e)
        {
            var templateText = e.MessageTemplate.Text;
            if (Counts.TryGetValue(templateText, out var count))
            {
                count++;
            }
            else
            {
                count = 1;
            }

            Counts[templateText] = count;

            //Don't add it to the list
            return false;
        }
    }
}
