using Exam_Retake.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;



namespace Exam_Retake
{
    [TestFixture]
    public class Tests
    {
        private RestClient? client;
        private string revueID;
        private const string baseUrl = "https://d2925tksfvgq8c.cloudfront.net";


        [OneTimeSetUp]
        public void Setup()
        {
            string token = AccessToken("Yoto@example.com", "123456");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string AccessToken(string _email, string _passwd)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest($"{baseUrl}/api/User/Authentication", Method.Post);
            request.AddJsonBody(new
            {
                Email = _email,
                Password = _passwd
            });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateRevue_CreatedRevueShouldReturn_codeOK_Msg()
        {
            var newRevue = new RevueDTO
            {
                Title = "Test",
                Descriction = "Test",
                Url = ""
            };

            var request = new RestRequest($"{baseUrl}/api/Revue/Create", Method.Post);
            request.AddJsonBody(newRevue);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllRevue_GetAllRevueShouldReturn_codeOK_NonEmptyArr_StoreID()
        {
            var request = new RestRequest($"{baseUrl}/api/Revue/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetArrayLength(), Is.GreaterThan(0));
            
            var lastRevue = json[json.GetArrayLength() - 1];
            revueID = lastRevue.GetProperty("id").GetString() ?? string.Empty;
        }

        [Test, Order(3)]
        public void EditTheLastRevue_TheLastRevueShouldReturn_codeOK_Msg()
        {
            var updatedRevue = new RevueDTO
            {
                Title = "Updated Test",
                Url = "",
                Descriction = "Updated Test"
            };

            var request = new RestRequest($"{baseUrl}/api/Revue/Edit?revueId={revueID}", Method.Put);
            request.AddJsonBody(updatedRevue);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Edited successfully"));
        }

        [Test, Order(4)]
        public void DeleteTheEditedRevue_TheEditedRevueShouldReturn_codeOK_Msg()
        {
            var request = new RestRequest($"{baseUrl}/api/Revue/Delete?revueId={revueID}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("The revue is deleted!"));
        }

        [Test, Order(5)]
        public void CreateRevueWithoutRequiredFields_ShouldReturn_codeBadRequest()
        {
            var newRevue = new RevueDTO
            {
                Title = "",
                Url = "",
                Descriction = ""
                
            };
            var request = new RestRequest($"{baseUrl}/api/Revue/Create", Method.Post);
            request.AddJsonBody(newRevue);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Test, Order(6)]
        public void EditNonExistentRevue_ShouldReturn_BadRequest_Msg()
        {
            var updatedRevue = new RevueDTO
            {
                Title = "Updated Test",
                Url = "",
                Descriction = "Updated Test"
            };
            var request = new RestRequest($"{baseUrl}/api/Revue/Edit?revueId=nonexistentid", Method.Put);
            request.AddJsonBody(updatedRevue);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
            Assert.That(response.Content, Does.Contain("There is no such revue!"));
        }


        [OneTimeTearDown]
        public void TearDown() { client?.Dispose(); }
    }

}
