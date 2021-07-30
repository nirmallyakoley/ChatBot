using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GTAADevChatBot.Dialogs.AirportInformation.ServiceAmenities
{
    public class ServiceAmenitiesHelper
    {
        public static List<ServiceAmenities> GetServiceAmenities(FoodRetailServiceBO foodStatus)
        {
            try
            {
                List<ServiceAmenities> serAmenities = null;
                List<ServiceAmenities> lstserAmenities = null;
                string[] paths = { ".", "Dialogs", "Resources", "ServiceAmenities.json" };
                string fullPath = Path.Combine(paths);
                serAmenities = JsonConvert.DeserializeObject<List<ServiceAmenities>>(File.ReadAllText(fullPath));
                if (foodStatus.DepartureArea.ToLower().Trim() == "pre-security")
                {
                    lstserAmenities = serAmenities.Where(x => x.Terminal == foodStatus.Terminal && x.Security.ToLower().Trim() == "pre").ToList();
                    return lstserAmenities;
                }
                else
                {
                    lstserAmenities = serAmenities.Where(x => x.Terminal == foodStatus.Terminal && x.Country.Contains(foodStatus.DepartureArea) && x.Security.ToLower() == "post").ToList();
                    return lstserAmenities;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

    }
}
