using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Request
{
    public class BookingRequest
    {
        [Required]
        public long CustomerID { get; set; }
        [Required]
        public string Source { get; set; }
        [Required]
        public string Destination { get; set; }
    }
}
