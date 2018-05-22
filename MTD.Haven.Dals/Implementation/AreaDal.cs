using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTD.Haven.Domain;
using MTD.Haven.Domain.Models.World;
using Newtonsoft.Json;

namespace MTD.Haven.Dals.Implementation
{
    public class AreaDal : IAreaDal
    {
        public List<Area> GetAreas()
        {
            var areas = new List<Area>();

            foreach (var p in GetAreaFiles())
            {
                areas.Add(JsonConvert.DeserializeObject<Area>(File.ReadAllText(p)));
            }

            return areas;
        }

        private List<string> GetAreaFiles()
        {
            return Directory.GetFiles(Constants.AreaDirectory).ToList();
        }
    }
}