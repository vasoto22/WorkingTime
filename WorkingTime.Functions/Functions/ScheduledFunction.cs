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
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable2,

            ILogger log)
        {
            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>().Where(filter);
            TableQuerySegment<WorkingEntity> allCheckEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);

            //CheckConsolidateEntity = Tabla 2
            TableQuery<ConsolidateEntity> queryConsolidate = new TableQuery<ConsolidateEntity>();
            TableQuerySegment<ConsolidateEntity> allCheckConsolidateEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            bool correctUpdate = false;

            log.LogInformation($"Entrando al primer foreach");
            foreach (WorkingEntity item in allCheckEntity)
            {
                log.LogInformation($"Este es el primer if");
                if (!string.IsNullOrEmpty(item.IdEmployee.ToString()) && item.Type == 0)
                {
                    log.LogInformation($"Este es el segundo foreach");
                    foreach (WorkingEntity itemtwo in allCheckEntity)
                    {
                        TimeSpan dateCalculated = (itemtwo.RegisterTime - item.RegisterTime);
                        log.LogInformation($"Este es el tercer foreach");
                        if (itemtwo.IdEmployee.Equals(item.IdEmployee) && itemtwo.Type == 1)
                        {
                            log.LogInformation($"Este es el IDRowKey, {item.RowKey}, {itemtwo.RowKey}");

                            WorkingEntity check = new WorkingEntity
                            {
                                IdEmployee = itemtwo.IdEmployee,
                                RegisterTime = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = itemtwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = itemtwo.RowKey,
                                ETag = "*"
                            };

                            log.LogInformation($"Este es el cálculo, {dateCalculated}");
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await workingTimeTable.ExecuteAsync(updateCheckEntity);
                            correctUpdate = true;
                        }

                        log.LogInformation($"He estado aquí, {item.RowKey}");
                        if (correctUpdate == true)
                        {
                            WorkingEntity check = new WorkingEntity
                            {
                                IdEmployee = item.IdEmployee,
                                RegisterTime = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = item.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = item.RowKey,
                                ETag = "*"
                            };
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await workingTimeTable.ExecuteAsync(updateCheckEntity);
                        }
                    }
                }
            }
            /* return new OkObjectResult(new ResponseConsolidate
             {
                 Message = "Table",
                 Result = allCheckEntity
             });*/

        }

        /*string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
        TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>().Where(filter);
        TableQuerySegment<WorkingEntity> unconsolidate = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);
        List<WorkingEntity> workinglist = unconsolidate.ToList();

        log.LogInformation($"Los cuchos: {workinglist.Count}");*/
    }
}
