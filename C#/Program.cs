using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using static TestingAuth.Sample.Api.Get.ProcessClaimsShield_API;

namespace TestingAuth.Sample.Api.Get
{

   public struct Claim
   {

      public string type;
      public string processed;
      public string id;

   }

   class Program
   {
      static async Task Main(string[] args)
      {
         #region Get duplicates for Merge
         List<MergeObjAPI> duplicates = new List<MergeObjAPI>();

         string fileLocation = @"C:\Users\CYAN1\Desktop\duplicates.csv";

         try {
            System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
            string currentLine = file.ReadLine();

            while (!file.EndOfStream) {
               currentLine = file.ReadLine();
               var cols = currentLine.Split(',');

               string s = cols[1];
               string d = cols[0];

               var duplicate = new MergeObjAPI() { sourcePatientId = s, destinationPatientId = d };
               duplicates.Add(duplicate);
            }
         } catch (Exception e) {
            int p = 0;
         }
         #endregion

         #region Get Claims to be processed
         //List<Claim> claims = new List<Claim>();

         //string fileLocation = @"C:\Users\CYAN1\Desktop\OpenClaims.csv";

         //try {
         //   System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
         //   string currentLine = file.ReadLine();

         //   while (!file.EndOfStream) {
         //      currentLine = file.ReadLine();
         //      var cols = currentLine.Split(',');

         //      string id = cols[0];
         //      string processed = cols[1];
         //      string type = cols[2];

         //      Claim claim = new Claim() { id = id, processed = processed, type = type };

         //      claims.Add(claim);

         //   }
         //} catch (Exception e) {
         //   int p = 0;
         //}
         #endregion

       
         var configuration = new ConfigurationBuilder()
             .AddEnvironmentVariables()
             .AddJsonFile("appsettings.json", true, true)
             .Build();

         #region PROD
         var tenantId = "7af37f6c-082f-40cd-b11b-e5a64696d758";
         var clientId = "c5b84547-0c65-4259-bc29-cc737782457d";
         var clientSecret = "esA8Q~YzHKkMXBHuXC8SqeOxobraXRFLkUKJ3dpY";
         var b2Cauthority = @"https://login.microsoftonline.com/7af37f6c-082f-40cd-b11b-e5a64696d758/v2.0/";


         var ServiceBaseUrl = Environments.Production.BaseUrl;
         var ServiceScope = Environments.Production.Scope;
         #endregion

         #region QA
         //var tenantId = "0e754259-60f4-48f9-b2cc-425d83a53a9e";
         //var clientId = "0b33f54d-70a1-4caf-8176-59af23f51a94";
         //var clientSecret = "lvD8Q~L5Gzoh5oP3dqeQZdJwiz6l7osZr~HiEc8m";
         //var b2Cauthority = @"https://login.microsoftonline.com/0e754259-60f4-48f9-b2cc-425d83a53a9e/v2.0/";

         //var ServiceBaseUrl = Environments.Test.BaseUrl;
         //var ServiceScope = Environments.Test.Scope;
         #endregion

         var token = await GetAccessToken(tenantId, clientId, clientSecret, b2Cauthority, ServiceScope);
         var client = GetHttpClient(token);



         #region Create Claims
         //int count = 0;
         //List<Claim> batch = new List<Claim>();

         //for (int pos = 0; pos < claims.Count; pos++) {

         //   if ((pos > 0 && claims[pos].type != claims[pos - 1].type) || (pos > 0 && claims[pos].processed != claims[pos - 1].processed)) {
         //      count = 25;
         //   } else {
         //      batch.Add(claims[pos]);
         //      count++;
         //   }

         //   if (count == 25 || pos == claims.Count - 1) {
         //      //currentBatch = new string[] { "fcd99cbf-2bde-497d-b352-25a0374b95e6","55626683-f91b-4fde-aa4c-7065d3f6f9b8","90276bd8-7097-4c62-a844-b981fb9b61b8","ec59e969-4d98-497a-adc0-a7b5e2ffb15d"};

         //      try {
         //         await ProcessClaims(client, ServiceBaseUrl, batch);
         //      } catch (Exception e) {
         //         string err = e.ToString();
         //      }

         //      System.Threading.Thread.Sleep(500);

         //      count = 0;
         //      batch = new List<Claim>();
         //   }
         //}
         #endregion


         #region merge duplicates
         foreach (MergeObjAPI duplicate in duplicates) {
            try {
               await MergePatients(client, ServiceBaseUrl, duplicate.sourcePatientId, duplicate.destinationPatientId);
            } catch (Exception e) {
               string err = e.ToString();
            }
         }
         #endregion

         // await GetLabOrders(client, TestingServiceBaseUrl);

      }

      private static async Task MergePatients(HttpClient client, string baseUrl, string sourcePatientId, string destinationPatientId)
      {
         MergeObjAPI table = new MergeObjAPI();
         table.sourcePatientId = sourcePatientId;
         table.destinationPatientId = destinationPatientId;
         string json = JsonConvert.SerializeObject(table);

         StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

         var response = await client.PostAsync($"{baseUrl}/Patient/merge/{sourcePatientId}/into/{destinationPatientId}", httpContent);

         response.EnsureSuccessStatusCode();

         var responseInfo = await response.Content.ReadAsStringAsync();
         Console.WriteLine(responseInfo);
      }

      private static async Task ProcessClaims(HttpClient client, string baseUrl, List<Claim> claims)
      {
         string type = claims[0].type.ToUpper();
         string processed = claims[0].processed.ToUpper();

         List<string> ids =  new List<string>();
         foreach(Claim claim in claims)
            ids.Add(claim.id);

         

         string[] currentBatch = ids.ToArray();

         HttpResponseMessage response = null;
         string json = string.Empty;

         if (type == "TEST") {
            ProcessClaimsTests_API table = new ProcessClaimsTests_API();
            table.testResultIds = currentBatch;
            json = JsonConvert.SerializeObject(table);
         } else if (type == "VAX") {
            ProcessClaimsVAX_API table = new ProcessClaimsVAX_API();
            table.vaccinationIds = currentBatch;
            json = JsonConvert.SerializeObject(table);
         } else if (type == "CHLA") {
            ProcessClaimsCHLA_API table = new ProcessClaimsCHLA_API();
            table.chlaServiceIds = currentBatch;
            json = JsonConvert.SerializeObject(table);
         } else if (type == "SHIELD") {
            ProcessClaimsShield_API table = new ProcessClaimsShield_API();
            table.shieldServiceIds = currentBatch;
            json = JsonConvert.SerializeObject(table);
         }

         if (processed == "YES") {
            response = await client.PutAsync($"{baseUrl}/ServiceClaimAudit/unmatched", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
         } else {
            response = await client.PostAsync($"{baseUrl}/ServiceClaimAudit/unmatched", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
         }

         if (response != null) {
            response.EnsureSuccessStatusCode();
            var responseInfo = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseInfo);
         } else {
            throw new Exception("No response");
         }
      }




      private static async Task<string> GetAccessToken(string tenantId, string clientId, string clientSecret, string b2Cauthority, params string[] scopes)
      {
         var app = ConfidentialClientApplicationBuilder.Create(clientId)
             .WithAuthority(b2Cauthority)
             .WithTenantId(tenantId)
             .WithClientSecret(clientSecret)
             .Build();

         var result = await app.AcquireTokenForClient(scopes)
             .ExecuteAsync();

         return result.AccessToken;
      }

      private static HttpClient GetHttpClient(string accessToken)
      {
         // Use HttpClientFactory when using this for real
         var client = new HttpClient();
         client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

         return client;
      }

      private static async Task GetLabOrders(HttpClient client, string baseUrl)
      {
         // var response = await client.GetAsync($"{baseUrl}/LabGlu/customers.xml");
         var response = await client.GetAsync($"{baseUrl}/laborder");

         //var responseInfo1 = await response.Content.ReadAsStringAsync();

         response.EnsureSuccessStatusCode();

         var responseInfo = await response.Content.ReadAsStringAsync();
         Console.WriteLine(responseInfo);
      }


      public static class Environments
      {
         public static Environment Production = new Environment {
            BaseUrl = "https://api.veritas.healthcare",
            Scope = "https://veritastestingb2c.onmicrosoft.com/1d22859f-da06-4709-bb33-0bab942d8443/.default"
         };
         public static Environment Test = new Environment {
            BaseUrl = "https://test.api.veritas.healthcare",
            Scope = "https://veritasvaxtestb2c.onmicrosoft.com/d7cf4f44-ce43-4fd9-9da2-ba88ebb6e6b2/.default"
         };
      }
      public class Environment
      {
         public string BaseUrl { get; set; }
         public string Scope { get; set; }
      }
   }

}
