using System;
using RestSharp;
using Newtonsoft.Json;

using ACE.Server.Entity;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;

using log4net;
using log4net.Config;

namespace ACE.Server.Riptide
{
    // Each experimental feature should have its own flag.
    // To conduct UAT on a feature, set the flag to true.
    // If feature fails UAT, set the flag back to false.
    // If feature passes UAT, the flag can be deprecated.
    //public class GlobalEventFlags
    //{
    //    public bool PK_DEATH_ANNOUNCEMENT = true;
    //}
    public static class GlobalEventManager
    {
        //public static GlobalEventFlags Enabled = new GlobalEventFlags();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void GlobalWorldBroadcast(string message)
        {
            foreach (var player in PlayerManager.GetAllOnline())
            {
                player.Session.WorldBroadcast(message);
            }
        }
        public static void OnPKDeath(Player killer, Player victim, DeathMessage deathMessage)
        {
            try
            {
                if (killer != null && victim != null)
                {
                    string msg = string.Format(deathMessage.Broadcast, victim.Name, killer.Name);
                    foreach (var player in PlayerManager.GetAllOnline())
                    {
                        if (player != killer && player != victim)
                        {
                            player.Session.WorldBroadcast(msg);
                        }
                    }
                    //GlobalWorldBroadcast(msg);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void AuthWebPortal()
        {
            var web_portal_url = PropertyManager.GetString("web_portal_url").Item;
            var web_portal_api_version = PropertyManager.GetString("web_portal_api_version").Item;
            var web_portal_api_username = PropertyManager.GetString("web_portal_api_username").Item;
            var web_portal_api_password = PropertyManager.GetString("web_portal_api_password").Item;
            var web_portal_api_jwt = PropertyManager.GetString("web_portal_api_jwt").Item;
            if (!string.IsNullOrEmpty(web_portal_url) || !string.IsNullOrEmpty(web_portal_api_version) || !string.IsNullOrEmpty(web_portal_api_username) || !string.IsNullOrEmpty(web_portal_api_password))
            {
                var client = new RestClient(web_portal_url);

                var request = new RestRequest("ace" + web_portal_api_version + "/accounts/login", Method.POST);
                request.AddHeader("Content-type", "application/json");
                
                //request.AddHandler("Content-type", "application/json");
                request.AddJsonBody(new
                {
                    username = web_portal_api_username,
                    password = web_portal_api_password,
                });

                // easy async support
                client.ExecuteAsync(request, response => {
                    //response.Content
                    if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        log.Info("ERROR: Unauthorized Riptide API Web Portal");
                    } else
                    {
                        var a = JsonConvert.DeserializeObject<JWT>(response.Content);
                        PropertyManager.ModifyString("web_portal_api_jwt", a.jwt);
                    }

                });
            } else  {
                log.Info("ERROR: Failed to initialize Riptide API Web Portal");
            }
        }

        public static void SendDeathDetailsViaHTTP(DamageHistoryInfo topDamager, DamageHistoryInfo killshot, WorldObject victim)
        {
            var web_portal_url = PropertyManager.GetString("web_portal_url").Item;
            var web_portal_api_version = PropertyManager.GetString("web_portal_api_version").Item;
            var web_portal_api_jwt = PropertyManager.GetString("web_portal_api_jwt").Item;
            if (!string.IsNullOrEmpty(web_portal_url) || !string.IsNullOrEmpty(web_portal_api_version) || !string.IsNullOrEmpty(web_portal_api_jwt))
            {
                var client = new RestClient(web_portal_url);
                // client.Authenticator = new HttpBasicAuthenticator(username, password);

                var request = new RestRequest("killshot" + web_portal_api_version + "/deaths", Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.AddHeader("Authorization", web_portal_api_jwt);
                request.AddJsonBody(new
                {
                    killer = topDamager.Guid.Full,
                    victim = victim.Guid.Full,
                    finisher = killshot.Guid.Full,
                });

                // easy async support
                client.ExecuteAsync(request, response => {
                    // Nothing here
                    var x = 1;

                });
            }
            else
            {
                log.Info("ERROR: Riptide API Web Portal not initialized");
            }
        }


    }

    public class JWT
    {
        [JsonProperty("jwt")]
        
        public String jwt { get; set; }
    }
}
