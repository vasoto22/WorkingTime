using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using WorkingTime.Functions.Entities;

namespace WorkingTime.Functions.Functions.functionHelper
{

    public class EmployInformation
    {
        //Method for storing employee information when you already have an in and out
        public static WorkingEntity saveEmployInformation(WorkingEntity employData)
        {
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

        //Method for storing consolidated employee information
        public static ConsolidateEntity saveConsolidatedUserInformation(ConsolidateEntity employConsolidate,
        TimeSpan dateCalculated)
        {
            ConsolidateEntity consolidate = new ConsolidateEntity
            {
                IdEmployee = employConsolidate.IdEmployee,
                DateTime = employConsolidate.DateTime,
                MinuteTime = (double)(employConsolidate.MinuteTime + dateCalculated.TotalMinutes),
                PartitionKey = employConsolidate.PartitionKey,
                RowKey = employConsolidate.RowKey,
                ETag = "*"
            };

            return consolidate;
        }

        //Method to create consolidated employee information if you find no record in the table
        public static ConsolidateEntity createConsolidatedUserInformation(WorkingEntity employConsolidate,
        TimeSpan dateCalculated)
        {
            ConsolidateEntity consolidate = new ConsolidateEntity
            {
                IdEmployee = employConsolidate.IdEmployee,
                DateTime = employConsolidate.RegisterTime,
                MinuteTime = dateCalculated.TotalMinutes,
                PartitionKey = "WORKINGCONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            return consolidate;
        }

        //validation of duplicates in the main table and called methods that store as condition is met
        public static async Task collectConsolidateDates(TableQuerySegment<ConsolidateEntity> consolidateEntity,
        WorkingEntity date, WorkingEntity dateTwo, TimeSpan dateCalculated, CloudTable workingTimeTable2)
        {
            if (consolidateEntity.Results.Count == 0)
            {
                TableOperation insertConsolidate = TableOperation.Insert(EmployInformation
                    .createConsolidatedUserInformation(date, dateCalculated));
                await workingTimeTable2.ExecuteAsync(insertConsolidate);
            }
            else
            {
                foreach (ConsolidateEntity itemConsol in consolidateEntity)
                {
                    if (itemConsol.IdEmployee == date.IdEmployee)
                    {
                        TableOperation insertConsolidate = TableOperation.Insert(EmployInformation
                        .saveConsolidatedUserInformation(itemConsol, dateCalculated));
                        await workingTimeTable2.ExecuteAsync(insertConsolidate);
                    }
                    else
                    {
                        TableOperation insertConsolidate = TableOperation.Insert(EmployInformation
                            .createConsolidatedUserInformation(date, dateCalculated));
                        await workingTimeTable2.ExecuteAsync(insertConsolidate);
                    }
                }
            }
        }
    }
}