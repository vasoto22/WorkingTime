using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WorkingTime.Functions.Entities
{
    public class ConsolidateEntity : TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime DateTime { get; set; }
        public double MinuteTime { get; set; }
    }
}
