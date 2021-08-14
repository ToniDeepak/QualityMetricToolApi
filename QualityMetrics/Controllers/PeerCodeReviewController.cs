using BusinessLayer.Interfaces;

using System.Web.Http;

namespace QualityMetrics.Controllers
{
    [RoutePrefix("api/PeerCodeReview")]
    public class PeerCodeReviewController : ApiController
    {
        private IPeerCodeReviewService _peerCodeReviewService;

        public PeerCodeReviewController(IPeerCodeReviewService peerCodeReviewService)
        {
            this._peerCodeReviewService = peerCodeReviewService;
        }
        [Route("ExtractAndSaveReviewDetails")]
        [HttpPost]
        public IHttpActionResult ExtractAndSaveReviewDetails([FromBody] BusinessLayer.BusinessLayerDtos.GetPeerReviewRequest getPeerReviewRequest)
        {
            _peerCodeReviewService.GetCodeReviewDetails(getPeerReviewRequest);
            return Ok();
        }
    }
}