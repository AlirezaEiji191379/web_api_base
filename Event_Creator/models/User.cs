using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class User
    {

        public long UserId { get; set; }
        [Required]
        [RegularExpression(" ^[a - zA - Z0 - 9_] * $")]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [RegularExpression("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^\(?([0-9]{4})\)?[-.●]?([0-9]{3})[-.●]?([0-9]{4})$")]
        public string PhoneNumber { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(30)")]
        public string FirstName { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(30)")]
        public string LastName { get; set; }
    }
}
