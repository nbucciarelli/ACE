using System;
using System.IO;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
using Nancy.Hosting.Self;
using Nancy.Owin;


//https://www.hanselman.com/blog/ExploringAMinimalWebAPIWithNETCoreAndNancyFX.aspx
namespace ACE.Server.Riptide.API
{
    // MODULES MUST BE PUBLIC
    public class SampleModule : Nancy.NancyModule
    {
        public SampleModule()
        {
            //Get["/"] = _ => "Hello World!";
            Get("", _ => "Hello World!");
            Get("/", (_ => "Hello World!/"));
            Get("/hello", (_ => "Hello World!/hello"));
        }
    }

    public class RiptideAPI
    {
        // https://volkanpaksoy.com/archive/2015/11/11/building-a-simple-http-server-with-nancy/
        private static RiptideAPI p;
        private string _url = "http://localhost";
        private int _port = 12345;
        private Uri uri { get { return new Uri($"{_url}:{_port}/"); } }
        private NancyHost nancyHost;

        public static void Initialize()
        {
            p = new RiptideAPI();
            p.Start();
        }

        public static void Destroy()
        {
            p.Stop();
        }

        public RiptideAPI()
        {
            //var uri = new Uri($"{_url}:{_port}/");
            var config = new HostConfiguration
            {
                UrlReservations = new UrlReservations { CreateAutomatically = true }
            };
            nancyHost = new NancyHost(config, uri);
        }

        public void Start()
        {
            nancyHost.Start();
            Console.WriteLine(String.Format("Listening on {0}. Press any key to stop.", uri.AbsoluteUri));
            Console.ReadKey();
            Stop();
        }

        private void Stop()
        {
            nancyHost.Stop();
        }
    }

}

