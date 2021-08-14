

using BusinessLayer.BusinessLayerDtos;
using BusinessLayer.Interfaces;
using ClosedXML.Excel;
using ExternalServices.Interfaces;
using Microsoft.TeamFoundation.Discussion.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace BusinessLayer.Implementations
{
    public class PeerCodeReviewService : IPeerCodeReviewService
    {
        private readonly ITeamFoundationService _teamFoundationService;
        private readonly IExcelService _excelService;
        private const string codeReviewRequest = "Code Review Request";
        private readonly string FileDropLocation = ConfigurationManager.AppSettings.Get("FileDropLocation").ToString();
        private readonly List<string> teams = ConfigurationManager.AppSettings.Get("Teams").Split(',').Select(i => i.Trim()).ToList();
        private readonly string _projectName = ConfigurationManager.AppSettings.Get("Project").ToString();
        public PeerCodeReviewService(ITeamFoundationService teamFoundationService,IExcelService excelService)
        {
            _teamFoundationService = teamFoundationService;
            _excelService = excelService;
        }
        public void GetCodeReviewDetails(GetPeerReviewRequest getPeerReviewRequest)
        {
            List<CodeReviewCompletedChangeset> codeReviewCompletedChangesets = new List<CodeReviewCompletedChangeset>();
            List<NoCodeReviewRequestChangeset> noCodeReviewRequestChangesets = new List<NoCodeReviewRequestChangeset>();
            List<CodeReviewNotDoneChangeset> codeReviewNotDoneChangesets = new List<CodeReviewNotDoneChangeset>();
            IEnumerable<Changeset> changesets = _teamFoundationService.GetChangesetsInDateRange(getPeerReviewRequest.sprintStartDate.Date, getPeerReviewRequest.sprintEndDate.AddDays(1).Date);
            foreach (Changeset changeset in changesets)
            {
                if (changeset.WorkItems.Any(i => i.Type.Name == codeReviewRequest))
                {

                    AddIntoCompletedOrRequestedCodeReviewChangesets(changeset,
                                                                    changeset.WorkItems,
                                                                    codeReviewCompletedChangesets,
                                                                    codeReviewNotDoneChangesets);

                }
                else
                {
                    AddIntoCodeReviewRequestChangesets(changeset, noCodeReviewRequestChangesets);
                }
            }
            var codeReviewDetails = new CodeReviewDetails 
            { codeReviewCompletedChangesets = codeReviewCompletedChangesets, 
              noCodeReviewRequestChangesets = noCodeReviewRequestChangesets, 
              codeReviewNotDoneChangesets = codeReviewNotDoneChangesets };
            CreateExcelReport(codeReviewDetails,getPeerReviewRequest.sprintName);
        }
        private static DataTable GetCodeReviewCompletedChangesetsData(List<CodeReviewCompletedChangeset> codeReviewCompletedChangesets)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ChangeSet", typeof(string));
            dataTable.Columns.Add("Owner", typeof(string));
            dataTable.Columns.Add("Reviewer", typeof(string));
            dataTable.Columns.Add("CheckedInDate", typeof(string));
            dataTable.Columns.Add("ReviewedDate", typeof(string));
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("Comments", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));

            foreach (var changeset in codeReviewCompletedChangesets)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["ChangeSet"] = changeset.Changeset;
                dataRow["Owner"] = changeset.Owner;
                dataRow["Reviewer"] = string.Join("\n", changeset.Reviewers);
                dataRow["CheckedInDate"] = changeset.CheckedInDate.Date.ToString("d");
                dataRow["ReviewedDate"] = changeset.ReviewedDate.Value.ToString("d");
                dataRow["Title"] = changeset.Title;

                string commentsString = string.Empty;
                var commentsbyauthors = changeset.CodeReviewComments.OrderBy(i => i.Author).GroupBy(i => i.Author);
                foreach (var item in commentsbyauthors)
                {
                    int counter = 1;
                    commentsString += item.Key + "\n";

                    foreach (var item1 in item.Distinct())
                    {
                        commentsString += counter + " " + item1.Comments + "\n";
                        counter++;
                    }

                    commentsString += "\n";
                }
                dataRow["Comments"] = commentsString;
                dataRow["Status"] = changeset.Status;

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        private static DataTable GetCodeReviewNotDoneChangesets(List<CodeReviewNotDoneChangeset> codeReviewNotDoneChangesets)
        {

            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ChangeSet", typeof(string));
            dataTable.Columns.Add("Owner", typeof(string));
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("CheckedInDate", typeof(string));
            dataTable.Columns.Add("AssignedTo", typeof(string));

            foreach (var item in codeReviewNotDoneChangesets)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["ChangeSet"] = item.Changeset;
                dataRow["Owner"] = item.Owner;
                dataRow["Title"] = item.Title;
                dataRow["CheckedInDate"] = item.CheckedInDate.Date.ToString("d");
                dataRow["AssignedTo"] = string.Join("\n", item.Reviewers);

                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private static DataTable GetNoCodeReviewRequestChangesets(List<NoCodeReviewRequestChangeset> noCodeReviewRequestChangesets)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ChangeSet", typeof(string));
            dataTable.Columns.Add("Owner", typeof(string));
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("CheckedInDate", typeof(string));

            foreach (var item in noCodeReviewRequestChangesets)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["ChangeSet"] = item.Changeset;
                dataRow["Owner"] = item.Owner;
                dataRow["Title"] = item.Title;
                dataRow["CheckedInDate"] = item.CheckedInDate.Date.ToString("d");
                dataTable.Rows.Add(dataRow);
            }


            return dataTable;

        }
        private static List<string> FindChangsetOwnersWhoAreNotPartOfAnyTeam(List<GetTeamDetails> teamDetails, CodeReviewDetails codeReviewDetails)
        {
            var teamMembers = new List<string>();
            teamDetails.ForEach(team => teamMembers.AddRange(team.Members));
            var changesetOwners = new List<string>();
            changesetOwners.AddRange(codeReviewDetails.codeReviewCompletedChangesets.Select(i => i.Owner).Distinct());
            changesetOwners.AddRange(codeReviewDetails.codeReviewNotDoneChangesets.Select(i => i.Owner).Distinct());
            changesetOwners.AddRange(codeReviewDetails.noCodeReviewRequestChangesets.Select(i => i.Owner).Distinct());

            return changesetOwners.Except(teamMembers).ToList();
        }

        private static int CountChangsesetsTeamWise(List<string> teamMembers, CodeReviewDetails codeReviewDetails)
        {
            int count = 0;
            foreach (var teamMember in teamMembers)
            {
                count += codeReviewDetails.codeReviewCompletedChangesets.Count(changest => changest.Owner == teamMember);
                count += codeReviewDetails.codeReviewNotDoneChangesets.Count(changest => changest.Owner == teamMember);
                count += codeReviewDetails.noCodeReviewRequestChangesets.Count(changest => changest.Owner == teamMember);
            }
            return count;
        }
        private static DataTable GetTeamDetails(List<GetTeamDetails> teamDetails, CodeReviewDetails codeReviewDetails)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Team", typeof(string));
            dataTable.Columns.Add("Members");
            dataTable.Columns.Add("Changets having Comments", typeof(int));
            dataTable.Columns.Add("Changests without completing code review", typeof(int));
            dataTable.Columns.Add("Changesets not requested for code review", typeof(int));
            dataTable.Columns.Add("Total Number Of Changesets", typeof(int));

            dataTable.Rows.Add(dataTable.NewRow());

            foreach (var team in teamDetails)
            {

                List<DataRow> Members = new List<DataRow>();

                DataRow teamRow = dataTable.NewRow();
                teamRow["Team"] = team.TeamName;

                if (team.TeamName == "IPW Support Squad")
                {
                    team.Members.AddRange(FindChangsetOwnersWhoAreNotPartOfAnyTeam(teamDetails, codeReviewDetails));
                }
                foreach (var teamMember in team.Members)
                {
                    if (codeReviewDetails.codeReviewCompletedChangesets.Any(c => c.Owner == teamMember) || codeReviewDetails.codeReviewNotDoneChangesets.Any(c => c.Owner == teamMember) || codeReviewDetails.noCodeReviewRequestChangesets.Any(c => c.Owner == teamMember))
                    {
                        DataRow memberRow = dataTable.NewRow();
                        memberRow["Members"] = teamMember;
                        memberRow["Changets having Comments"] = codeReviewDetails.codeReviewCompletedChangesets.Count(changest => changest.Owner == teamMember);
                        memberRow["Changests without completing code review"] = codeReviewDetails.codeReviewNotDoneChangesets.Count(changest => changest.Owner == teamMember);
                        memberRow["Changesets not requested for code review"] = codeReviewDetails.noCodeReviewRequestChangesets.Count(changest => changest.Owner == teamMember);
                        Members.Add(memberRow);
                    }
                }



                teamRow[5] = CountChangsesetsTeamWise(team.Members, codeReviewDetails);
                dataTable.Rows.Add(teamRow);
                Members.ForEach(m => dataTable.Rows.Add(m));
                dataTable.Rows.Add(dataTable.NewRow());
            }

            //Total Calculation
            DataRow totalRow = dataTable.NewRow();
            totalRow["Team"] = "Total";
            totalRow["Changets having Comments"] = codeReviewDetails.codeReviewCompletedChangesets.Count;
            totalRow["Changests without completing code review"] = codeReviewDetails.codeReviewNotDoneChangesets.Count;
            totalRow["Changesets not requested for code review"] = codeReviewDetails.noCodeReviewRequestChangesets.Count;
            totalRow["Total Number Of Changesets"] = codeReviewDetails.codeReviewCompletedChangesets.Count + codeReviewDetails.codeReviewNotDoneChangesets.Count +
                                                     codeReviewDetails.noCodeReviewRequestChangesets.Count;
            dataTable.Rows.Add(totalRow);
            return dataTable;
        }
        private  void CreateExcelReport(CodeReviewDetails codeReviewDetails, string sprint)
        {
            Console.WriteLine("Generating Excel Report");
            var workBook = new XLWorkbook();

            DataTable CodeReviewComments = GetCodeReviewCompletedChangesetsData(codeReviewDetails.codeReviewCompletedChangesets);
            workBook.Worksheets.Add(CodeReviewComments, "CodeReviewComments");
            DataTable CodeReviewNotDoneChangesets = GetCodeReviewNotDoneChangesets(codeReviewDetails.codeReviewNotDoneChangesets);
            workBook.Worksheets.Add(CodeReviewNotDoneChangesets, "RequestedButZeroComments");
            DataTable CodeReviewNotRequested = GetNoCodeReviewRequestChangesets(codeReviewDetails.noCodeReviewRequestChangesets);
            workBook.Worksheets.Add(CodeReviewNotRequested, "CodeReviewNotRequested");
            DataTable teamDetails = GetTeamDetails(GetTeamMembersOfTheSprint(sprint), codeReviewDetails);
            workBook.Worksheets.Add(teamDetails, "TeamDetails");

            _excelService. SetColumnWidths(workBook.Worksheet("CodeReviewComments"), new int[] { 15, 16, 20, 20, 15, 15, 40, 50, 15 });
            _excelService. SetColumnWidths(workBook.Worksheet("RequestedButZeroComments"), new int[] { 15, 20, 20, 40, 20, 22 });
            _excelService. SetColumnWidths(workBook.Worksheet("CodeReviewNotRequested"), new int[] { 15, 20, 20, 40, 20, 22 });
            _excelService. SetColumnWidths(workBook.Worksheet("TeamDetails"), new int[] { 15, 20, 30, 30, 20 });

            _excelService. StyleWorkSheet(workBook.Worksheet("CodeReviewComments"), XLColor.Black, XLBorderStyleValues.Thin, XLTableTheme.TableStyleLight8);
            _excelService. StyleWorkSheet(workBook.Worksheet("RequestedButZeroComments"), XLColor.Black, XLBorderStyleValues.Thin, XLTableTheme.TableStyleLight8);
            _excelService. StyleWorkSheet(workBook.Worksheet("CodeReviewNotRequested"), XLColor.Black, XLBorderStyleValues.Thin, XLTableTheme.TableStyleLight8);
            _excelService. StyleWorkSheet(workBook.Worksheet("TeamDetails"));
           
            string fileName = System.IO.Path.Combine(FileDropLocation, $"CodeReviewDetails_{sprint}.xlsx");
            workBook.SaveAs(fileName);
            Console.WriteLine($"Report is saved at the location {fileName}");
            Console.WriteLine("Generated Excel Report");
        }

        private List<GetTeamDetails> GetTeamMembersOfTheSprint(string sprint)
        {
            List<GetTeamDetails> teamDetails = new List<GetTeamDetails>();

            foreach (string team in teams)
            {
                var teamMembers = new List<string>();

                Guid iterationId;
                var jsonString = _teamFoundationService.InvokeHttpClient($"{_projectName}/{team}/_apis/work/teamsettings/iterations").Content.ReadAsStringAsync().Result;
                var deserialisedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<GetIterationsResponse>(jsonString);
                iterationId = deserialisedObject.Value.First(iteration => iteration.Name == sprint).Id;

                jsonString = _teamFoundationService.InvokeHttpClient($"{_projectName}/{team}/_apis/work/teamsettings/iterations/{iterationId}/capacities").Content.ReadAsStringAsync().Result;
                var deserialiseTeams = Newtonsoft.Json.JsonConvert.DeserializeObject<TeamMemberDetails>(jsonString);
                foreach (var teamMember in deserialiseTeams.Value)
                {
                    if (teamMember.Activities.Any(i => i.CapacityPerDay > 0.0))
                        teamMembers.Add(teamMember.TeamMember.DisplayName);
                }
                teamDetails.Add(new GetTeamDetails { TeamName = team, Members = teamMembers });
            }
            return teamDetails;
        }

        private void AddIntoCompletedOrRequestedCodeReviewChangesets(Changeset changeset, IEnumerable<WorkItem> workItems, List<CodeReviewCompletedChangeset> codeReviewCompletedChangesets, List<CodeReviewNotDoneChangeset> codeReviewNotDoneChangesets)
        {
            List<string> codeReviewAssignedToTheReviewersList = new List<string>();
            List<string> reviewersWhoCompletedReview = new List<string>();
            List<string> finalCodeReviewStatus = new List<string>();
            List<CodeReviewComment> codeReviewComments = new List<CodeReviewComment>();
            var availableStatuses = new List<string> { "Needs Work", "With Comments", "Looks Good" };
            List<string> shelvesetList = new List<string>();
            workItems = workItems.Where(workItem => workItem.Type.Name == codeReviewRequest);

            foreach (WorkItem workItem in workItems)
            {
                string shelveset = workItem.Fields.OfType<Field>().FirstOrDefault(f => f.Name == "Associated Context")?.Value?.ToString();
                shelvesetList.Add(shelveset);
                List<int> relatedLinkIds = workItem.Links.OfType<RelatedLink>().Select(r => r.RelatedWorkItemId).ToList();
                foreach (int relatedLinkId in relatedLinkIds)
                {
                    var item = _teamFoundationService.GetWorkItemById(relatedLinkId).Fields.OfType<Field>();
                    codeReviewAssignedToTheReviewersList.Add(item.FirstOrDefault(f => f.Name == "Reviewed By")?.Value?.ToString());
                    if (!string.IsNullOrEmpty(item.FirstOrDefault(f => f.Name == "Closed By")?.Value?.ToString()))
                    {
                        string closedBy = item.FirstOrDefault(f => f.Name == "Closed By")?.Value?.ToString();
                        if (closedBy != workItem.CreatedBy)
                        {
                            reviewersWhoCompletedReview.Add(closedBy);

                            finalCodeReviewStatus.Add(item.FirstOrDefault(f => f.Name == "Closed Status")?.Value.ToString());
                        }
                    }
                }
                codeReviewComments.AddRange(GetReviewComments(workItem.Id, workItem.CreatedBy));
            }


            string status = string.Empty;
            finalCodeReviewStatus = finalCodeReviewStatus.Where(s => !string.IsNullOrEmpty(s) && availableStatuses.Contains(s)).ToList();
            if (finalCodeReviewStatus.Distinct().Count() == 1)
                status = finalCodeReviewStatus.First();
            else if (finalCodeReviewStatus.Any(s => s == "Needs Work") && (finalCodeReviewStatus.Any(s => s == "With Comments") || finalCodeReviewStatus.Any(s => s == "Looks Good")))
                status = "Needs Work";
            else if (finalCodeReviewStatus.Any(s => s == "With Comments") && finalCodeReviewStatus.Any(s => s == "Looks Good"))
                status = "With Comments";

            if (reviewersWhoCompletedReview.Any())
            {
                if (AreAllFilesOfTheChangsetReviewed(changeset, shelvesetList))
                    CodeReviewCompletedChangesets(changeset, codeReviewCompletedChangesets, reviewersWhoCompletedReview, codeReviewComments, status);
            }
            else
            {
                codeReviewNotDoneChangesets.Add(
                     new CodeReviewNotDoneChangeset
                     {
                         Changeset = changeset.ChangesetId,
                         Title = changeset.Comment,
                         Owner = changeset.CommitterDisplayName,
                         CheckedInDate = changeset.CreationDate.Date,
                         Reviewers = codeReviewAssignedToTheReviewersList
                     });
            }

        }
        private void AddIntoCodeReviewRequestChangesets(Changeset changeset, List<NoCodeReviewRequestChangeset> noCodeReviewRequestChangesets)
        {
            noCodeReviewRequestChangesets.Add(
                new NoCodeReviewRequestChangeset
                {
                    Changeset = changeset.ChangesetId,
                    Owner = changeset.CommitterDisplayName,
                    Title = changeset.Comment,
                    CheckedInDate = changeset.CreationDate.Date
                });
        }
        private IEnumerable<CodeReviewComment> GetReviewComments(int workItemId, string createdBy)
        {

            var discussionThreads = _teamFoundationService.GetReviewComments(workItemId, createdBy); ;
            List<CodeReviewComment> codeReviewComments = new List<CodeReviewComment>();
            foreach (DiscussionThread discussionThread in discussionThreads)
            {
                if (discussionThread.RootComment != null && discussionThread.RootComment.Author.DisplayName != createdBy)
                {
                    codeReviewComments.Add(new CodeReviewComment
                    {
                        Author = discussionThread.RootComment.Author.DisplayName,
                        Comments = discussionThread.RootComment.Content,
                        ReviewedDate = discussionThread.RootComment.PublishedDate.Date
                    });

                }
            }

            return codeReviewComments;

        }
        private bool AreAllFilesOfTheChangsetReviewed(Changeset changeset, List<string> shelvesetList)
        {
            var changesetFilesList = _teamFoundationService.GetFilesAssociatedWithChangeSet(changeset);
            var shelvesetFilesList = _teamFoundationService.GetFilesAssociatedWithCodeReview(shelvesetList);
            return changesetFilesList.All(cf => shelvesetFilesList.Contains(cf));
        }
        private static void CodeReviewCompletedChangesets(Changeset changeset, List<CodeReviewCompletedChangeset> codeReviewCompletedChangesets, List<string> reviewersWhoCompletedReview, List<CodeReviewComment> codeReviewComments, string status)
        {
            codeReviewCompletedChangesets.Add(
                                new CodeReviewCompletedChangeset
                                {
                                    Changeset = changeset.ChangesetId,
                                    Owner = changeset.OwnerDisplayName,
                                    Reviewers = reviewersWhoCompletedReview.Distinct().OrderBy(s => s).ToList(),
                                    CheckedInDate = changeset.CreationDate.Date,
                                    ReviewedDate = codeReviewComments.Min(i => i.ReviewedDate) ?? changeset.CreationDate.Date,
                                    Status = status,
                                    Title = changeset.Comment,
                                    CodeReviewComments = codeReviewComments
                                });
        }

    }
}
