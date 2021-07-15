using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Sonar2PBI
{
    public class DbHelper
    {
        public string GetConnString
        {
            get
            {
                return ConfigHelper.KAPConnectionString;
            }
        }

        public int GetProjectCount()
        {
            try
            {
                const string sql = "select count(*) from proje ";
                int result = 0;
                using (var connection = new SqlConnection(GetConnString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        result = (int)command.ExecuteScalar();
                    }
                    connection.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logla(ex);

            }
            return 0;
        }

        private SqlCommand GetCommand(string projectKey)
        {
            StringBuilder stringBuilder = new StringBuilder("SELECT [SonarqubeProjectKey], IsSonar2PBIActive, IsPbiCreateActive, DeveloperTeamNameContains, Area, ZeroOptionArea, ZeroOptionIterationPath, SeveritiesPBI, Severities, Statuses ,Transition ,CreatedAfter,ExculudeFolders,EkipEpostaAdresi,IlkYoneticiEpostaAdresi, IsSingleBugMultipleTask, BugState FROM PROJE WHERE [AktifMi]=1 and [IsSonar2PBIActive]=1");
            SqlCommand command = new SqlCommand();

            if (!string.IsNullOrWhiteSpace(projectKey))
            {
                stringBuilder.Append(" and SonarqubeProjectKey=@projectKey");
                command.Parameters.Add("@projectKey", SqlDbType.NVarChar);
                command.Parameters["@projectKey"].Value = projectKey;
            }
            command.CommandText = stringBuilder.ToString();
            return command;    
        }

            public List<Project> GetProjects(string projectKey)
        {
            List<Project> sonarProjects = new List<Project>();
            try
            {

                using (var connection = new SqlConnection(GetConnString))
                {
                    connection.Open();
                    using (SqlCommand command = GetCommand(projectKey))
                    {
                        command.Connection = connection;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var project = new Project()
                                {
                                    //"ArfleetMicroservicesPK" 
                                    ProjectKey = reader.GetValue(reader.GetOrdinal("SonarqubeProjectKey")).ToString().Trim(),
                                    //"1" 
                                    IsPbiCreateActive = ConvertoBooleanSafe(reader.GetValue(reader.GetOrdinal("IsPbiCreateActive"))),
                                    //"1" 
                                    IsSonar2PBIActive = ConvertoBooleanSafe(reader.GetValue(reader.GetOrdinal("IsSonar2PBIActive"))),
                                    //"1" 
                                    IsSingleBugMultipleTask = ConvertoBooleanSafe(reader.GetValue(reader.GetOrdinal("IsSingleBugMultipleTask"))),
                                    //"Arfleet"       
                                    DeveloperTeamNameContains = reader.GetValue(reader.GetOrdinal("DeveloperTeamNameContains")).ToString().Trim().Replace("?", " "),
                                    //"Projects\Arfleet"  
                                    Area = reader.GetValue(reader.GetOrdinal("Area")).ToString().Trim(),
                                    // ""  
                                    ZeroOptionArea = reader.GetValue(reader.GetOrdinal("ZeroOptionArea")).ToString().Trim(),
                                    //"Projects\Arfleet" 
                                    ZeroOptionIterationPath = reader.GetValue(reader.GetOrdinal("ZeroOptionIterationPath")).ToString().Trim(),
                                    //"BLOCKER,CRITICAL" 
                                    SeveritiesPBI = reader.GetValue(reader.GetOrdinal("SeveritiesPBI")).ToString().Trim(),
                                    //"BLOCKER,CRITICAL,MAJOR"  
                                    Severities = reader.GetValue(reader.GetOrdinal("Severities")).ToString().Trim(),
                                    //Statuses = "OPEN,REOPENED" 
                                    Statuses = reader.GetValue(reader.GetOrdinal("Statuses")).ToString().Trim(),
                                    //Transition = "confirm" 
                                    Transition = reader.GetValue(reader.GetOrdinal("Transition")).ToString().Trim(),
                                    //CreatedAfter = "2019-01-01" 
                                    CreatedAfter = reader.GetValue(reader.GetOrdinal("CreatedAfter")).ToString().Trim(),
                                    //ExculudeFolders = "Scriptz" 
                                    ExculudeFolders = reader.GetValue(reader.GetOrdinal("ExculudeFolders")).ToString().Trim(),
                                    //EkipEpostaAdresi="arles@your-domain.com.tr"
                                    EkipEpostaAdresi = reader.GetValue(reader.GetOrdinal("EkipEpostaAdresi")).ToString().Trim(),
                                    IlkYoneticiEpostaAdresi = reader.GetValue(reader.GetOrdinal("IlkYoneticiEpostaAdresi")).ToString().Trim(),
                                                                        //BugState New, Committed
                                    BugState = reader.GetValue(reader.GetOrdinal("BugState")).ToString().Trim()
                                };
                                sonarProjects.Add(project);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logla(ex);

            }
            return sonarProjects; 
        }

        private bool ConvertoBooleanSafe(object obj)
        {
            if (obj == null)
                return false;
            
            int deger = (int)obj;

            return (deger == 1);
                

        }

    }
}