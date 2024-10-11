using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Request
{
    public class UserRequest
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; }
        [Required]
        public bool Staus { get; set; }
    }
}
