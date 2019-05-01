using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Gateway.GraphQL.Services
{
    public class NotificationService
    {

        public NotificationService()
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault()
            });
        }

        public static async void SendNewBroadcastNotification(double[] categories)
        {
            
            // Define a condition which will send to devices which are subscribed
            // to either the Google stock or the tech industry topics.
            var condition = CreateCondition(categories);
            
            // See documentation on defining a message payload.
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = "New stream",
                    Body = $"A new stream you might be interested in has begun.",
                },
                Condition = condition,
            };

            // Send a message to devices subscribed to the combination of topics
            // specified by the provided condition.
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            // Response is a message ID string.
            Console.WriteLine("Successfully sent message: " + response);
        }

        private static string CreateCondition(double[] topics)
        {
            var condition = new string("");
            for (int i = 0; i < topics.Length; i++)
            {
                if (i==0)
                {
                    condition += " || ";
                }
                condition += $"'Category:{i}' in topics";
            }
            
            return condition;
        }
    }
}