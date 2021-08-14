using System.Collections.Generic;

namespace BusinessLayer.BusinessLayerDtos
{
    public class GetTeamDetails
    {
        public string TeamName { get; set; }
        public List<string> Members { get; set; }
    }
}
