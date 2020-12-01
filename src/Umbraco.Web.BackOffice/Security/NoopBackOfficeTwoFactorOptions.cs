namespace Umbraco.Web.BackOffice.Security
{
    public class NoopBackOfficeTwoFactorOptions : IBackOfficeTwoFactorOptions
    {
        public string GetTwoFactorView(string username) => null;
    }

}
