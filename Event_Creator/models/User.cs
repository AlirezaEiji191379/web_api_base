using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
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
        [StringLength(255,MinimumLength =6,ErrorMessage ="رمز عبور باید حداقل 6  حرفی باشد")]
        public string Password { get; set; }
        [Required(ErrorMessage ="پست الکترونیکی الزامی است.")]
        [StringLength(60)]
        [RegularExpression("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*",ErrorMessage ="لطفا ایمیل معتبر وارد کنید")]
        public string Email { get; set; }
        [Required(ErrorMessage ="لطفا شماره تلفن همراه خود را وارد کنید")]
        [StringLength(11,MinimumLength =11,ErrorMessage ="شماره تلفن همراه معتبر نمی باشد")]
        [RegularExpression(@"^\(?([0-9]{4})\)?[-.●]?([0-9]{3})[-.●]?([0-9]{4})$",ErrorMessage ="لطفا تلفن معتبر وارد کنید")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage ="لطفا نام خود را وارد کنید")]
        [Column(TypeName ="nvarchar(30)")]
        public string FirstName { get; set; }
        [Required(ErrorMessage ="لطفا نام خانوادگی خود را وارد کنید")]
        [Column(TypeName ="nvarchar(30)")]
        public string LastName { get; set; }
        [Column(TypeName ="nvarchar(255)")] 
        public string Address { get; set; }

        public bool Enable { get; set; }
        [JsonIgnore]
        public Role role { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } 
        public ICollection<Book> books { get; set; }
        public ICollection<Bookmark> bookmarks { get; set; }

        public bool ShouldSerializebookmarks()
        {
            return false;
        }
        //public bool ShouldSerializeUserId()
        //{
        //    return false;
        //}

        public bool ShouldSerializePassword()
        {
            return false;
        }

        public bool ShouldSerializebooks()
        {
            return false;
        }

        public bool ShouldSerializeRefreshTokens()
        {
            return false;
        }

        public bool ShouldSerializeUsername()
        {
            return false;
        }

        public bool ShouldSerializeEnable()
        {
            return false;
        }

        public bool ShouldSerializerole()
        {
            return false;
        }

    }
    public enum Role
    {
        User,
        Admin
    }

}
