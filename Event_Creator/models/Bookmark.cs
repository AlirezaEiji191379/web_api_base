using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Bookmark
    {
        public long BookmarkId { get; set; }
        public long userId { get; set; }
        public User user { get; set; }
        public Book book { get; set; }

        public bool ShouldSerializeuser() => false;
        public bool ShouldSerializeuserId() => false;
        

    }
}
