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
        [Required(ErrorMessage ="نام کاربری الزامیست")]
        [RegularExpression("^[a-zA-Z0-9]*$",ErrorMessage ="نام کاربری باید شامل ارقام و حروف انگلیسی باشد")]
        [StringLength(20,MinimumLength =8,ErrorMessage ="نام کاربری باید حداقل 8 و حداکثر 20 حرف باشد")]
        public string Username { get; set; }
        [Required(ErrorMessage ="رمز عبور الزامی است")]
        [StringLength(80,MinimumLength =6,ErrorMessage ="رمز عبور باید حداقل 6  حرفی باشد")]
        public string Password { get; set; }
        [Required(ErrorMessage ="پست الکترونیکی الزامی است.")]
        [StringLength(60)]
        [RegularExpression("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*",ErrorMessage ="لطفا ایمیل معتبر وارد کنید")]
        public string Email { get; set; }
        [Required]
        [StringLength(11,MinimumLength =11,ErrorMessage ="شماره تلفن همراه معتبر نمی باشد")]
        [RegularExpression(@"^\(?([0-9]{4})\)?[-.●]?([0-9]{3})[-.●]?([0-9]{4})$")]
        public string PhoneNumber { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(30)")]
        public string FirstName { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(30)")]
        public string LastName { get; set; }
        public bool Enable { get; set; }

        public Role role { get; set; }

    }
    public enum Role
    {
        User,
        Admin
    }

}
