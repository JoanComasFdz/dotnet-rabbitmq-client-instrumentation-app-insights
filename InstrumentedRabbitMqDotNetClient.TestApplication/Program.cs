using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Host terminated unexpectedly: {ex.GetType()}: {ex.Message}");
            }
            finally
            {
                //Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}