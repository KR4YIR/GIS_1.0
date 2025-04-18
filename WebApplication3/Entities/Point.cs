using WebApplication3.Entities.Common;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3
{
    public class Point : BaseEntity
    {
        public Guid UserId { get; set; }
        public Geometry Location { get; set; }

        public string Name { get; set; }

        // WKT accessor - for API usage
        [NotMapped]
        public string Wkt
        {
            get => Location?.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Location = null;
                    return;
                }

                var reader = new WKTReader();
                Location = reader.Read(value);
            }
        }
    }
}