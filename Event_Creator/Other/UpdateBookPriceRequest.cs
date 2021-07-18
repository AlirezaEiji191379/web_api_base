using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class UpdateBookPriceRequest
    {
        public long BookId { get; set; }
        public double Price { get; set; }

    }
}
