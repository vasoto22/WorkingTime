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
            if (string.IsNullOrEmpty(workingTable?.IdEmployee.ToString()))
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

        [FunctionName(nameof(UpdateWorking))]
        public static async Task<IActionResult> UpdateWorking(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "workingTime/{IdEmployee}")] HttpRequest req,
           [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
           string IdEmployee,
           ILogger log)
        {
            log.LogInformation($"Updating user registered: {IdEmployee}, in the table workingTime.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkingTable workingTable = JsonConvert.DeserializeObject<WorkingTable>(requestBody);

            TableOperation findOperation = TableOperation.Retrieve<WorkingEntity>("WORKINGTIME", IdEmployee);
            TableResult findEmployeeResult = await workingTimeTable.ExecuteAsync(findOperation);

            //Validate idEmployee, find
            if (findEmployeeResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = $"The employee with the id: {IdEmployee}, was not found"
                });
            }

            WorkingEntity workingEntity = (WorkingEntity)findEmployeeResult.Result;
            workingEntity.RegisterTime = workingTable.RegisterTime;
            workingEntity.Type = workingTable.Type;




            if (string.IsNullOrEmpty(workingTable.Type.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "You must enter a type"
                });
            }

            TableOperation substituteOperation = TableOperation.Replace(workingEntity);
            await workingTimeTable.ExecuteAsync(substituteOperation);

            log.LogInformation($"Update a register in table, id:{IdEmployee}");

            return new OkObjectResult(new Response
            {
                IdEmployee = workingEntity.IdEmployee,
                Message = "Information successfully update"
            });
        }

    }
}
