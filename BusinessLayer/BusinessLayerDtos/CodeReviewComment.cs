using System;


namespace BusinessLayer.BusinessLayerDtos
{
    public class CodeReviewComment
    {
        public string Author { get; set; }
        public string Comments { get; set; }
        public DateTime? ReviewedDate { get; set; }
    }
}
