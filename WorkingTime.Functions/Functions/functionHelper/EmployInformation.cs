using System;
using WorkingTime.Functions.Entities;

namespace WorkingTime.Functions.Functions.functionHelper
{

    public class EmployInformation
    {
    
        public static WorkingEntity saveEmployInformation(WorkingEntity employData) {
            WorkingEntity working = new WorkingEntity
            {
                IdEmployee = employData.IdEmployee,
                RegisterTime = employData.RegisterTime,
                Type = employData.Type,
                Consolidated = true,
                PartitionKey = "WORKINGTIME",
                RowKey = employData.RowKey,
                ETag = "*"
            };
            
            return working;
        }
        /*
        public static ConsolidateEntity saveConsolidatedEmployInformation(ConsolidateEntity employData) {
            ConsolidateEntity validateConsolidate = new ConsolidateEntity
            {
                IdEmployee = employData.IdEmployee,
                DateTime = employData.RegisterTime,
                MinuteTime = employData.TotalMinutes,
                PartitionKey = "WORKINGCONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            return validateConsolidate;
        }*/
    }
}