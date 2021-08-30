using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using WorkingTime.Common.Models;
using WorkingTime.Common.Responses;
using WorkingTime.Functions.Entities;

namespace WorkingTime.Functions.Functions
{
    public static class WorkingApi
    {
        [FunctionName(nameof(CreateWorking))]
        public static async Task<IActionResult> CreateWorking(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workingTime")] HttpRequest req,
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new working.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkingTable workingTable = JsonConvert.DeserializeObject<WorkingTable>(requestBody);
            if(string.IsNullOrEmpty(workingTable?.IdEmployee.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The request must have a employee identification"
                });
            }

            WorkingEntity workingEntity = new WorkingEntity
            {
                IdEmployee = workingTable.IdEmployee,
                RegisterTime = DateTime.UtcNow,
                Type = workingTable.Type,
                Consolidated = false,
                PartitionKey = "WORKINGTIME",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            TableOperation addOperation = TableOperation.Insert(workingEntity);
            await workingTimeTable.ExecuteAsync(addOperation);

            log.LogInformation("Add new register in table");

            return new OkObjectResult(new Response
            {
                IdEmployee = workingEntity.IdEmployee,
                RegisteredTime = workingEntity.RegisterTime,
                Message = "Information successfully recorded"
            });
        }
    }
}
