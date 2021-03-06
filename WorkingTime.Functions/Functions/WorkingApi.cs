using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
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

        [FunctionName(nameof(GetAllWorking))]
        public static async Task<IActionResult> GetAllWorking(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workingTime")] HttpRequest req,
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            ILogger log)
        {
            log.LogInformation("All jobs recieved.");

            TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>();
            TableQuerySegment<WorkingEntity> workings = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);

            log.LogInformation("Retrieved all workings");

            return new OkObjectResult(new Response
            {
                Message = "Retrieved all workings",
                Result = workings
            });
        }

        [FunctionName(nameof(GetWorkingById))]
        public static IActionResult GetWorkingById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workingTime/{IdEmployee}")] HttpRequest req,
            [Table("workingTime", "WORKINGTIME", "{IdEmployee}", Connection = "AzureWebJobsStorage")] WorkingEntity workingEntity,
            string IdEmployee,
            ILogger log)
        {
            log.LogInformation($"Get working by Id: {IdEmployee}, recieved.");

            if (workingEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Working not found."
                });
            }

            log.LogInformation($"Working: {workingEntity.RowKey}, retrieved.");

            return new OkObjectResult(new Response
            {
                Message = "Retrieved working",
                Result = workingEntity
            });
        }

        [FunctionName(nameof(DeleteWorking))]
        public static async Task<IActionResult> DeleteWorking(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "workingTime/{IdEmployee}")] HttpRequest req,
            [Table("workingTime", "WORKINGTIME", "{IdEmployee}", Connection = "AzureWebJobsStorage")] WorkingEntity workingEntity,
             [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            string IdEmployee,
            ILogger log)
        {
            log.LogInformation($"Delete working: {IdEmployee}, recieved.");

            if (workingEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Working not found."
                });
            }

            await workingTimeTable.ExecuteAsync(TableOperation.Delete(workingEntity));
            log.LogInformation($"Working: {workingEntity.RowKey}, deleted.");

            return new OkObjectResult(new Response
            {
                Message = "Deleted working",
                Result = workingEntity
            });
        }

        [FunctionName(nameof(GetConsolidated))]
        public static async Task<IActionResult> GetConsolidated(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workingConsolidated/{Date}")] HttpRequest req,
            [Table("workingConsolidated", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable2,
            DateTime Date,
            ILogger log)
        {
            log.LogInformation("Recieved a new register");
            string filter = TableQuery.GenerateFilterConditionForDate("DateTime", QueryComparisons.Equal, Date);
            TableQuery<ConsolidateEntity> query = new TableQuery<ConsolidateEntity>().Where(filter);
            TableQuerySegment<ConsolidateEntity> allCheckConsolidateEntity = await workingTimeTable2.ExecuteQuerySegmentedAsync(query, null);

            if (allCheckConsolidateEntity == null || allCheckConsolidateEntity.Results.Count.Equals(0))
            {
                return new OkObjectResult(new ResponseConsolidate
                {
                    Message = "Date not found.",
                });
            }
            else
            {
                return new OkObjectResult(new ResponseConsolidate
                {
                    Message = $"Get the register from consolidate. Date:{Date}",
                    Result = allCheckConsolidateEntity
                });
            }
        }

    }
}
