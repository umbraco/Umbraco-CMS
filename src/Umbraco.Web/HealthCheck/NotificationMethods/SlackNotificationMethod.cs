using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slack.Webhooks;
using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    [HealthCheckNotificationMethod("slack")]
    public class SlackNotificationMethod : NotificationMethodBase, IHealthCheckNotificatationMethod
    {
        public SlackNotificationMethod(bool enabled, bool failureOnly, HealthCheckNotificationVerbosity verbosity,
                string webHookUrl, string channel, string username)
            : base(enabled, failureOnly, verbosity)
        {
            WebHookUrl = webHookUrl;
            Channel = channel;
            Username = username;
        }

        public string WebHookUrl { get; set; }

        public string Channel { get; set; }

        public string Username { get; set; }

        public async Task SendAsync(HealthCheckResults results)
        {
            if (ShouldSend(results) == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(WebHookUrl) || string.IsNullOrEmpty(Channel) || string.IsNullOrEmpty(Username))
            {
                return;
            }

            var slackClient = new SlackClient(WebHookUrl);

            var icon = Emoji.Warning;
            if (results.AllChecksSuccessful)
            {
                icon = Emoji.WhiteCheckMark;
            }

            var successResults = results.GetResultsForStatus(StatusResultType.Success);
            var warnResults = results.GetResultsForStatus(StatusResultType.Warning);
            var errorResults = results.GetResultsForStatus(StatusResultType.Error);
            var infoResults = results.GetResultsForStatus(StatusResultType.Info);

            // todo construct Slack Message using Slack Attachments

            var attachments = new List<SlackAttachment>();

            // adding strings here so that they can be localised

            var checkStringPural = "Checks";
            var checkString = "Check";

            var totalPasses = successResults.Count + warnResults.Count + infoResults.Count;
            var messageText = string.Format("{0} Health Checks passed", totalPasses);

            if (successResults.Any())
            {
                var passedTitle = string.Format("{0} Health {1} Passed", successResults.Count, successResults.Count > 1 ? checkStringPural : checkString);

                var successAttachment = GenerateAttachment(successResults, "good", passedTitle);
                attachments.Add(successAttachment);
            }

            if (warnResults.Any())
            {
                var warnTitle = string.Format("{0} Health {1} Passed with Warnings", warnResults.Count, warnResults.Count > 1 ? checkStringPural : checkString);

                var warnAttachment = GenerateAttachment(warnResults, "warning", warnTitle);
                attachments.Add(warnAttachment);
            }

            if (infoResults.Any())
            {
                var infoTitle = string.Format("{0} Health {1} Passed with Info", infoResults.Count, infoResults.Count > 1 ? checkStringPural : checkString);

                var infoAttachment = GenerateAttachment(infoResults, "#439FE0", infoTitle);
                attachments.Add(infoAttachment);
            }

            if (errorResults.Any())
            {
                var errorTitle = string.Format("{0} Health {1} Failed", errorResults.Count, errorResults.Count > 1 ? checkStringPural : checkString);

                var errorAttachment = GenerateAttachment(errorResults, "danger", errorTitle);
                attachments.Add(errorAttachment);

                messageText = string.Format("{0} Health {1} failed and {2}", errorResults.Count, errorResults.Count > 1 ? checkStringPural : checkString, messageText);
            }

            var slackMessage = new SlackMessage
            {
                Channel = Channel,
                Attachments = attachments,
                IconEmoji = icon,
                Username = Username,
                Text = messageText
            };
            await slackClient.PostAsync(slackMessage);

        }

        private SlackAttachment GenerateAttachment(Dictionary<string, IEnumerable<HealthCheckStatus>> successResults, string color, string title)
        {
            var slackFields = new List<SlackField>();
            foreach (var result in successResults)
            {
                var resultsText = string.Empty;

                // only show details if Verbosity is detailed

                var shortText = true;

                if (Verbosity == HealthCheckNotificationVerbosity.Detailed)
                {
                    shortText = false;

                    foreach (var check in result.Value)
                    {
                        // if more than one result use bullets
                        if (result.Value.Count() > 1)
                        {
                            resultsText += "• ";
                        }

                        resultsText = resultsText + RemoveSimpleHtml(check.Message);

                        // if not last result as a new line
                        if (check != result.Value.Last())
                        {
                            resultsText = resultsText + Environment.NewLine;
                        }
                    }
                }
                slackFields.Add(new SlackField() { Title = result.Key, Value = resultsText, Short = shortText });
            }

            var slackAttachment = new SlackAttachment
            {
                Color = color,
                Title = title,
                Fields = slackFields
            };

            return slackAttachment;
        }

        private static string RemoveSimpleHtml(string html)
        {
            return html.Replace("<strong>", "")
                .Replace("</strong>", "")
                .Replace("<em>", "")
                .Replace("</em>", "");
        }
    }
}
