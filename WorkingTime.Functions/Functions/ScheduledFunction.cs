using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using WorkingTime.Functions.Entities;

namespace WorkingTime.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName(nameof(ProgrammerTime))]
        public static async Task ProgrammerTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consolidated")] HttpRequest req,
           // [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,

            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>().Where(filter);
            TableQuerySegment<WorkingEntity> unconsolidate = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);
            List<WorkingEntity> workinglist = unconsolidate.ToList();

            log.LogInformation($"Los cuchos: {workinglist.Count}");
        }
    }
}
