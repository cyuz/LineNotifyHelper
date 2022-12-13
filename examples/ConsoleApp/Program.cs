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
        }
    }
}
