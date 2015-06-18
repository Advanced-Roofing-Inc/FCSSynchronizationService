using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json;
using RestSharp;

namespace FCSSynchronizationService
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Running FCS Synchronization Service");

         var connectionString = ConfigurationManager.ConnectionStrings["Solomon2"].ConnectionString;
         var connection = new SqlConnection(connectionString);
         connection.Open();

         // Create the api request
         var request = new ApiRequest
         {
            api_key = ConfigurationManager.AppSettings["ApiKey"],
            master_id = ConfigurationManager.AppSettings["ApiMasterId"],
            batch = new List<Dictionary<string, object>>()
         };

         // Fetch employee data and add to request batch
         using (connection)
         {
            var queryText = "select employeeid, employeerate from smEmp where EmployeeActive = 1";
            var command = new SqlCommand(queryText, connection);
            SqlDataReader reader = null;
            reader = command.ExecuteReader();

            int burdenRate = Convert.ToInt32(ConfigurationManager.AppSettings["BurdenRate"]);

            while (reader.Read())
            {
               var dict = new Dictionary<string, object>();
               dict.Add("employee_id", reader["employeeid"].ToString());
               dict.Add("rate", Convert.ToDouble(reader["employeerate"]));
               dict.Add("type", "labor");
               dict.Add("burden", burdenRate);

               request.batch.Add(dict);
            }
         }

         connection.Close();

         // Send the request
         var requestUrl = ConfigurationManager.AppSettings["ApiUrl"];

         var client = new RestClient(requestUrl);

         foreach (var employee in request.batch)
         {
            var restRequest = new RestRequest(Method.POST);
            restRequest.AddParameter("api_key", request.api_key);
            restRequest.AddParameter("master_id", request.master_id);
            restRequest.AddParameter("batch[0][type]", "labor");
            restRequest.AddParameter("batch[0][employee_id]", employee["employee_id"].ToString());
            restRequest.AddParameter("batch[0][rate]", employee["rate"].ToString());

            IRestResponse response = client.Execute(restRequest);
            Console.WriteLine("Response Status: {0}", (int)response.StatusCode);
            Console.WriteLine("Response: {0}", response.Content);
         }

         //Console.WriteLine("Press any key to exit...");
         //Console.ReadKey();
      }
   }
}
