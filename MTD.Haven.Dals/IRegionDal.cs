using MTD.Haven.Domain.Models.World;
using System.Collections.Generic;

namespace MTD.Haven.Dals
{
    public interface IRegionDal
    {
        List<Region> GetRegions();
    }
}