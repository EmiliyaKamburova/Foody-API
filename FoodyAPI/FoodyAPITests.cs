using RestSharp;
using System.Text.Json;
using System.Net;
using RestSharp.Authenticators;
using FoodyAPI.Models;

namespace FoodyAPI
{
    [TestFixture]
    public class FoodyAPITests
    {
        private RestClient client;
        private static string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private static string lastCreatedFoodId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("emi12", "12348765");
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var jsonItems = JsonSerializer.Deserialize<JsonElement>(response.Content);

            var accessToken = jsonItems.GetProperty("accessToken").GetString();

            return accessToken;
        }

        [Test, Order(1)]
        public void Test_AddNewFood_WithRequiedFields_ShouldReturnCreated()
        {
            var requestBody = new 
            {
                Name = "Test2",
                Description = "Test2",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(requestBody);

            var response = client.Execute(request);

            //Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            //Assert.That(json.TryGetProperty("foodId", out _), Is.True, "Response JSON should contain 'foodId' property.");
            lastCreatedFoodId = json.GetProperty("foodId").GetString();
        }

        [Test, Order(2)]
        public void Test_EditFoodTitle_ShouldReturnSuscces()
        {
            var editedBody = new[]
            {
              new {
                  path =  "/name",
                  op = "replace",
                  value = "Update Food Name"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{lastCreatedFoodId}", Method.Patch);
            request.AddJsonBody(editedBody);  

            var response = client.Execute(request);

            //Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<APIResponseDTO>(response.Content);
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void Test_GetAllFoods_ShouldReturnSuscces()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
           

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseItems = JsonSerializer.Deserialize<List<APIResponseDTO>>(response.Content);
            Assert.That(responseItems, Is.Not.Empty);            
        }

        [Test, Order(4)]
        public void Test_DeleteEditeFood_ShouldReturnSuscces()
        { 
            var request = new RestRequest($"/api/Food/Delete/{lastCreatedFoodId}", Method.Delete);         

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            Assert.That(response.Content,Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void Test_AddNewFood_WithoutRequiedFields_ShouldReturnBadRequest()
        {
            var requestBody = new
            {             
                Description = "Test2",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(requestBody);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));     
        }

        [Test, Order(6)]
        public void Test_EditNonExistingFood_ShouldReturnNotFound()
        {
            var fakeFoodId = "123";
            var editedBody = new[]
             {
              new {
                  path =  "/name",
                  op = "replace",
                  value = "Update Food Name"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{fakeFoodId}", Method.Patch);
            request.AddJsonBody(editedBody);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."));
        }

        [Test, Order(7)]
        public void Test_DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            var fakeFoodId = "123";
            var request = new RestRequest($"/api/Food/Delete/{fakeFoodId}", Method.Delete);          

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));    
        }



        [OneTimeTearDown]
        public void Clear()
        {
            client?.Dispose();
        }
    }
}