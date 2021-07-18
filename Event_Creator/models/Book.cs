using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Book
    {
        public long BookId{ get; set; }
        public Category category { get; set; }
        public User user;
        public string bookName { get; set; }
        public double? price { get; set; }
        public long addedDate { get; set; }
        public string publisherName { get; set; }
        public ICollection<Exchange> exchanges { get; set; }


    }
}
