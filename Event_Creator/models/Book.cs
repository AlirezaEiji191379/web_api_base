using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Book
    {
        public long BookId{ get; set; }
        [Required(ErrorMessage ="لطفا دسته بندی را وارد کنید")]
        public long CategoryId { get; set; }
        public Category Category { get; set; }
        public long UserId { get; set; }
        public User user { get; set; }
        [Required(ErrorMessage ="لطفا نام کتاب را وارد کنید")]
        public string BookName { get; set; }
        public double Price { get; set; }
        public long addedDate { get; set; }
        [Required(ErrorMessage ="لطفا نام انتشارات کتاب را وارد کنید")]
        public string PublisherName { get; set; }
        public ICollection<Exchange> exchanges { get; set; }


    }
}
