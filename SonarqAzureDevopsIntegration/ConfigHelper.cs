namespace Sonar2PBI
{
    public static class ConfigHelper
    {
        public static string KAPConnectionString
        {
            get
            {
                string hashed = System.Configuration.ConfigurationManager.ConnectionStrings["KAPConnectionString"].ConnectionString;
                string conn = CryptoHelper.Decrypt(hashed, "your-crypyto-salt-here");
                return conn;
            }
        }
        //app settings
        public static string SonarServerURL { get { return System.Configuration.ConfigurationManager.AppSettings["SonarServerURL"]; } }
        public static string SonarServerToken { get { return System.Configuration.ConfigurationManager.AppSettings["SonarServerToken"]; } }
        public static string AzureDevopsToken { get { return System.Configuration.ConfigurationManager.AppSettings["AzureDevopsToken"]; } }
        public static string SmtpServer { get { return System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]; } }
        public static string FromAdress { get { return System.Configuration.ConfigurationManager.AppSettings["FromAdress"]; } }
        public static string ToAdress { get { return System.Configuration.ConfigurationManager.AppSettings["ToAdress"]; } }
        public static string SubjectPrefix { get { return System.Configuration.ConfigurationManager.AppSettings["SubjectPrefix"]; } }
        
        public static string TitlePrefix { get { return System.Configuration.ConfigurationManager.AppSettings["TitlePrefix"]; } }
        public static string Company { get { return System.Configuration.ConfigurationManager.AppSettings["Company"]; } }
        public static string DescriptionIssueSonarLink { get { return System.Configuration.ConfigurationManager.AppSettings["DescriptionIssueSonarLink"]; } }
    }
}