using System;
using System.Collections.Generic;
using ACE.Database.Models.Shard;
using ACE.Server.Riptide.Managers;
using ACE.Server.WorldObjects;
using Nancy;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//https://www.hanselman.com/blog/ExploringAMinimalWebAPIWithNETCoreAndNancyFX.aspx
namespace ACE.Server.Riptide.API
{
    // MODULES MUST BE PUBLIC. Otherwise you'll get 404 on every route. :)
    public class SampleModule : Nancy.NancyModule
    {
        public SampleModule()
        {
            //Get["/"] = _ => "Hello World!";
            Get("", _ => "Hello World!");
            Get("/", (_ => "Hello World!/"));
            Get("/hello", (_ => "Hello World!/hello"));
            Get("/players", _ =>
            {
                List<Player> players = RiptideManager.Players.GetAllOnline();
                JArray arr = new JArray();
                foreach(var p in players)
                {
                    var jp = new JObject();
                    jp.Add("id", p.Guid.Full);
                    jp.Add("ip", p.Session.EndPoint.ToString());
                    jp.Add("playerName", p.Name);
                    jp.Add("characterName", p.Character.Name);
                    arr.Add(jp);
                }
                return Json(arr.ToString());
            });
            Get("/inventory/{character_id}", _ =>
            {
                uint character_id = uint.Parse(_.character_id);
                var character = RiptideManager.Database.GetCharacter(character_id);
                var inventory = RiptideManager.Inventory.GetInventory(character);
                //return Json(inventory);
                return Text(inventory.Print());
            });
            Put("/characters/{character_id}/inventory/{item_id}", _ => {
                Character recipient = RiptideManager.Database.GetCharacter(uint.Parse(_.character_id));
                WorldObject item = RiptideManager.Database.GetWorldObject(uint.Parse(_.item_id));
                Character sender = null;
                if (item.OwnerId.HasValue)
                    sender = RiptideManager.Database.GetCharacter(item.OwnerId.Value);
                try
                {
                    RiptideManager.Inventory.TradeItem(sender, recipient, item, -1);
                    return Text("", 201);
                } catch
                {
                    return Text("Failed to Trade", 500);
                }
            });
        }

        public Response Json(string payload, int status = 200)
        {
            var response = (Response)payload;
            response.ContentType = "application/json";
            //response.StatusCode = status;
            return response;
        }

        public Response Json<T>(T payload, int status=200) {
            var myJsonString = JsonConvert.SerializeObject(payload);
            var response = (Response)myJsonString;
            response.ContentType = "application/json";
            //response.StatusCode = status;
            return response;
        }

        public Response Text(string payload, int status = 200)
        {
            var response = (Response)payload;
            response.ContentType = "application/text";
            //response.StatusCode = status;
            return response;
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

