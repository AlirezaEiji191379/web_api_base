using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Category
    {
        public long CategoryId { get; set;}
        public string CategoryName { get; set; }
        public Category parent { get; set; }

    }
}
