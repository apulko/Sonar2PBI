using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Sonar2PBI
{
    public static class AzureDevopsApiClient
    {


        private static HttpResponseMessage CallGetApi(string accessToken, HttpClient client, string requestUrl)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", accessToken))));
            var request = new HttpRequestMessage(new HttpMethod("GET"), requestUrl);
            HttpResponseMessage responseMessage = client.SendAsync(request).Result;

            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                LogHelper.Logla(new HttpRequestException(responseMessage.ToString().Substring(0, 100)));

            }
            return responseMessage;
        }

        public static TeamsJson GetTeams(string accessToken, string organization)
        {
            string requestUrl = $"https://dev.azure.com/{organization}/_apis/teams?api-version=6.0-preview.3";

            HttpClientHandler _httpclienthndlr = new HttpClientHandler();

            using (HttpClient client = new HttpClient(_httpclienthndlr))
            {

                HttpResponseMessage responseMessage = CallGetApi(accessToken, client, requestUrl);
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    TeamsJson teamsJson = Newtonsoft.Json.JsonConvert.DeserializeObject<TeamsJson>(responseMessage.Content.ReadAsStringAsync().Result);
                    //var sorgum = teamsJson.Teams.Where(x => x.Name.Contains("Arma")).Select(x => x)
                    return teamsJson;
                }
            }
            return new TeamsJson();
        }

        public static TeamMemberJson GetTeamMembers(string accessToken, string organization, string projectId, string teamId)
        {
            string requestUrl = $"https://dev.azure.com/{organization}/_apis/projects/{projectId}/teams/{teamId}/members?api-version=6.0";

            HttpClientHandler _httpclienthndlr = new HttpClientHandler();

            using (HttpClient client = new HttpClient(_httpclienthndlr))
            {
                HttpResponseMessage responseMessage = CallGetApi(accessToken, client, requestUrl);
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    TeamMemberJson teamMemberJson = Newtonsoft.Json.JsonConvert.DeserializeObject<TeamMemberJson>(responseMessage.Content.ReadAsStringAsync().Result);
                    return teamMemberJson;
                }
            }
            return new TeamMemberJson();
        }

        public static dynamic GetTeamfieldvalues(string accessToken, string projectId, string teamId)
        {
            string requestUrl = $"https://dev.azure.com/your-domainteams/{projectId}/{teamId}/_apis/work/teamsettings/teamfieldvalues";

            HttpClientHandler _httpclienthndlr = new HttpClientHandler();

            using (HttpClient client = new HttpClient(_httpclienthndlr))
            {
                HttpResponseMessage responseMessage = CallGetApi(accessToken, client, requestUrl);
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var jsonstr = responseMessage.Content.ReadAsStringAsync().Result;
                    dynamic root = JsonConvert.DeserializeObject(jsonstr);
                    return root;
                }
                
            }
            return null;
        }
            public static IterationJson GetIterations(string accessToken, string organization, string projectId, string teamId)
        {
            string requestUrl = $"https://dev.azure.com/{organization}/{projectId}/{teamId}/_apis/work/teamsettings/iterations?api-version=6.0";

            HttpClientHandler _httpclienthndlr = new HttpClientHandler();

            using (HttpClient client = new HttpClient(_httpclienthndlr))
            {
                HttpResponseMessage responseMessage = CallGetApi(accessToken, client, requestUrl);
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    IterationJson iterationJson = Newtonsoft.Json.JsonConvert.DeserializeObject<IterationJson>(responseMessage.Content.ReadAsStringAsync().Result);
                    return iterationJson;
                }
            }
            return new IterationJson();
        }


        public static (HttpResponseMessage httpResponseMessage, long workItemId, string workItemURL) CreatePBI(string personalAccessToken, string organization, string title, string areaPath, string iterationPath, string wiType, string bugState, string description, string assignedTo, string issueSeverty, string issueType)
        {
            string parentWorkItemUrl = string.Empty;
            return CreatePBI(personalAccessToken, organization, title, areaPath, iterationPath, wiType, bugState, description, assignedTo, issueSeverty, issueType, parentWorkItemUrl);
        }

        public static (HttpResponseMessage httpResponseMessage, long workItemId, string workItemURL) CreatePBI(string personalAccessToken, string organization, string title, string areaPath, string iterationPath, string wiType, string bugState, string description, string assignedTo, string issueSeverty, string issueType, string parentWorkItemUrl)
        {
            string apiversion = "6.0";

            long workItemId = 0;
            string workItemURL = string.Empty;

            bool isBug = (wiType.Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture) == "bug");

            string requestUrl = $"https://dev.azure.com/{organization}/projects/_apis/wit/workitems/${wiType}?api-version={apiversion}";

            string tags = isBug ? "Sonar2PBI" : $"{issueSeverty};{issueType}";

            try
            {
                List<Object> flds = new List<Object>
                    {
                    new { op = "add", path = "/fields/System.AssignedTo", value = assignedTo },
                    new { op = "add", path = "/fields/System.Title", value = title },
                    new { op = "add", path = "/fields/System.AreaPath", value = areaPath },
                    new { op = "add", path = "/fields/System.IterationPath", value = iterationPath },
                    new { op = "add", path = "/fields/System.Tags", value = tags },
                    new { op = "add", path = "/fields/Custom.Company", value = ConfigHelper.Company }
                    };

                if (isBug)
                {
                    flds.Add(new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = description });

                    if (!bugState.IsNullOrWhiteSpace())
                        flds.Add(new { op = "add", path = "/fields/System.State", value = bugState });
                }
                else
                    flds.Add(new { op = "add", path = "/fields/System.Description", value = description });

                if (!string.IsNullOrEmpty(parentWorkItemUrl))
                {
                    List<Object> relationFields = new List<Object>
                    {
                    new {
                        rel = "System.LinkTypes.Hierarchy-Reverse" ,
                        url= parentWorkItemUrl,
                        attributes = new { name= "Parent", isLocked=false }
                        }
                    };
                    flds.Add(new { op = "add", path = "/relations/-", value = relationFields[0] });

                }

                string postContent = JsonConvert.SerializeObject(flds);

                HttpClientHandler _httpclienthndlr = new HttpClientHandler();

                using (HttpClient client = new HttpClient(_httpclienthndlr))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken))));

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl)
                    {
                        Content = new StringContent(postContent, Encoding.UTF8, "application/json-patch+json")
                    };

                    HttpResponseMessage responseMessage = client.SendAsync(request).Result;

                    if (responseMessage.StatusCode.ToString() != "OK")
                    {
                        LogHelper.Logla(new HttpRequestException(), requestUrl + " " + postContent);
                    }
                    else
                    {
                        var json = responseMessage.Content.ReadAsStringAsync().Result;
                        JObject jWorkItemObj = JObject.Parse(json);
                        workItemId = long.Parse(jWorkItemObj.Root["id"].ToString());
                        workItemURL = jWorkItemObj.Root["url"].ToString();
                    }

                    return (responseMessage, workItemId, workItemURL);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logla(ex, requestUrl);
                return (null, workItemId, workItemURL);
            }
        }
    }
}