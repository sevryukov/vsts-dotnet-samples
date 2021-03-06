﻿using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.TeamServices.Samples.Client
{
    public static class ClientSampleHelpers
    {
        public static TeamProjectReference FindAnyProject(ClientSampleContext context)
        {
            TeamProjectReference project;
            if (!FindAnyProject(context, out project))
            {
                throw new Exception("No sample projects available. Create a project in this collection and run the sample again.");
            }

            return project;
        }

        public static bool FindAnyProject(ClientSampleContext context, out TeamProjectReference project)
        {  
            // Check if we already have a default project loaded
            if (!context.TryGetValue<TeamProjectReference>("$defautProject", out project))
            {
                VssConnection connection = context.Connection;
                ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

                using (new ClientSampleHttpLoggerOutputSuppression())
                {
                    // Check if an ID was already set (this could have been provided by the caller)
                    Guid projectId;
                    if (!context.TryGetValue<Guid>("projectId", out projectId))
                    {
                        // Get the first project
                        project = projectClient.GetProjects(null, top: 1).Result.FirstOrDefault();
                    }
                    else
                    {
                        // Get the details for this project
                        project = projectClient.GetProject(projectId.ToString()).Result;
                    }
                }

                if (project != null)
                {
                    context.SetValue<TeamProjectReference>("$defautProject", project);
                }
                else
                {
                    // create a project here?
                    throw new Exception("No projects available for running the sample.");
                }
            }

            return project != null;
        }

        public static WebApiTeamRef FindAnyTeam(ClientSampleContext context, Guid? projectId)
        {
            WebApiTeamRef team;
            if (!FindAnyTeam(context, projectId, out team))
            {
                throw new Exception("No sample teams available. Create a project/team in this collection and run the sample again.");
            }

            return team;
        }

        public static bool FindAnyTeam(ClientSampleContext context, Guid? projectId, out WebApiTeamRef team)
        {
            if (!projectId.HasValue)
            {
                TeamProjectReference project;
                if (FindAnyProject(context, out project))
                {
                    projectId = project.Id;
                }
            }

            // Check if we already have a team that has been cached for this project
            if (!context.TryGetValue<WebApiTeamRef>("$" + projectId + "Team", out team))
            {
                TeamHttpClient teamClient = context.Connection.GetClient<TeamHttpClient>();

                using (new ClientSampleHttpLoggerOutputSuppression())
                {
                    team = teamClient.GetTeamsAsync(projectId.ToString(), top: 1).Result.FirstOrDefault();
                }

                if (team != null)
                {
                    context.SetValue<WebApiTeamRef>("$" + projectId + "Team", team);
                }
                else
                {
                    // create a team?
                    throw new Exception("No team available for running this sample.");
                }
            }

            return team != null;
        }

        public static Guid GetCurrentUserId(ClientSampleContext context)
        {
            return context.Connection.AuthorizedIdentity.Id;
        }

        public static String GetSampleTextFile()
        {
            return GetSampleFilePath("Microsoft.TeamServices.Samples.Client.WorkItemTracking.SampleFile.txt");
        }

        public static String GetSampleBinaryFile()
        {
            return GetSampleFilePath("Microsoft.TeamServices.Samples.Client.WorkItemTracking.SampleFile.png");
        }

        /// <summary>
        /// Creates a temp file from an embedded resource and returns the full path to it
        /// </summary>
        /// <param name="fullResourceName"></param>
        /// <returns></returns>
        private static string GetSampleFilePath(String fullResourceName)
        {
            Stream inputStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
            FileInfo tempOutputFile = new FileInfo(Path.Combine(Path.GetTempPath(), fullResourceName));
            FileStream tempFileOutputStream = tempOutputFile.OpenWrite();
            inputStream.CopyTo(tempFileOutputStream);

            tempFileOutputStream.Close();
            inputStream.Close();

            return tempOutputFile.FullName;
        }


    }
}

