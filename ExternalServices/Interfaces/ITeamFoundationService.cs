
using Microsoft.TeamFoundation.Discussion.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ExternalServices.Interfaces
{
    public interface ITeamFoundationService
    {
        IEnumerable<Changeset> GetChangesetsInDateRange(DateTime startDate,DateTime endDate);
        WorkItem GetWorkItemById(int workItemId);
        IEnumerable<DiscussionThread> GetReviewComments(int workItemId, string createdBy);
        List<string> GetFilesAssociatedWithCodeReview(List<string> shelvesetList);
        List<string> GetFilesAssociatedWithChangeSet(Changeset changeSet);
        //void GetTeamMembersOfTheSprint(string sprint);
        HttpResponseMessage InvokeHttpClient(string requestUri);
    }
}
