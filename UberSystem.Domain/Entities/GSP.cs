using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Entities
{
    [Table("GSP")]
    public partial class GSP
    {
        [Key]
        public long Id { get; set; }
        public int Index { get; set; }
        public string VehicleID { get; set; }
        public string PStart { get; set; }
        public string PTerm { get; set; }
        public string PEnd { get; set; }

        // Đảm bảo rằng kiểu dữ liệu là nvarchar(1000) cho PreRouted
        [Column(TypeName = "nvarchar(1000)")]
        public string PreRouted { get; set; }

        public int Freg { get; set; }
        public bool Label { get; set; }

        // Đảm bảo rằng kiểu dữ liệu là nvarchar(1000) cho Regions
        [Column(TypeName = "nvarchar(1000)")]
        public string Regions { get; set; }
    }

}
