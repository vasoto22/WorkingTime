using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using WorkingTime.Functions.Entities;
using WorkingTime.Functions.Functions.functionHelper;

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
            TableQuerySegment<ConsolidateEntity> allConsolidateEntity = await workingTimeTable2
                .ExecuteQuerySegmentedAsync(queryConsolidate, null);

            foreach (WorkingEntity date in allEntity)
            {
                if (!string.IsNullOrEmpty(date.IdEmployee.ToString()) && date.Type == 0)
                {
                    foreach (WorkingEntity datetwo in allEntity)
                    {
                        TimeSpan dateCalculated = (datetwo.RegisterTime - date.RegisterTime);

                        if (datetwo.IdEmployee.Equals(date.IdEmployee) && datetwo.Type == 1)
                        {
                            TableOperation updateWorking = TableOperation.Replace(EmployInformation
                                .saveEmployInformation(datetwo));
                            await workingTimeTable.ExecuteAsync(updateWorking);

                            TableOperation updateOtherWorking = TableOperation.Replace(EmployInformation
                                .saveEmployInformation(date));
                            await workingTimeTable.ExecuteAsync(updateOtherWorking);
                            await EmployInformation.collectConsolidateDates(allConsolidateEntity, date, datetwo,
                                dateCalculated, workingTimeTable2);
                        }
                    }
                }
            }
            log.LogInformation("Employee information was successfully consolidated.");
        }
    }
}