using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Exchange
    {
        public long ExchangeId { get; set; }
        public Book bookToExchange { get; set; }
        public User user { get; set; }
        public string bookName { get; set; }

    }
}
