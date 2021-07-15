using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace Sonar2PBI
{
    #region ENUMS

    public enum Severities
    {
        INFO, MINOR, MAJOR, CRITICAL, BLOCKER
    }

    public enum Transitions
    {
        confirm, unconfirm, reopen, resolve, falsepositive, wontfix, close
    }

    #endregion ENUMS

    internal class SonarApiClient
    {
        #region API CAGIRIMLARI

        public SearchApiResponse Search(string projectKey, string severities, string statuses, string createdAfter)
        {
            var openIssuesUriStr = string.Format("{0}/api/issues/search?componentKeys={1}&createdAfter={2}&severities={3}&statuses={4}", ConfigHelper.SonarServerURL, projectKey, createdAfter, severities, statuses);

            try
            {
                using (var webClient = new WebClient())
                {
                    //username alanında token'i veriyoruz. password boş geçiyoruz.
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ConfigHelper.SonarServerToken + ":");
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    webClient.Headers.Add("Authorization", "Basic " + val);
                    var bytes = webClient.DownloadData(new Uri(openIssuesUriStr));
                    string result = System.Text.Encoding.UTF8.GetString(bytes);
                    return (SearchApiResponse)JsonConvert.DeserializeObject(result, typeof(SearchApiResponse));
                }
            }
            catch (Exception logException)
            {
                LogHelper.Logla(logException, openIssuesUriStr);
                throw;
            }
        }

        public DoTransitionApiResponseCouple DoTransition(string issueKey, string transition)
        {
            var responseCouple = new DoTransitionApiResponseCouple();

            string setConfirmUriStr = string.Concat("api/issues/do_transition?issue=", issueKey, "&transition=", transition);
            var httpResponse = PostRequest(setConfirmUriStr, null);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
                responseCouple.DoTransitionApi = httpResponse.Content.ReadAsAsync<DoTransitionApiResponse>().Result;
            else
            {
                responseCouple.ApiError = httpResponse.Content.ReadAsAsync<ApiErrorResponse>().Result;
                if (responseCouple.ApiError == null || responseCouple.ApiError.errors == null)
                    responseCouple.ApiError = ApiErrorResponseOlustur(httpResponse);
            }

            return responseCouple;
        }

        public AddCommentApiResponseCouple AddComment(string issueKey, string text)
        {
            AddCommentApiResponseCouple responseCouple = new AddCommentApiResponseCouple();

            string setConfirmUriStr = string.Concat("api/issues/add_comment?issue=", issueKey, "&text=", text);

            var httpResponse = PostRequest(setConfirmUriStr, null);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
                responseCouple.AddCommentApi = httpResponse.Content.ReadAsAsync<AddCommentApiResponse>().Result;
            else
            {
                responseCouple.ApiError = httpResponse.Content.ReadAsAsync<ApiErrorResponse>().Result;
                if (responseCouple.ApiError == null || responseCouple.ApiError.errors == null)
                    responseCouple.ApiError = ApiErrorResponseOlustur(httpResponse);
            }

            return responseCouple;
        }

        public string GetCompanentUrl(Issue issue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Concat(ConfigHelper.SonarServerURL, @"/code?id=", issue.component));
            if (issue.textRange != null && issue.textRange.startLine > 0)
                sb.Append(string.Concat("&line=", issue.textRange.startLine));
            return sb.ToString();
        }

        public RuleShowApiResponse ShowRule(string ruleKey)
        {
            try
            {
                string getUrl = string.Format("{0}/api/rules/show?key={1}", ConfigHelper.SonarServerURL, ruleKey);

                using (var webClient = new WebClient())
                {
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ConfigHelper.SonarServerToken + ":");
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    webClient.Headers.Add("Authorization", "Basic " + val);

                    var bytes = webClient.DownloadData(new Uri(getUrl));
                    string result = System.Text.Encoding.UTF8.GetString(bytes);
                    return (RuleShowApiResponse)JsonConvert.DeserializeObject(result, typeof(RuleShowApiResponse));
                }
            }
            catch (Exception logException)
            {
                LogHelper.Logla(logException);
            }
            return null;
        }

        #endregion API CAGIRIMLARI

        #region YARDIMCI METOTLAR

        private ApiErrorResponse ApiErrorResponseOlustur(HttpResponseMessage httpResponseMessage)
        {
            var apiErrorResponse = new ApiErrorResponse();
            Error error = new Error();
            error.msg = string.Concat(httpResponseMessage.StatusCode, " Request: ", httpResponseMessage.RequestMessage.RequestUri.AbsoluteUri);
            apiErrorResponse.errors = new List<Error> { error };
            return apiErrorResponse;
        }

        public HttpResponseMessage PostRequest(string requestUri, object value)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigHelper.SonarServerURL);
                ConfigureAlpHeader(client.DefaultRequestHeaders);
                return client.PostAsJsonAsync(requestUri, value).Result;
            }
        }

        private void ConfigureAlpHeader(HttpRequestHeaders headers)
        {
            headers.Accept.Clear();
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ConfigHelper.SonarServerToken + ":");
            string val = System.Convert.ToBase64String(plainTextBytes);
            headers.Add("Authorization", "Basic " + val);
        }

        #endregion YARDIMCI METOTLAR
    }
}