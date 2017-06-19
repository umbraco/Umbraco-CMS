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

            var successResults = results.Results(StatusResultType.Success);
            var warnResults = results.Results(StatusResultType.Warning);
            var errorResults = results.Results(StatusResultType.Error);
            var infoResults = results.Results(StatusResultType.Info);

            // todo construct Slack Message using Slack Attachments

            var slackMessage = new SlackMessage
            {
                Channel = Channel,
                Text = results.ResultsAsMarkDown(Verbosity, true),
                IconEmoji = icon,
                Username = Username
            };
            await slackClient.PostAsync(slackMessage);
        }
    }
}
