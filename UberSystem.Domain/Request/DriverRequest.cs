using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Request
{
    public class DriverRequest
    {

        public string? Dob { get; set; }

        public string? CurrentLocation { get; set; }

        public CabRequest CabRequest { get; set; }
    }
}
