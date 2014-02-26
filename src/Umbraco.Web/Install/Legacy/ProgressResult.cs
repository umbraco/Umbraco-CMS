namespace Umbraco.Web.Install
{
    internal class ProgressResult
    {
        public string Error { get; set; }
        public int Percentage { get; set; }
        public string Description { get; set; }
        public ProgressResult()
        {

        }

        public ProgressResult(int percentage, string description, string error)
        {
            Percentage = percentage;
            Description = description;
            Error = error;
        }

    }
}