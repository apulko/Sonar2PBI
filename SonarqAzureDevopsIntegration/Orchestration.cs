using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;


namespace Sonar2PBI
{
    public class Orchestration
    {
        public TeamsJson TeamCollection { get; set; } = null;
        public IEnumerable TeamFilteredCollection { get; set; } = null;

        private Hashtable userTeamHastable = null;
        public Hashtable UserTeamHastable { get => userTeamHastable; set => userTeamHastable = value; }

        private Hashtable parentWIHashTable = null;
        public Hashtable ParentWIHashTable { get => parentWIHashTable; set => parentWIHashTable = value; }



        private Hashtable userIterationHastable = null;
        public Hashtable UserIterationHastable { get => userIterationHastable; set => userIterationHastable = value; }

        //APPLICATIPN ARGUMENTS
        public string AzureDevopsApiToken { get; set; } = string.Empty;
        public string ProjectKey { get; set; } = string.Empty;
        
        int hataAdedi = 0;
        public int HataAdedi { get => hataAdedi; set => hataAdedi = value; }

        public void Execute(string _azureDevopsApiToken, string _projectKey)
        {
            //application parameters initialize
            if (!string.IsNullOrWhiteSpace(_azureDevopsApiToken))
                AzureDevopsApiToken = _azureDevopsApiToken;

            if (!string.IsNullOrWhiteSpace(_projectKey))
                ProjectKey = _projectKey;

            string errorMessage = string.Empty;
            try
            {
                //Healty Check 
                IsAllConsumingAPIsHealty();
                DbHelper dbHelper = new DbHelper();
                List<Project> projects = dbHelper.GetProjects(ProjectKey);
                int projectCount = projects.Count;

                if (projectCount > 0)
                {
                    TeamCollection = AzureDevopsApiClient.GetTeams(GetAzureDevopsApiAccessToken(), "your-domainteams");

                    for (int i = 0; i < projectCount; i++)
                    {
                        var project = projects[i];

                        if (!string.IsNullOrWhiteSpace(ProjectKey) && project.ProjectKey != ProjectKey)
                            continue;

                        Process(project);
                    }
                }
            }
            catch (Exception logExceptionProgram)
            {
                LogHelper.Logla(logExceptionProgram);
                errorMessage = logExceptionProgram.Message;
            }

            string symbol = (string.IsNullOrWhiteSpace(errorMessage)) ? "✅" : "❌";

            SmtpHelper.EpostaGonder(GetSummary(errorMessage), string.Concat(ConfigHelper.SubjectPrefix, " ", symbol), ConfigHelper.ToAdress);
        }
        private string TakePart(string source, int len)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            if (source.Length > len)
                return source.Substring(0, len) + "...";

            return source;
        }

        private string GetSummary(string errorMessage)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append($"<br><b>ProjectKey: </b>{ProjectKey}");
                sb.Append($"<br><b>Machine Name: </b>{Environment.MachineName}");
                sb.Append($"<br><b>Path: </b>{Environment.CurrentDirectory}");
                sb.Append($"<br><b>Token: </b>{TakePart(AzureDevopsApiToken, 10)}");
                sb.Append($"<br>--------");
                var appKeys = (ConfigurationManager.AppSettings).AllKeys;
                foreach (var appKey in appKeys)
                    sb.AppendLine("<br><b>" + appKey + "</b> " + (appKey.ToLower(CultureInfo.InvariantCulture).Contains("token") ? TakePart(ConfigurationManager.AppSettings[appKey], 10) : ConfigurationManager.AppSettings[appKey]));

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    sb.Append("<br>--------");
                    sb.Append(errorMessage);
                }
            }
            catch (Exception)
            {
                //bu noktadaki exp. programı kesmemelidir.
            }
            return sb.ToString();
        }

        private void IsAllConsumingAPIsHealty()
        {
            KapConnectionHealtyCheck();
            AzureDevopsApiHealtyCheck();
            SonarqubeApiHealtyCheck();
        }

        private void KapConnectionHealtyCheck()
        {
            var dbHelper = new DbHelper();
            int projectCount = dbHelper.GetProjectCount();
            if (projectCount == 0)
                throw new KapConnectionHealtyCheckException();
        }

        private void AzureDevopsApiHealtyCheck()
        {
            var teamCollection = AzureDevopsApiClient.GetTeams(GetAzureDevopsApiAccessToken(), "your-domainteams");
            if (teamCollection.Count == 0)
                throw new AzureDevopsApiHealtyCheckException();
        }

        private static void SonarqubeApiHealtyCheck()
        {
            const string sampleRuleKey = "csharpsquid:S1698";
            SonarApiClient sonarApiClient = new SonarApiClient();
            //S2971 is not special. Just consuming an sonarqube api.
            RuleShowApiResponse response = sonarApiClient.ShowRule(sampleRuleKey);
            if (response == null)
                throw new SonarqubeApiHealtyCheckException();
        }

        private void ClearPerProjectGlobals()
        {
            ParentWIHashTable = new Hashtable();
            UserTeamHastable = new Hashtable();
            UserIterationHastable = new Hashtable();
            HataAdedi = 0;
        }

        private void Process(Project project)
        {
            if (!project.IsSonar2PBIActive)
                return;
            
#if DEBUG
            project.IsPbiCreateActive = false;
            project.CreatedAfter = "2000-01-01";

#endif
            ClearPerProjectGlobals();

            long acilanWorkItemAdedi = 0;
            long islenenIssueAdedi = 0;
            StringBuilder workItemDetailsSB = new StringBuilder();

            //Hesaplanan son iterasyonu saklıyoruz. 
            //Projeden ayrılan birinin issue'su geldiğinde Default Iteration yerine
            //Bir onceki issue gerçek bir ekip üyesine aitse o issue'dan dogru iterasyon gelmiş olacak.
            string priorIssueIteration = string.Empty;
            string accessToken = GetAzureDevopsApiAccessToken();
            string organization = "your-domainteams";

            //DeveloperTeamNameContains "Arma " ise sonuç olarak "ARMA 1" "ARMA 2" 2 team gelir.
            List<string> ignoredTeamList = new List<string>() { "YNA UI Test" };
            TeamFilteredCollection = TeamCollection.Teams.Where(x => !ignoredTeamList.Contains(x.Name) && x.Name.ToUpper(CultureInfo.InvariantCulture).Contains(project.DeveloperTeamNameContains.ToUpper(CultureInfo.InvariantCulture))).Select(x => x).ToList();

            try
            {
                SonarApiClient sonarApiClient = new SonarApiClient();
                int issueCount = 1;
                bool issueCekmeyeDevamEdecekMi = true;

                while (issueCount > 0 && issueCekmeyeDevamEdecekMi)
                {
                    //Get 'Open' Issues for a specific Sonarqube project
                    //by using Sonarqube API
                    var sonarApiData = sonarApiClient.Search(project.ProjectKey, project.Severities, project.Statuses, project.CreatedAfter);
                    issueCount = sonarApiData.issues.Count;

                    if (issueCount == 0)
                        break;

                    //kayıt açılmıyorsa issue grubu bir kez çekiliyor.
                    if (!project.IsPbiCreateActive)
                        issueCekmeyeDevamEdecekMi = false;

                    //her seferinde en fazla 100 kayıt çektiği için while döngüsü kuruldu.
                    for (int i = 0; i < issueCount; i++)
                    {
                        try
                        {
                            //Process Each Sonarqube Issue
                            var issue = sonarApiData.issues[i];
                            var fileName = FileNameOlustur(issue);

                            bool dosyaExculudeFolderIcindeMi = DosyaExculudeFolderIcindeMi(project.ExculudeFolders, fileName);
                            if (dosyaExculudeFolderIcindeMi)
                            {
                                //PBI açılmayacak. Sadece tekrar okumamak için sonar issue "confirm" yapılıyor.
                                sonarApiClient.DoTransition(issue.key, Transitions.wontfix.ToString());
                                sonarApiClient.AddComment(issue.key, "Sonar2PBI");
                                continue;//to the next sonar issue
                            }

                            DoTransitionApiResponseCouple doTransitionApiResponseCouple;

                            if (project.IsPbiCreateActive)
                                //Update Sonarqube Issue status OPEN to CONFIRMED
                                //So we ensure that we process an ISSUE ONLY ONCE!
                                doTransitionApiResponseCouple = sonarApiClient.DoTransition(issue.key, project.Transition);
                            else
                                doTransitionApiResponseCouple = new DoTransitionApiResponseCouple();

                            if (!project.IsPbiCreateActive || (doTransitionApiResponseCouple.DoTransitionApi != null && doTransitionApiResponseCouple.DoTransitionApi.issue != null))
                            {
                                islenenIssueAdedi += 1;

                                bool pbiAcilacakMi = (project.SeveritiesPBI.Contains(issue.severity) || issue.type == "BUG");
                                if (!pbiAcilacakMi)
                                    continue; //to the next sonar issue


                                Team team = GetTeam(issue.author);

                                string iterationPath = GetIterationPath(issue.author, project, priorIssueIteration);

                                if (priorIssueIteration != iterationPath)
                                    priorIssueIteration = iterationPath;

                                string area = GetArea(project, team, iterationPath);
                                string assignedTo = null;
                                string takimiOlmayanDeveloper = string.Empty;
                                if (string.IsNullOrEmpty(team.Name))
                                {
                                    assignedTo = string.Empty;
                                    takimiOlmayanDeveloper = issue.author;
                                }
                                else
                                {
                                    assignedTo = SanitizeEmailAddress(issue.author);
                                }

                                string title = WorkItemTitleOlustur(issue);
                                long childWorkItemId = 0;
                                string parentWorkItemURL = string.Empty;
                                //Create AzureDevOps BUG
                                if (project.IsPbiCreateActive)
                                {
                                    string wiType = "BUG";
                                    string bugState = project.BugState;

                                    if (project.IsSingleBugMultipleTask)
                                    {
                                        parentWorkItemURL = GetIterationParentWIPath(iterationPath);

                                        if (parentWorkItemURL.IsNullOrWhiteSpace())
                                        {
                                            #region PARENT BUG Açılacak
                                            string parentTitle = ParentWorkItemTitleOlustur(issue.project);
                                            string parentDescription = "Bağlı taskları kontrol ediniz.";
                                            var (phttpResponseMessage, pworkItemId, pworkItemURL) = AzureDevopsApiClient.CreatePBI(accessToken, organization, parentTitle, area, iterationPath, wiType, bugState, parentDescription, assignedTo, issue.severity, issue.type);
                                            if (phttpResponseMessage.StatusCode.ToString() == "OK")
                                            {
                                                parentWorkItemURL = pworkItemURL;
                                                SetIterationParentWIPath(iterationPath, pworkItemURL);
                                            }
                                            else
                                                ProjeHataLogla(new InvalidOperationException(phttpResponseMessage.ToString()));
                                            #endregion
                                        }
                                        wiType = "TASK";
                                        bugState = string.Empty;
                                    }

                                    string description = WorkItemDecriptionOlustur(issue, takimiOlmayanDeveloper);
                                    var (httpResponseMessage, workItemId, workItemURL) = AzureDevopsApiClient.CreatePBI(accessToken, organization, title, area, iterationPath, wiType, bugState, description, assignedTo, issue.severity, issue.type, parentWorkItemURL);
                                    if (httpResponseMessage.StatusCode.ToString() == "OK")
                                    {
                                        childWorkItemId = workItemId;
                                        var addCommentApiResponseCouple = sonarApiClient.AddComment(issue.key, string.Concat("AzureDevops WorkItem: ", workItemId));
                                        if (addCommentApiResponseCouple.AddCommentApi == null)
                                            ProjeHataLogla(addCommentApiResponseCouple.ApiError);
                                    }
                                    else
                                        ProjeHataLogla(new InvalidOperationException(httpResponseMessage.ToString()));
                                }

                                //Bildirim ayarlari
                                workItemDetailsSB.AppendLine("<br>");
                                string workItemDetails = $" {childWorkItemId}; {project.ProjectKey}; {assignedTo}; {takimiOlmayanDeveloper}; {area}; {iterationPath}; {title}";
                                workItemDetailsSB.AppendLine(workItemDetails);
                                Debug.WriteLine(workItemDetails);
                                acilanWorkItemAdedi++;

                            }
                            else
                            {
                                ProjeHataLogla(doTransitionApiResponseCouple.ApiError);
                            }

                        }
                        catch (Exception logExceptionIssue)
                        {
                            ProjeHataLogla(logExceptionIssue);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                ProjeHataLogla(e);
            }

            LogSummary(project , HataAdedi, acilanWorkItemAdedi, islenenIssueAdedi, workItemDetailsSB.ToString());


        }

        /// <summary>
        /// Arma Web üzerinde çalışan Arma 1 ve Arma 2 takımplarının koştukları sprintler faklı
        /// Aynı taramada iki takım (iteration) için de issue çıkıyor.
        /// Bunların kendi parent BUG larına bağlanması gerekir.
        /// </summary>
        /// <param name="iterationPath"></param>
        private string GetIterationParentWIPath(string iterationPath)
        {
            if (iterationPath.IsNullOrWhiteSpace())
                return string.Empty;

            if (parentWIHashTable.Contains(iterationPath))
                return (parentWIHashTable[iterationPath]).ToString();

            return string.Empty;
        }

        private void SetIterationParentWIPath(string iterationPath, string parentWorkItemURL)
        {
            parentWIHashTable[iterationPath] = parentWorkItemURL;
        }

        private static string ParentWorkItemTitleOlustur(string project)
        {
            return $"{ConfigHelper.TitlePrefix} [{project}] Parent-BUG {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}";
        }

        public void ProjeHataLogla(ApiErrorResponse apiErrorResponse)
        {
            HataAdedi++;
            LogHelper.Logla(apiErrorResponse);
        }

        public void ProjeHataLogla(Exception logException)
        {
            HataAdedi++;
            LogHelper.Logla(logException);
        }
        




        private static void LogSummary(Project project, long hataAdedi, long acilanWorkItemAdedi, long islenenIssueAdedi, string workItemDetails )
        {
            try
            {
                string projectKey = project.ProjectKey;

                string hataMesaji = (hataAdedi > 0) ? string.Format(" <b>{0}</b> hata alındı. ", hataAdedi) : string.Empty;
                string issueMesaji =    $"<br><b>{islenenIssueAdedi}</b> Issue işlendi. <i>({project.CreatedAfter} tarihinden itibaren açılan Sonarqube Issue'lar dikkate alınmıştır. Bu tarih KAP uygulamasından değiştirilebilir.)</i>";
                string workItemMesaji = $"<br><b>{acilanWorkItemAdedi}</b> İş Maddesi oluştu.";
                if (!project.IsPbiCreateActive)
                {
                    workItemMesaji = string.Concat(workItemMesaji, "<i> Projenizin 'IsPbiCreateActive' anahtarı kapalı olduğu için <u>fiili olarak herhangi bir iş maddesi açılmamıştır</u>.</i>");
                }
                
                if (hataAdedi > 0 || acilanWorkItemAdedi > 0 || islenenIssueAdedi > 0)
                {
                    string sembol = (acilanWorkItemAdedi > 0) ? "⚠" : "👍";
                    if (hataAdedi > 0)
                        sembol = "❌";

                    string subject = $"{ConfigHelper.SubjectPrefix} {projectKey} {sembol}";
                    Debug.WriteLine(string.Concat(projectKey, "; ", workItemMesaji));
                    StringBuilder bodysb = new StringBuilder();

                    if (!string.IsNullOrWhiteSpace(hataMesaji))
                        bodysb.AppendLine(hataMesaji);

                    if (!string.IsNullOrWhiteSpace(issueMesaji))
                        bodysb.AppendLine(issueMesaji);

                    if (!string.IsNullOrWhiteSpace(workItemMesaji))
                        bodysb.AppendLine(workItemMesaji);

                    if (!string.IsNullOrWhiteSpace(workItemDetails))
                    {
                        bodysb.AppendLine($"<br>{workItemDetails}");
                    }

                    string body = bodysb.ToString();
                    string toAdresi = ToAdresiOlustur(project, hataAdedi);

                    SmtpHelper.EpostaGonder(body, subject, toAdresi);
                }
            }
            catch (Exception logException)
            {
                LogHelper.Logla(logException);
            }
        }

        private static string ToAdresiOlustur(Project project, long hataAdedi)
        {
            string toAdresi = ConfigHelper.ToAdress;

            if (!project.IsPbiCreateActive)
                toAdresi = project.IlkYoneticiEpostaAdresi;

            if (hataAdedi > 0)
                toAdresi = project.EkipEpostaAdresi;

            return toAdresi;
        }

        public string GetTeamsDefaultArea(Team team)
        {
            try
            {
                var root = AzureDevopsApiClient.GetTeamfieldvalues(GetAzureDevopsApiAccessToken(), team.ProjectId.ToString(), team.Id.ToString());
                if (root != null)
                {
                    string defaultValue = root.defaultValue;
                    if (!defaultValue.IsNullOrWhiteSpace())
                        return defaultValue;
                }
            }
            catch (Exception)
            {
                //isteyerek boş bırakıldı.
            }
            return string.Empty;
        }
        private string GetArea(Project project, Team team, string iterationName)
        {
            //area girilmis ise kullanılır.
            if (project.Area.HasValidInput())
                return project.Area;

            //Teams'in default areası varsa oradan kullanır.
            string defaultArea = GetTeamsDefaultArea(team);
            if (defaultArea.HasValidInput())
                return defaultArea;

            //Gerçek bir iteration bulursak, o zaman AREA'yı oradan çıkartırız.
            if ((!string.IsNullOrWhiteSpace(team.Name) && !project.Area.Contains(team.Name)))
            {
                var iterationItems = iterationName.Split('\\');
                StringBuilder sb = new StringBuilder();
                char ayrac = '\\';
                for (int j = 0; j < iterationItems.Length - 1; j++)
                {
                    sb.Append(iterationItems[j] + ayrac);
                }
                string area = sb.ToString().TrimEnd(ayrac);
                if (area.HasValidInput())
                    return area;
            }

            //buraya kadar geldi ise, KAP'ta tanımlı en kötü ihtimal AREA ne olmalı değeri SET edilir.
            if (project.ZeroOptionArea.HasValidInput())
                return project.ZeroOptionArea;

            return string.Empty;
        }

        public static string GetUpperPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            var iterationItems = path.Split('\\');
            if (iterationItems.Length < 2)
                return path;

            StringBuilder sb = new StringBuilder();
            char ayrac = '\\';
            for (int j = 0; j < iterationItems.Length - 1; j++)
            {
                sb.Append(iterationItems[j] + ayrac);
            }
            return sb.ToString().TrimEnd(ayrac);
        }

        private Team GetTeam(string author)
        {
            if (string.IsNullOrEmpty(author))
                return new Team();

            if (UserTeamHastable.ContainsKey(author))
                return (Team)UserTeamHastable[author];


            string uniqueName = GetUserPart(author);

            foreach (Team team in TeamFilteredCollection)
            {

                var members = AzureDevopsApiClient.GetTeamMembers(GetAzureDevopsApiAccessToken(), "your-domainteams", team.ProjectId.ToString(), team.Id.ToString());
                if (members.TeamMembers.Any(x => x.Identity.UniqueName.Contains(uniqueName)))
                {
                    UserTeamHastable.Add(author, team);
                    return team;
                }
            }

            UserTeamHastable.Add(author, new Team());
            return new Team();

        }

        private string GetIterationPath(string author, Project project, string priorIssueIteration)
        {
            //get it from the cache, if avaliable
            if (UserIterationHastable.ContainsKey(author))
                return (string)UserIterationHastable[author];

            //not in cache. Do Calculation ( using API CALL)
            //defult
            if (string.IsNullOrWhiteSpace(author))
                return GetZeroOptionIterationPath(project, priorIssueIteration);

            string result = string.Empty;

            Team team = GetTeam(author);
            if (!string.IsNullOrWhiteSpace(team.Name))
            {
                //Use "current iteration" if exists
                var teamIterations = AzureDevopsApiClient.GetIterations(GetAzureDevopsApiAccessToken(), "your-domainteams", team.ProjectId.ToString(), team.Id.ToString());
                if (teamIterations.Count > 0)
                {
                    if (teamIterations.Iterations.Any(x => x.Attributes.TimeFrame == TimeFrame.Current))
                    {
                        var selectedIteration = teamIterations.Iterations.Select(x => x).Where(x => x.Attributes.TimeFrame == TimeFrame.Current).ElementAtOrDefault(0);
                        result = selectedIteration.Path;
                    }
                    else
                    {
                        //"current" iteration yok
                        result = GetZeroOptionIterationPath(project, priorIssueIteration);
                    }
                }
                else
                {
                    //elimde hiç bir iteration yok.
                    result = GetZeroOptionIterationPath(project, priorIssueIteration);
                }
            }
            else
            {
                //elimde takım yok
                //bu nedenle hiç bir iteration yok.
                result = GetZeroOptionIterationPath(project, priorIssueIteration);
            }

            //update cache for later calls
            UserIterationHastable.Add(author, result);
            return result;
        }

        private static string GetZeroOptionIterationPath(Project project, string priorIssueIteration)
        {
            if (!string.IsNullOrWhiteSpace(priorIssueIteration))
                return priorIssueIteration;

            if (!string.IsNullOrWhiteSpace(project.ZeroOptionIterationPath))
                return project.ZeroOptionIterationPath;

            if (!string.IsNullOrEmpty(project.Area))
                return GetUpperPath(project.Area);

            if (!string.IsNullOrEmpty(project.ZeroOptionArea))
                return GetUpperPath(project.Area);

            return string.Empty;
        }

        private string GetUserPart(string email)
        {
            int pos = email.IndexOf('@');
            if (pos < 0)
                return email;

            return email.Substring(0, pos);
        }

        public static string SanitizeEmailAddress(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
                return string.Empty;

            int pos = author.IndexOf('@');
            if (pos < 0)
                return author;

            return author.Substring(0, pos) + "@your-domain.com";
        }

        private static bool DosyaExculudeFolderIcindeMi(string excludeFolders, string fileName)
        {
            if (string.IsNullOrWhiteSpace(excludeFolders))
                return false;

            string[] exculudeFolderList = excludeFolders.Split(',');

            if (exculudeFolderList == null || !exculudeFolderList.Any())
                return false;

            var arr = fileName.Split('/');

            string folder = (arr.Length == 0) ? fileName.Trim() : arr[0].Trim();

            foreach (var exculudeFolder in exculudeFolderList)
                if (folder.Equals(exculudeFolder.Trim()))
                    return true;

            return false;
        }

        private static string FileNameOlustur(Issue issue)
        {
            var companents = issue.component.Split(':');
            string file = (companents.Length > 0) ? companents[companents.Length - 1] : string.Empty;
            string line = string.Empty;
            if (issue.textRange != null)
                line = (issue.textRange.startLine == issue.textRange.endLine) ? issue.textRange.startLine.ToString() : string.Format("{0} - {1}", issue.textRange.startLine, issue.textRange.endLine);
            return string.Concat(file, " ", line).Trim();
        }

        private static string WorkItemTitleOlustur(Issue issue)
        {
            string file = FileNameOlustur(issue);
            string message = (issue.message.Length <= 50) ? issue.message : issue.message.Substring(0, 47) + "...";
            return $"{ConfigHelper.TitlePrefix} [{issue.project}] {message} {file}";
        }

        private static string WorkItemDecriptionOlustur(Issue issue, string takimiOlmayanDeveloper)
        {
            SonarApiClient sonarApiClient = new SonarApiClient();
            string dashBoardURL = sonarApiClient.GetCompanentUrl(issue);

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Concat("<B>", issue.message, "</B>"));

            if (!string.IsNullOrWhiteSpace(takimiOlmayanDeveloper))
            {
                sb.Append("<BR>");
                sb.Append($"issue author : {takimiOlmayanDeveloper}");
            }

            sb.Append("<BR>");
            sb.Append(FileNameOlustur(issue));
            sb.Append("<BR>");
            sb.Append("<BR>");
            sb.Append(string.Concat("<a href =\"", dashBoardURL, "\" >", ConfigHelper.DescriptionIssueSonarLink, "</a>"));
            sb.Append("<BR>");
            sb.Append("<BR>");
            RuleShowApiResponse response = sonarApiClient.ShowRule(issue.rule);
            if (response != null)
            {
                sb.Append(response.rule.name);
                sb.Append("<BR>");
                sb.Append(response.rule.htmlDesc);
            }

            return sb.ToString();
        }

        public string GetAzureDevopsApiAccessToken()
        {
            //1st Chance : Argument
            if (!string.IsNullOrWhiteSpace(AzureDevopsApiToken))
                return AzureDevopsApiToken;

            //2nd Chance : App.config
            return ConfigHelper.AzureDevopsToken;
        }
    }
}