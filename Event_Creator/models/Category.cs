using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Category
    {
        public long CategoryId { get; set;}
        [Required(ErrorMessage ="لطفا نام دسته بندی را وارد نمایید")]
        public string CategoryName { get; set; }
        [Required(ErrorMessage ="لطفا دسته بندی مادر را وارد کنید")]
        public int ParentId { get; set; }

        public ICollection<Book> books { get; set; }

        public bool ShouldSerializebooks()
        {
            return false;
        }

        public bool ShouldSerializeParentId()
        {
            return false;
        }

    }
}
