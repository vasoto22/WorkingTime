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

        /*[FunctionName(nameof(WorkingTest))]
        public static async Task<IActionResult> WorkingTest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TestWorking")] HttpRequest req,
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            ILogger log)
        {
            log.LogInformation($"Prepare to consolidate all registers. Time: {DateTime.Now}");

            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>().Where(filter);
            TableQuerySegment<WorkingEntity> allCheckEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);


            //CheckConsolidateEntity = Tabla 2
            TableQuery<ConsolidateEntity> queryConsolidate = new TableQuery<ConsolidateEntity>();
            TableQuerySegment<ConsolidateEntity> allCheckConsolidateEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            if (allCheckEntity == null)
            {
                return new BadRequestObjectResult(new ResponseConsolidate
                {
                    Message = "Object null"
                });
            }

            log.LogInformation("Entrando al primer foreach");

            foreach (WorkingEntity item in allCheckEntity) //1
            {
                if (item.Type == 0)
                {
                    log.LogInformation("Entrando al segundo foreach");
                    foreach (WorkingEntity itemTwo in allCheckEntity)
                    {
                        log.LogInformation("validado if");
                        if (item.IdEmployee == itemTwo.IdEmployee && itemTwo.Type == 1)
                        {
                            TimeSpan dateCalculated = (itemTwo.RegisterTime - item.RegisterTime);

                            WorkingEntity check = new WorkingEntity
                            {
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = item.RowKey,
                                ETag = "*"
                            };
                            log.LogInformation($"Este es el IDRowKey, {item.RowKey}");

                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await workingTimeTable.ExecuteAsync(updateCheckEntity);

                            

                            foreach (ConsolidateEntity itemConsolidate in allCheckConsolidateEntity)
                            {
                                log.LogInformation("Actualizando consolidado segunda tabla");
                                if (itemConsolidate.IdEmployee == item.IdEmployee)
                                {
                                    ConsolidateEntity checkConsolidate = new ConsolidateEntity
                                    {
                                        IdEmployee = itemConsolidate.IdEmployee,
                                        DateTime = itemConsolidate.DateTime,
                                        MinuteTime = itemConsolidate.MinuteTime + dateCalculated.TotalMinutes
                                    };

                                    TableOperation insertCheckConsolidate = TableOperation.Replace(checkConsolidate);
                                    await workingTimeTable.ExecuteAsync(insertCheckConsolidate);
                                }
                                else
                                {
                                    ConsolidateEntity checkConsolidate = new ConsolidateEntity
                                    {
                                        IdEmployee = item.IdEmployee,
                                        DateTime = item.RegisterTime,
                                        MinuteTime = dateCalculated.TotalMinutes
                                    };

                                    TableOperation insertCheckConsolidate = TableOperation.Insert(checkConsolidate);
                                    await workingTimeTable.ExecuteAsync(insertCheckConsolidate);
                                }
                            }
                        }
                    }
                }
            }*/
        /*[FunctionName(nameof(Test))]
        public static async Task<IActionResult> Test(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Tests")] HttpRequest req,
            [Table("workingTime", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            ILogger log)
        {
            log.LogInformation($"Prepare to consolidate all registers. Time: {DateTime.Now}");

            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingEntity> query = new TableQuery<WorkingEntity>().Where(filter);
            TableQuerySegment<WorkingEntity> allCheckEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);

            //CheckConsolidateEntity = Tabla 2
            TableQuery<ConsolidateEntity> queryConsolidate = new TableQuery<ConsolidateEntity>();
            TableQuerySegment<ConsolidateEntity> allCheckConsolidateEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            bool correctUpdate = false;

            foreach (WorkingEntity item in allCheckEntity)
            {
                if (!string.IsNullOrEmpty(item.IdEmployee.ToString()) && item.Type == 0)
                {
                    foreach (WorkingEntity itemtwo in allCheckEntity)
                    {
                        TimeSpan dateCalculated = (itemtwo.RegisterTime - item.RegisterTime);
                        
                        if (itemtwo.IdEmployee.Equals(item.IdEmployee) && itemtwo.Type == 1)  
                        {
                            log.LogInformation($"Este es el IDRowKey, {item.RowKey}");
                            log.LogInformation($"Este es el cálculo, {dateCalculated}");

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


            return new OkObjectResult(new ResponseConsolidate
            {
                Message = "Table",
                Result = allCheckEntity
            });*/
        //}
    }
}
