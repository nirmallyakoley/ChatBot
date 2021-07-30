using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GTAADevChatBot.Dialogs.AirportInformation.RetailAndDutyFree
{
    public class RetailHelper
    {
        public static List<Retail> GetRetail(FoodRetailServiceBO foodStatus)
        {
            try
            {
                List<Retail> retails = null;
                List<Retail> lstRetail = null;
                string[] paths = { ".", "Dialogs", "Resources", "RetailAndDutyFree.json" };
                string fullPath = Path.Combine(paths);
                retails = JsonConvert.DeserializeObject<List<Retail>>(File.ReadAllText(fullPath));
                if (foodStatus.DepartureArea.ToLower().Trim() == "pre-security")
                {
                    lstRetail = retails.Where(x => x.Terminal == foodStatus.Terminal && x.Security.ToLower().Trim() == "pre").ToList();
                    return lstRetail;
                }
                else
                {
                    lstRetail = retails.Where(x => x.Terminal == foodStatus.Terminal && x.Country.Contains(foodStatus.DepartureArea) && x.Security.ToLower() == "post").ToList();
                    return lstRetail;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
