using System;


namespace BusinessLayer.BusinessLayerDtos
{
    public class NoCodeReviewRequestChangeset
    {
        public int Changeset { get; set; }
        public string Title { get; set; }
        public string Owner { get; set; }
        public DateTime CheckedInDate { get; set; }
    }
}
