using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Sonar2PBI.Tests
{
    [TestClass()]
    public class AzureDevopsApiClientTests
    {
        [TestMethod()]
        public void GetTeamfieldvaluesTest()
        {
            Orchestration orchestration = new Orchestration();
            string token = orchestration.GetAzureDevopsApiAccessToken();

            string projectid = "99999999-39e2-4563-84fc-ad943f9f641b";
            string teamid = "99999999-d114-4d79-b322-e8441e852ce8";

            var root = AzureDevopsApiClient.GetTeamfieldvalues(token, projectid, teamid);
            Assert.IsNotNull(root);
                        
            var valuesFirstItem = root.values[0];
            Assert.IsTrue((bool)valuesFirstItem.includeChildren);
            Assert.IsFalse(string.IsNullOrWhiteSpace((string)root.defaultValue));

            //Bunları  Eğitim-Dokumantasyon adına bıraktım. *Apul
            //var field = root.field
            //string referenceName = field.referenceName
            //string url = field.url
            //string defaultValue = root.defaultValue
            //var valuesFirstItem = root.values[0]
            //bool  includeChildren = (bool)valuesFirstItem.includeChildren
        }


        [TestMethod()]
        public void GetTeamsTest()
        {
            Orchestration orchestration = new Orchestration();
            string token = orchestration.GetAzureDevopsApiAccessToken();

            TeamsJson teamsJson = AzureDevopsApiClient.GetTeams(token, "your-domainteams");
            Assert.IsTrue(teamsJson.Count > 0);

            string projectId = teamsJson.Teams[0].ProjectId.ToString();
            string teamsId = teamsJson.Teams[0].Id.ToString();

            TeamMemberJson teamMemberJson = AzureDevopsApiClient.GetTeamMembers(token, "your-domainteams", projectId, teamsId);
            Assert.IsTrue(teamMemberJson.Count > 0);

            IterationJson iterationJson = AzureDevopsApiClient.GetIterations(token, "your-domainteams", projectId, teamsId);
            Assert.IsTrue(iterationJson.Count > 0);
        }

        [TestMethod()]
        //[DataRow("BUG")]
        [DataRow("TASK")]
        //[DataRow("DEFECT")]
        public void CreatePBITest(string wiType)
        {
            string organization = "your-domainteams";
            string title = "[Sonar2PBI] Test amaçlı silebilisin";
            string areaPath = @"Projects\Yazılım Mimari ve Kalite";
            string iterationPath = @"Projects\Yazılım Mimari ve Kalite";
            string description = "description";
            string assignedTo = "";
            string issueSeverty = "CRITICAL";
            string issueType = "VULNERABILITY";
            string bugState = "New";

            Orchestration orchestration = new Orchestration();
            string token = orchestration.GetAzureDevopsApiAccessToken();

            var parentWorkItemUrl = "https://dev.azure.com/your-domainteams/99999999-39e2-4563-84fc-ad943f9f641b/_apis/wit/workItems/51686";

            var (httpResponseMessage, workItemId, workItemURL) = AzureDevopsApiClient.CreatePBI(token, organization, title, areaPath, iterationPath, wiType, bugState, description, assignedTo, issueSeverty, issueType, parentWorkItemUrl);
            
            Assert.AreEqual( System.Net.HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.IsTrue(workItemId > 0);
            Assert.IsFalse(string.IsNullOrEmpty(workItemURL));
        }
    }
}