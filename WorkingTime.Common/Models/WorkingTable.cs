﻿using System;

namespace WorkingTime.Common.Models
{
    public class WorkingTable
    {
        public int IdEmployee { get; set; }

        public DateTime RegisterTime { get; set; }


        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
