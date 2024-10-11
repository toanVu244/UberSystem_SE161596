using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Request
{
    public class AddUserRequest
    {
        [Required]
        public string? Role { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string? Password { get; set; }
        public DriverRequest? DriverRequest { get; set; }

    }
}
