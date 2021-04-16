namespace Umbraco.Cms.Core.Events
{
    public class CancelableNotification : StatefulNotification, ICancelableNotification
    {
        public bool Cancel { get; set; }
        public void CancelOperation()
        {
            Cancel = true;
        }
    }
}
