using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Service.DTO
{
    public class EmailConfirmationDTO
    {
        public string UserId { get; set; }
        public string Code { get; set; }
    }
}
