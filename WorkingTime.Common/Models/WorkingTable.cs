using System;
using System.Collections.Generic;
using System.Text;

namespace WorkingTime.Common.Models
{
    class WorkingTable
    {
        public int IdEmployee { get; set; }

        public DateTime RegisterTime { get; set; }


        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
