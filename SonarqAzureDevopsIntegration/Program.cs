
namespace Sonar2PBI
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //sonar2PBIexe -AzureDevopsApiToken:837123h3k17keh12ı371 -ProjectKey:"Arfllet-pk"

            const string projectKeyIdentifier = "-ProjectKey:";
            const string azureDevopsApiTokenIdentifier = "-AzureDevopsApiToken:";

            string projectKey = string.Empty;
            string azureDevopsApiToken = string.Empty;

            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (arg.StartsWith(azureDevopsApiTokenIdentifier))
                    {
                        azureDevopsApiToken = arg.Replace(azureDevopsApiTokenIdentifier, "").Trim();
                        continue;//next arg
                    }
                    if (arg.StartsWith(projectKeyIdentifier))
                        projectKey = arg.Replace(projectKeyIdentifier, "").Trim();
                }
            }

#if DEBUG
            projectKey = "YNA-PK";
            
#endif

            Orchestration client = new Orchestration();
            client.Execute(azureDevopsApiToken, projectKey);
        }
    }
}