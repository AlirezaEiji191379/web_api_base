using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.models
{
    public class Exchange
    {
        public long ExchangeId { get; set; }
        public Book bookToExchange { get; set; }
        public User user { get; set; }
        [Required(ErrorMessage ="لطفا اسم کتابی که تبادل می کنید را وارد کنید")]
        public string BookName { get; set; }

    }
}
