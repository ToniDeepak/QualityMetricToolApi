using System.Collections.Generic;

namespace BusinessLayer.BusinessLayerDtos
{
    class CodeReviewDetails
    {
        public List<CodeReviewCompletedChangeset> codeReviewCompletedChangesets { get; set; }
        public List<NoCodeReviewRequestChangeset> noCodeReviewRequestChangesets { get; set; }
        public List<CodeReviewNotDoneChangeset> codeReviewNotDoneChangesets { get; set; }
    }
}
