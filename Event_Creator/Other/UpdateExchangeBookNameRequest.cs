using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other
{
    public class UpdateExchangeBookNameRequest
    {
        public long BookId { get; set; }
        public long ExchangeId { get; set; }
        public string BookName { get; set; }

    }
}
