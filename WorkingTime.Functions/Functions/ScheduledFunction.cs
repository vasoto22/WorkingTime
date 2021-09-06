using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using WorkingTime.Functions.Entities;

namespace WorkingTime.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName(nameof(ProgrammerTime))]
        public static async Task ProgrammerTime(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            [Table("workingConsolidated", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable2,
            ILogger log)
        {
            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>().Where(filter);
            TableQuerySegment<WorkingEntity> allEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);

            //CheckConsolidateEntity = Tabla 2
            TableQuery<ConsolidateEntity> queryConsolidate = new TableQuery<ConsolidateEntity>();
            TableQuerySegment<ConsolidateEntity> allConsolidateEntity = await workingTimeTable2.ExecuteQuerySegmentedAsync(queryConsolidate, null);


            log.LogInformation($"Entrando al primer foreach");
            foreach (WorkingEntity date in allEntity)
            {
                log.LogInformation($"Este es el primer if");
                if (!string.IsNullOrEmpty(date.IdEmployee.ToString()) && date.Type == 0)
                {
                    log.LogInformation($"Este es el PRIMER IF foreach");
                    foreach (WorkingEntity datetwo in allEntity)
                    {
                        TimeSpan dateCalculated = (datetwo.RegisterTime - date.RegisterTime);
                        log.LogInformation($"Este es el SEGUNDO foreach");
                        if (datetwo.IdEmployee.Equals(date.IdEmployee) && datetwo.Type == 1)
                        {
                            log.LogInformation($"Este es el IDRowKey, {date.RowKey}, {datetwo.RowKey}");

                            WorkingEntity working = new WorkingEntity
                            {
                                IdEmployee = datetwo.IdEmployee,
                                RegisterTime = datetwo.RegisterTime,
                                Type = datetwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = datetwo.RowKey,
                                ETag = "*"
                            };
                            WorkingEntity otherWorking = new WorkingEntity
                            {
                                IdEmployee = date.IdEmployee,
                                RegisterTime = date.RegisterTime,
                                Type = date.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = date.RowKey,
                                ETag = "*"
                            };
                            TableOperation updateWorking = TableOperation.Replace(working);
                            await workingTimeTable.ExecuteAsync(updateWorking);

                            TableOperation updateOtherWorking = TableOperation.Replace(otherWorking);
                            await workingTimeTable.ExecuteAsync(updateOtherWorking);
                            log.LogInformation($"Este es el SALI DEL SEGUNDO FOR EACH foreach");
                            await el_chocolero(allConsolidateEntity, date, datetwo, dateCalculated,workingTimeTable2);
                        }
                    }
                }
            }
        }

        public static async Task el_chocolero(TableQuerySegment<ConsolidateEntity> consolidateEntity, WorkingEntity date, WorkingEntity dateTwo, TimeSpan dateCalculated, CloudTable workingTimeTable2)
        {
            if (consolidateEntity.Results.Count == 0)
            {
                ConsolidateEntity checkConsolidate = new ConsolidateEntity
                {
                    IdEmployee = date.IdEmployee,
                    DateTime = date.RegisterTime,
                    MinuteTime = dateCalculated.TotalMinutes,
                    PartitionKey = "WORKINGCONSOLIDATED",
                    RowKey = Guid.NewGuid().ToString(),
                    ETag = "*"
                };

                TableOperation insertCheckConsolidate = TableOperation.Insert(checkConsolidate);
                await workingTimeTable2.ExecuteAsync(insertCheckConsolidate);
            }
            else
            {
                foreach (ConsolidateEntity itemConsolidate in consolidateEntity)
                {
                    //log.LogInformation("Actualizando consolidado segunda tabla");
                    if (itemConsolidate.IdEmployee == dateTwo.IdEmployee)
                    {
                        ConsolidateEntity checkConsolidateFor = new ConsolidateEntity
                        {
                            IdEmployee = itemConsolidate.IdEmployee,
                            DateTime = itemConsolidate.DateTime,
                            MinuteTime = (double)(itemConsolidate.MinuteTime + dateCalculated.TotalMinutes),
                            PartitionKey = itemConsolidate.PartitionKey,
                            RowKey = itemConsolidate.RowKey,
                            ETag = "*"
                        };

                        TableOperation insertConsolidate = TableOperation.Replace(checkConsolidateFor);
                        await workingTimeTable2.ExecuteAsync(insertConsolidate);
                    }
                    else
                    {
                        ConsolidateEntity checkConsolidateFor = new ConsolidateEntity
                        {
                            IdEmployee = date.IdEmployee,
                            DateTime = date.RegisterTime,
                            MinuteTime = dateCalculated.TotalMinutes,
                            PartitionKey = "WORKINGCONSOLIDATED",
                            RowKey = Guid.NewGuid().ToString(),
                            ETag = "*"
                        };

                        TableOperation insertConsolidate = TableOperation.Insert(checkConsolidateFor);
                        await workingTimeTable2.ExecuteAsync(insertConsolidate);
                    }
                }
            }
        }
    }
}
