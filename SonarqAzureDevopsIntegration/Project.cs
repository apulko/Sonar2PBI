namespace Sonar2PBI
{
    public class Project
    {
        public string ProjectKey { get; set; }
        public bool IsPbiCreateActive { get; set; }

        public bool IsSonar2PBIActive { get; set; }

        public bool IsSingleBugMultipleTask { get; set; }
        
        public string Severities { get; set; }
        public string SeveritiesPBI { get; set; }
        public string Statuses { get; set; }
        public string Transition { get; set; }
        public string CreatedAfter { get; set; }

        public string DeveloperTeamNameContains { get; set; }
        public string Area { get; set; }
        public string ZeroOptionArea { get; set; }
        public string ZeroOptionIterationPath { get; set; }
        public string ExculudeFolders { get; set; }

        public string EkipEpostaAdresi { get; set; }
        public string IlkYoneticiEpostaAdresi { get; set; }
        public string BugState { get; set; }
    }
}
