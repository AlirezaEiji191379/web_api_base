using Event_Creator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class AddExchangeBookRequest
    {
        public long BookId { get; set; }
        public Exchange exchange { get; set; }

    }
}
