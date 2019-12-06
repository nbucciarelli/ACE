//using System;
//using ACE.Server.Riptide.Managers;
//using Newtonsoft.Json;

//namespace ACE.Server.Riptide.API.Models
//{
//    public class WeenieView
//    {

//        // always require a string value
//        [JsonProperty("name", Required = Required.Always)]
//        public string Name { get; set; }

//        // don't require any value
//        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
//        public string Role { get; set; }

//        // property is ignored
//        [JsonIgnore]
//        public string Password { get; set; }
//    }

//    public class InventoryItem
//    {

//    }
//    public class PlayerInventoryModel
//    {
//        public PlayerInventoryModel(IPlayerInventory src)
//        {
            
//        }
//    }
//}
