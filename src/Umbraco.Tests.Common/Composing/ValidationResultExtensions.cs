using System;
using System.Text;
using Umbraco.Core;

namespace Umbraco.Tests.Common.Composing
{
    // These are used for Light Inject container validation
    public static class ValidationResultExtensions
    {
        public static string ToText(this ValidationResult result)
        {
            var text = new StringBuilder();

            text.AppendLine($"{result.Severity}: {WordWrap(result.Message, 120)}");
            var target = result.ValidationTarget;
            text.Append("\tsvce: ");
            text.Append(target.ServiceName);
            text.Append(target.DeclaringService.ServiceType);
            if (!target.DeclaringService.ServiceName.IsNullOrWhiteSpace())
            {
                text.Append(" '");
                text.Append(target.DeclaringService.ServiceName);
                text.Append("'");
            }

            text.Append("     (");
            if (target.DeclaringService.Lifetime == null)
                text.Append("Transient");
            else
                text.Append(target.DeclaringService.Lifetime.ToString().TrimStart("LightInject.").TrimEnd("Lifetime"));
            text.AppendLine(")");
            text.Append("\timpl: ");
            text.Append(target.DeclaringService.ImplementingType);
            text.AppendLine();
            text.Append("\tparm: ");
            text.Append(target.Parameter);
            text.AppendLine();

            return text.ToString();
        }

        private static string WordWrap(string text, int width)
        {
            int pos, next;
            var sb = new StringBuilder();
            var nl = Environment.NewLine;

            // Lucidity check
            if (width < 1)
                return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                var eol = text.IndexOf(nl, pos, StringComparison.Ordinal);

                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + nl.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        var len = eol - pos;

                        if (len > width)
                            len = BreakLine(text, pos, width);

                        if (pos > 0)
                            sb.Append("\t\t");
                        sb.Append(text, pos, len);
                        sb.Append(nl);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && char.IsWhiteSpace(text[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(nl); // Empty line
            }

            return sb.ToString();
        }

        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            var i = max - 1;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
    }
}
