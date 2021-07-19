using Event_Creator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class BookResponse
    {

        public Book book { get; set; }
        public string userPhone { get; set; }
        public string userEmail { get; set; }
    }
}
