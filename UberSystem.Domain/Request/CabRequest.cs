using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Request
{
    public class CabRequest
    {
        public string Type { get; set; }
        public string RegNo { get; set; }
    }
}
