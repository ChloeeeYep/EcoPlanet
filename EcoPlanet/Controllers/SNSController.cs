using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Data;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Security.Cryptography;
using EcoPlanet.Models; 
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text;

namespace EcoPlanet.Controllers
{
    public class SNSController : Controller
    {
        private readonly EcoPlanetContext _context;
        private const string TopicARN = "arn:aws:sns:us-east-1:637423376901:EcoPlanet";

        public SNSController(EcoPlanetContext context)
        {
            _context = context;
        }

        //function 1:get the keys from Appsettings.json
        private List<string> getKeys()
        {
            List<string> keys = new List<string>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration conf = builder.Build();

            //1.3 add the retrieved to the list
            keys.Add(conf["Keys:Key1"]);
            keys.Add(conf["Keys:Key2"]);
            keys.Add(conf["Keys:Key3"]);

            return keys;
        }

        public IActionResult Index()
        {
            return View();
        }

        //function 2:send the subscribe newsletter page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> subscribeNewsletter(string email)
        {
            using (var client = new HttpClient())
            {
                var endpoint = "https://7ywb3w0t07.execute-api.us-east-1.amazonaws.com/dev/subscribe";

                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Email cannot be empty.";
                    return RedirectToAction("Index", "SNS");
                }
                var json = JsonConvert.SerializeObject(new { email = email });

                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync(); // This will give you the response body
                Console.WriteLine($"Status: {response.StatusCode}, Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    // Handle success
                    TempData["Message"] = "Subscription request sent.";
                }
                else
                {
                    // Handle failure
                    TempData["Error"] = "Failed to subscribe.";
                }

            }
            return RedirectToAction("Index", "SNS");
        }

        //function 3: create admin pae for broadcast the msg to subscribe customer
        public IActionResult sendBroadcastMessage()
        {
            return View();
        }

        //function 4: Send Broadcast Message Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> broadcast(string title, string msgbody)
        {
            List<string> keys = getKeys();
            AmazonSimpleNotificationServiceClient agent = new AmazonSimpleNotificationServiceClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            try
            {
                PublishRequest request = new PublishRequest
                {
                    TopicArn = TopicARN,
                    Subject = title,
                    Message = msgbody
                };

                PublishResponse response = await agent.PublishAsync(request);
                ViewBag.subscribeMessageID = response.MessageId;


                // Add title, msgbody, and current datetime to database
                SNS newSNS = new SNS
                {
                    Title = title,
                    Content = msgbody,
                };

                _context.Add(newSNS);
                await _context.SaveChangesAsync(); // Save changes to the database
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
            catch (Exception ex) // Catch any other exceptions, particularly from the database operations
            {
                return BadRequest("Database Error: " + ex.Message);
            }
            return View();
        }


        public async Task<IActionResult> ViewBroadcast()
        {
            List<SNS> sns = await _context.SNSTable.ToListAsync();
            return View(sns);
        }

        //Function to retrieved the users who had confirm subscribed
        private async Task<List<Subscription>> GetConfirmedSubscribedEmailsAsync()
        {
            List<string> keys = getKeys();
            using var client = new AmazonSimpleNotificationServiceClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);
            var request = new ListSubscriptionsByTopicRequest { TopicArn = TopicARN };
            ListSubscriptionsByTopicResponse response = await client.ListSubscriptionsByTopicAsync(request);

            // Filter only confirmed subscriptions
            return response.Subscriptions
                           .Where(s => s.SubscriptionArn.StartsWith("arn:aws:sns") && s.TopicArn == TopicARN)
                           .ToList();
        }


        //Display the lists of subscribers
        public async Task<IActionResult> ViewSubscribers()
        {
            var confirmedSubscriptions = await GetConfirmedSubscribedEmailsAsync();
            // Directly pass the confirmed subscriptions to the view
            return View(confirmedSubscriptions);
        }



        //Check If the Users has Subscribed to Our Newsletter 
        private async Task<bool> IsEmailSubscribedAsync(string email)
        {
            List<string> keys = getKeys();
            using var client = new AmazonSimpleNotificationServiceClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            string topicArn = TopicARN;

            string userEndpoint = email;

            var request = new ListSubscriptionsByTopicRequest
            {
                TopicArn = TopicARN
            };

            ListSubscriptionsByTopicResponse response = await client.ListSubscriptionsByTopicAsync(request);

            foreach (var subscription in response.Subscriptions)
            {
                if (subscription.Endpoint  == userEndpoint)
                {
                    return true; // Email is already subscribed
                }
            }
            return false; // Email is not subscribed
        }

    }
}
