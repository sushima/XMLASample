using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AnalysisServices.AdomdClient;

namespace XMLASample
{
    public static class GetDataJson
    {
        [FunctionName("GetDataJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get catalog name and query from http argument
            string query = req.Query["query"];
            string catalog = req.Query["catalog"];
            try {
                var tabularResults = new DataTable();

                // Get the values needed for the connection
                string appId = Environment.GetEnvironmentVariable("appId", EnvironmentVariableTarget.Process);
                string authKey = Environment.GetEnvironmentVariable("authKey", EnvironmentVariableTarget.Process);
                string PBI_CONNECTION_STRING = Environment.GetEnvironmentVariable("PBI_CONNECTION_STRING", EnvironmentVariableTarget.Process);
                string connectionString = $"Provider=MSOLAP.8;{PBI_CONNECTION_STRING};Initial Catalog={catalog};User ID={appId};Password={authKey};";

                using (var connection = new AdomdConnection(connectionString))
                {
                    connection.Open();
                    var currentDataAdapter = new AdomdDataAdapter(query, connection);
                    currentDataAdapter.Fill(tabularResults);

                    // 
                    string json = JsonConvert.SerializeObject(tabularResults, Formatting.None);
                    return new OkObjectResult(json);
                }
            } 
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Query={query} Message={ex.Message}");
            }
                
        }
    }
}
