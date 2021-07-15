using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sonar2PBI
{
    [DataContract]
    public class SearchApiResponse
    {
        public SearchApiResponse()
        {
        }

        [DataMember]
        public int total { get; set; }

        [DataMember]
        public int p { get; set; }

        [DataMember]
        public int ps { get; set; }

        [DataMember]
        public Paging paging { get; set; }

        [DataMember]
        public List<Issue> issues { get; set; }

        [DataMember]
        public List<Component> components { get; set; }
    }

    [DataContract]
    public class DoTransitionApiResponse
    {
        public DoTransitionApiResponse()
        {
        }

        [DataMember]
        public Issue issue { get; set; }

        [DataMember]
        public List<Component> components { get; set; }

        [DataMember]
        public List<Rule> rules { get; set; }

        [DataMember]
        public List<object> users { get; set; }

        [DataMember]
        public List<object> actiunusedActionPlansonPlans { get; set; }
    }

    [DataContract]
    public class ApiErrorResponse
    {
        [DataMember]
        public List<Error> errors { get; set; }
    }

    [DataContract]
    public class AddCommentApiResponse
    {
        [DataMember]
        public Comment comment { get; set; }
    }

    public class Comment
    {
        public string key { get; set; }
        public string login { get; set; }
        public string htmlText { get; set; }
        public string createdAt { get; set; }
    }

    public class DoTransitionApiResponseCouple
    {
        public DoTransitionApiResponse DoTransitionApi;
        public ApiErrorResponse ApiError;
    }

    public class AddCommentApiResponseCouple
    {
        public AddCommentApiResponse AddCommentApi;
        public ApiErrorResponse ApiError;
    }

    public class Error
    {
        public string msg { get; set; }
    }

    public class Paging
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
    }

    public class TextRange
    {
        public int startLine { get; set; }
        public int endLine { get; set; }
        public int startOffset { get; set; }
        public int endOffset { get; set; }
    }

    public class Issue
    {
        public string key { get; set; }
        public string rule { get; set; }
        public string severity { get; set; }
        public string component { get; set; }
        public int componentId { get; set; }
        public string project { get; set; }
        public string subProject { get; set; }
        public int line { get; set; }
        public TextRange textRange { get; set; }
        public List<object> flows { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string effort { get; set; }
        public string debt { get; set; }
        public string author { get; set; }
        public List<object> tags { get; set; }
        public string creationDate { get; set; }
        public string updateDate { get; set; }
        public string type { get; set; }
        public string resolution { get; set; }
        public string closeDate { get; set; }
    }

    public class Component
    {
        public int id { get; set; }
        public string key { get; set; }
        public string uuid { get; set; }
        public bool enabled { get; set; }
        public string qualifier { get; set; }
        public string name { get; set; }
        public string longName { get; set; }
        public string path { get; set; }
        public int projectId { get; set; }
        public int subProjectId { get; set; }
    }


    public class Rule
    {
        public string key { get; set; }
        public string repo { get; set; }
        public string name { get; set; }
        public string createdAt { get; set; }
        public string htmlDesc { get; set; }
        public string mdDesc { get; set; }
        public string severity { get; set; }
        public string status { get; set; }
        public bool isTemplate { get; set; }
        public List<object> tags { get; set; }
        public List<string> sysTags { get; set; }
        public string lang { get; set; }
        public string langName { get; set; }
        public List<object> @params { get; set; }
        public string defaultDebtRemFnType { get; set; }
        public string defaultDebtRemFnOffset { get; set; }
        public bool debtOverloaded { get; set; }
        public string debtRemFnType { get; set; }
        public string debtRemFnOffset { get; set; }
        public string defaultRemFnType { get; set; }
        public string defaultRemFnBaseEffort { get; set; }
        public string remFnType { get; set; }
        public string remFnBaseEffort { get; set; }
        public bool remFnOverloaded { get; set; }
        public string type { get; set; }
    }

    [DataContract]
    public class RuleShowApiResponse
    {
        [DataMember]
        public Rule rule { get; set; }

        [DataMember]
        public List<object> actives { get; set; }
    }
}