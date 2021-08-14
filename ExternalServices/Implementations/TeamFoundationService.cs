using ExternalServices.Interfaces;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Discussion.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ExternalServices.Implementations
{
    public class TeamFoundationService : ITeamFoundationService
    {
        private WorkItemStore _workItemStore;
        private TfsTeamProjectCollection _projectCollection;
        private VssCredentials _vssCredentials;
        private readonly string _tfsUrlString = ConfigurationManager.AppSettings.Get("TfsUrl").ToString();
        private readonly string _projectName = ConfigurationManager.AppSettings.Get("Project").ToString();
        private readonly string _personalAccessToken = ConfigurationManager.AppSettings.Get("PersonalAccessToken").ToString();
        private readonly string _versionControlPath = ConfigurationManager.AppSettings.Get("VersionControlPath").ToString();

        public TeamFoundationService()
        {
            _vssCredentials = new VssCredentials(true);
            _vssCredentials.PromptType = CredentialPromptType.DoNotPrompt;

            _projectCollection = new TfsTeamProjectCollection(new Uri(_tfsUrlString), _vssCredentials);
            _projectCollection.EnsureAuthenticated();
            _workItemStore = _projectCollection.GetService<WorkItemStore>();

        }
        public WorkItem GetWorkItemById(int workItemId)
        {
            return _workItemStore.GetWorkItem(workItemId);
        }
        private VersionControlServer GetVersionControlServer()
        {
            return _projectCollection.GetService<VersionControlServer>();
        }

        public IEnumerable<Changeset> GetChangesetsInDateRange(DateTime startDate, DateTime endDate)
        {
            VersionSpec fromDateVersion = new DateVersionSpec(startDate);
            VersionSpec toDateVersion = new DateVersionSpec(endDate);

            _projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(_tfsUrlString));
            VersionControlServer versionControl = GetVersionControlServer();
            var path = versionControl.GetTeamProject(_projectName).ServerItem + _versionControlPath;

            return versionControl.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full, "",
                                        fromDateVersion, toDateVersion, int.MaxValue, true, true).OfType<Changeset>().AsEnumerable();
        }

        public IEnumerable<DiscussionThread> GetReviewComments(int workItemId, string createdBy)
        {
            TeamFoundationDiscussionService service = new TeamFoundationDiscussionService();
            service.Initialize(_projectCollection);
            IDiscussionManager discussionManager = service.CreateDiscussionManager();
            IAsyncResult discussions = discussionManager.BeginQueryByCodeReviewRequest(workItemId, QueryStoreOptions.ServerAndLocal, null, null);
            var discussionThreads = discussionManager.EndQueryByCodeReviewRequest(discussions);
            return discussionThreads;
        }

        public List<string> GetFilesAssociatedWithChangeSet(Changeset changeSet)
        {
            List<string> files = new List<string>();

            foreach (var changedItem in changeSet.Changes)
            {
                string item = changedItem.Item.ServerItem.Split('/').LastOrDefault();
                if (!files.Contains(item))
                {
                    files.Add(item);
                }
            }
            return files.Distinct().ToList();

        }
        public List<string> GetFilesAssociatedWithCodeReview(List<string> shelvesetList)
        {
            var files = new List<string>();
            VersionControlServer versionControl = GetVersionControlServer();
            foreach (var set in shelvesetList)
            {
                Shelveset[] shelves = versionControl.QueryShelvesets(set, null);
                Shelveset shelveset = shelves[0];

                PendingSet[] pendingSets = versionControl.QueryShelvedChanges(shelveset);
                foreach (PendingSet pendingSet in pendingSets)
                {
                    PendingChange[] changes = pendingSet.PendingChanges;
                    foreach (PendingChange change in changes)
                    {
                        files.Add(change.FileName);
                    }
                }
            }
            return files.Distinct().ToList();
        }
        public HttpResponseMessage InvokeHttpClient(string requestUri)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_tfsUrlString);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
                httpClient.DefaultRequestHeaders.Host = "alm.eurofins.local";
                return httpClient.GetAsync(requestUri).Result;
            }

        }
    }
}
