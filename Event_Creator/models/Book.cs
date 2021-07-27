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
        public int Publication { get; set; }
        public long addedDate { get; set; }
        [Required(ErrorMessage ="لطفا نام انتشارات کتاب را وارد کنید")]
        public string PublisherName { get; set; }
        public string Writer { get; set; }
        public static JsonStatus jsonStatus;
        public int imageCount { get; set; }
        public ICollection<Exchange> exchanges { get; set; }
        public bool Exchangable { get; set; }

        public long views { get; set; }

        public SellStatus sellStatus { get; set; }

        public long buyerId { get; set; }

        public long bookmarks { get; set; }

        public bool ShouldSerializebuyerId() {return false; }

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
            if (jsonStatus == JsonStatus.EnableUserAndCategory) return true;
            return false;
        }

        public bool ShouldSerializeexchanges()
        {
            if (jsonStatus == JsonStatus.EnableUserAndCategory) return true;
            return false;
        }

        public bool ShouldSerializeCategory()
        {
            if (jsonStatus == JsonStatus.EnableUserAndCategory) return true;
            return false;
        }

    }

    public enum JsonStatus
    {
        EnableUserAndCategory,
        DisableUserAndCategory
    }

    public enum SellStatus
    {
        none,
        AuthenticatedBuyer,
        unAuthenticatedBuyer
    }


}
