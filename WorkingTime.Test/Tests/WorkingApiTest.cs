using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WorkingTime.Common.Models;
using WorkingTime.Functions.Entities;
using WorkingTime.Functions.Functions;
using WorkingTime.Test.Helpers;
using Xunit;

namespace WorkingTime.Test.Tests
{
    public class WorkingApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorking mockCloudTableWorking = new MockCloudTableWorking(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingTable workingRequest = TestFactory.getWorkingRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(workingRequest);

            //Act
            IActionResult response = await WorkingApi.CreateWorking(request, mockCloudTableWorking, logger);



            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorking mockCloudTableWorking = new MockCloudTableWorking(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingTable workingRequest = TestFactory.getWorkingRequest();
            Guid IdEmployee = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdEmployee, workingRequest);

            //Act
            IActionResult response = await WorkingApi.UpdateWorking(request, mockCloudTableWorking, IdEmployee.ToString(), logger);


            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]

        public async void DeleteWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorking mockCloudTableWorking = new MockCloudTableWorking(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid IdEmployee = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdEmployee);
            WorkingEntity workingEntity = TestFactory.GetWorkingEntity();

            //Act
            IActionResult response = await WorkingApi.DeleteWorking(request, workingEntity, mockCloudTableWorking, IdEmployee.ToString(), logger);



            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
         [Fact]

        public void GetWorkingById_Should_Return_200()
        {
            //Arrenge
            //MockCloudTableWorking mockCloudTableWorking = new MockCloudTableWorking(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid IdEmployee = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdEmployee);
            WorkingEntity workingEntity = TestFactory.GetWorkingEntity();

            //Act
            IActionResult response = WorkingApi.GetWorkingById(request, workingEntity, IdEmployee.ToString(), logger);



            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }





       /* [Fact]
        public async void GetAllWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorking mockCloudTableWorking = new MockCloudTableWorking(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            //WorkingTable workingRequest = TestFactory.getWorkingRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //Act
            IActionResult response = await WorkingApi.GetAllWorking(request, mockCloudTableWorking, logger);


            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }*/
    }
}
