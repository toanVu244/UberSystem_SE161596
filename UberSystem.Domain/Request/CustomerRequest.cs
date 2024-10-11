using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Request
{
    public class CustomerRequest
    {
        public long Id { get; set; }

        public byte[] CreateAt { get; set; } = null!;

        public long? UserId { get; set; }
    }
}
