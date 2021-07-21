using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        public JsonStatus jsonStatus;
        public int imageCount { get; set; }
        public ICollection<Exchange> exchanges { get; set; }
        public bool Exchangable { get; set; }

        public long views { get; set; }

        public long bookmarks { get; set; }

        public bool ShouldSerializeCategoryId()
        {
            return false;
        }

        public bool ShouldSerializeUserId()
        {
            return false;
        }
        public bool ShouldSerializejsonStatus()
        {
            return false;
        }

        public bool ShouldSerializeuser()
        {
            if(this.jsonStatus==JsonStatus.EnableUserAndCategory)return true;
            return false;
        }

        public bool ShouldSerializeexchanges()
        {
            if (this.jsonStatus == JsonStatus.EnableUserAndCategory) return true;
            return false;
        }

        public bool ShouldSerializeCategory()
        {
            if (this.jsonStatus == JsonStatus.EnableUserAndCategory) return true;
            return false;
        }

    }

    public enum JsonStatus
    {
        DisableUserAndCategory,
        EnableUserAndCategory
    }


}
