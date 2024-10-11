using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Entities
{
    [Table("RefreshToken")]
    public partial class RefreshToken
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey("UserId")]
        public long UserId { get; set; }
        public User User { get; set; }
        public string JwtId {  get; set; }
        public string Token {  get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked {  get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime IssuedAt { get; set; }
    }

}
