using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Devices;

namespace HomeIoTFunctions20.ShellyDoorSensor
{
    public static class CloseGate
    {
        [FunctionName("CloseGate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var connectionString = config["IoTHubConnectionString"];

            var sendData = new
            {
                DeviceID = "Shelly",
                DateAndTime = GetEnergyMarketPrice.GetEnergyMarketPrice.DateTimeTZ(),
                isGateOpen = false
            };
            string requestBody = JsonConvert.SerializeObject(sendData);

            var serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            var cloudToDeviceMethod = new CloudToDeviceMethod("GateCommand")
            {
                ConnectionTimeout = TimeSpan.FromSeconds(5),
                ResponseTimeout = TimeSpan.FromSeconds(5)
            };

            cloudToDeviceMethod.SetPayloadJson(requestBody);
            var response = await serviceClient.InvokeDeviceMethodAsync("HomeEdgeDevice", "HomeModule", cloudToDeviceMethod).ConfigureAwait(false);
            var json = response.GetPayloadAsJson();

            return new OkObjectResult($"Ok");
        }
    }
}