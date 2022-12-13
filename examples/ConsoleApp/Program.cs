using LineNotifyHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string access_token = "YOUR_ACCESS_TOKEN";

            IOptions<LineNotifyOptions> optionParameter = Options.Create(new LineNotifyOptions());

            IHttpClientFactory clinetFatory = new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetService<IHttpClientFactory>();

            LineNotifySender sender = new LineNotifySender(optionParameter, clinetFatory);

            var result = sender.NotifyMessageAsync(access_token, "Hello World!!").Result;

            Console.WriteLine("Response:" + JsonSerializer.Serialize(result));

            if (!result.Success)
            {
                /// Http Status Code
                Console.WriteLine(result.StatusCode);
                /// raw message status
                Console.WriteLine(result.ErrorBody.status);
                /// raw message conent
                Console.WriteLine(result.ErrorBody.message);
            }

            /// cannot send sticker without message
            result = sender.NotifyMessageAsync(access_token, "*", "446", "1988").Result;
            Console.WriteLine("Response:" + JsonSerializer.Serialize(result));

            if(!result.Success)
            {
                Console.WriteLine(result.StatusCode);
                Console.WriteLine(result.ErrorBody.status);
                Console.WriteLine(result.StatusCode);
            }
        }
    }
}
