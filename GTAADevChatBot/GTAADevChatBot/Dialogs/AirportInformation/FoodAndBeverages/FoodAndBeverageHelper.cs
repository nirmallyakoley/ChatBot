using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages
{
    public class FoodAndBeverageHelper
    {
        public static List<Restaurant> GetRestaurant(FoodRetailServiceBO foodStatus)
        {
            try
            {
                List<Restaurant> restaurants = null;
                List<Restaurant> lstRestaurant = null;
                string fullPath = string.Empty;
                string[] paths = { ".", "Dialogs", "Resources", "FoodAndBeverage.json" };
                fullPath = Path.Combine(paths);

                restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(File.ReadAllText(fullPath));
                if (foodStatus.DepartureArea.ToLower().Trim() == "pre-security")
                {
                    lstRestaurant = restaurants.Where(x => x.Terminal == foodStatus.Terminal && x.Security.ToLower().Trim() == "pre").ToList();
                    return lstRestaurant;
                }
                else
                {
                    lstRestaurant = restaurants.Where(x => x.Terminal == foodStatus.Terminal && x.Country.Contains(foodStatus.DepartureArea) && x.Security.ToLower() == "post").ToList();
                    return lstRestaurant;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static string GetRestaurantLocation(IFoodRetailService restaurant, FoodRetailServiceBO objFoodRetailServiceBO)
        {
            var location = restaurant.Location;
            var country = objFoodRetailServiceBO?.DepartureArea;

            var locationText = string.Empty;
            if (restaurant.Security == "Post")
            {
                locationText = "After security" + " (" + country + ") - " + "Near gate " + location;

            }
            else
            {
                locationText = "Before security";
            }
            return locationText;

        }

    }
}
