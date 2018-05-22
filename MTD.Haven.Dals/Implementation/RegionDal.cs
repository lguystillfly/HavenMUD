using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTD.Haven.Domain;
using MTD.Haven.Domain.Models.Entities;
using MTD.Haven.Domain.Models.World;
using Newtonsoft.Json;

namespace MTD.Haven.Dals.Implementation
{
    public class RegionDal : IRegionDal
    {
        public List<Region> GetRegions()
        {
            var regions = new List<Region>();

            foreach (var p in GetRegionFiles())
            {
                regions.Add(JsonConvert.DeserializeObject<Region>(File.ReadAllText(p)));
            }

            return regions;
        }

        private List<string> GetRegionFiles()
        {
            return Directory.GetFiles(Constants.RegionDirectory).ToList();
        }
    }
}