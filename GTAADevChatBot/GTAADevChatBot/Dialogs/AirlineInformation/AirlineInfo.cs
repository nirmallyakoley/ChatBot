using GTAADevChatBot.CustomLogging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace GTAADevChatBot.Dialogs.AirlineInformation
{
    public class AirlineInfo
    {


        public static AirlineInformationBO GetAirlineInfo(string airlineNameOrCode)
        {
            try
            {
                string[] paths = { ".", "Dialogs", "Resources", "airline.json" };
                string fullPath = Path.Combine(paths);
                AirlineInformationBO result = null;
                IEnumerable<AirlineInformationBO> jsonResult = JsonConvert.DeserializeObject<IEnumerable<AirlineInformationBO>>(File.ReadAllText(fullPath));
                result = jsonResult.Where(x => x.Airline.Replace(" ", String.Empty).ToLower() == airlineNameOrCode || x.Code1.ToLower() == airlineNameOrCode || x.Code2.ToLower() == airlineNameOrCode || string.Concat(x.Code1.ToLower(), "/", x.Code2.ToLower()) == airlineNameOrCode || string.Concat(x.Code1.ToLower(), "\\", x.Code2.ToLower()) == airlineNameOrCode).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
