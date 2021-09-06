using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WorkingTime.Functions.Entities
{
    public class WorkingEntity : TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime RegisterTime { get; set; }
        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
