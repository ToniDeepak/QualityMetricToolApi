using System;


namespace BusinessLayer.BusinessLayerDtos
{
    public class GetPeerReviewRequest
    {
        public string sprintName { get; set; }
        public DateTime sprintStartDate { get; set; }
        public DateTime sprintEndDate { get; set; }
        public string applicationName { get; set; }
    }
}
