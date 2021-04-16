namespace Umbraco.Cms.Core.Events
{
    public class CancelableNotification : ICancelableNotification
    {
        public bool Cancel { get; set; }
        public void CancelOperation()
        {
            Cancel = true;
        }
    }
}
