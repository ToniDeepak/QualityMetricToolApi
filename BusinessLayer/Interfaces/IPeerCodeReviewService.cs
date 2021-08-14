

using BusinessLayer.BusinessLayerDtos;
using System;

namespace BusinessLayer.Interfaces
{
    public interface IPeerCodeReviewService
    {
        void GetCodeReviewDetails(GetPeerReviewRequest getPeerReviewRequest);
    }
}
