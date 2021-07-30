using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using GTAADevChatBot.Dialogs.FlightStatus.ByCity;
using System.IO;
using GTAADevChatBot.Helper;
using Newtonsoft.Json;

namespace GTAADevChatBot.Dialogs.FlightStatus
{
    public class FlightCardUtility
    {
        public static Attachment CreateSearchFlightAdaptiveCardAttachment(FlightByCity rootObject, FlightStatusState objFlightStatusState)
        {
            string adaptiveCard = string.Empty;
            try
            {
                string[] paths = { ".", "Dialogs", "Resources", "flightStatus.json" };
                string strCity = string.Empty, strCountry = string.Empty, strCityCode = String.Empty;
                string fullPath = Path.Combine(paths);
                Route objRoute = rootObject.routes.Where(route => route.city.ToLower().Contains(objFlightStatusState.City.ToLower())).FirstOrDefault();
                if (null != objRoute)
                {
                    strCity = objRoute.city;
                    strCityCode = objRoute.code;
                    strCountry = objRoute.cnty == "CAN" ? objRoute.region : objRoute.cnty;
                }


                adaptiveCard = File.ReadAllText(fullPath);
                adaptiveCard = adaptiveCard.Replace("<status>", Utility.ShowFlightStatus(rootObject.status));
                adaptiveCard = adaptiveCard.Replace("<flightnumber>", rootObject.id2);
                adaptiveCard = adaptiveCard.Replace("<schTime>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("HH:mm"));
                adaptiveCard = adaptiveCard.Replace("<latestTm>", Utility.GetESTCurrentTime(rootObject.latestTm).ToString("HH:mm"));
                adaptiveCard = adaptiveCard.Replace("<date>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("MMMM dd, HH:mm"));

                if (rootObject.type == "ARR")
                {
                    //adaptiveCard = adaptiveCard.Replace("<departcitycode>", rootObject.routes.FirstOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", strCityCode);
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", "YYZ");
                    //adaptiveCard = adaptiveCard.Replace("<departcity>", rootObject.routes.FirstOrDefault().cnty == "CAN" ? rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().region : rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<departcity>", $"{strCity}, {strCountry}");
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", "Toronto, ON");
                    adaptiveCard = adaptiveCard.Replace("<gate>", string.Empty);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", string.Empty);
                }

                if (rootObject.type == "DEP")
                {
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", "YYZ");
                    //adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", rootObject.routes.LastOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", strCityCode);
                    adaptiveCard = adaptiveCard.Replace("<departcity>", "Toronto, ON");
                    //adaptiveCard = adaptiveCard.Replace("<arrivecity>", rootObject.routes.LastOrDefault().cnty == "CAN" ? rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().region : rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", $"{strCity}, {strCountry}");
                    adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", "Gate");
                }

                adaptiveCard = adaptiveCard.Replace("<Airline>", rootObject.al + " (" + rootObject.alCode + ")");
                adaptiveCard = adaptiveCard.Replace("<term>", rootObject.term);
                //adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
                //
                // adaptiveCard = adaptiveCard.Replace("<type>", rootObject.type);

                if (rootObject.ids != null && rootObject.ids.Count() > 0)
                {
                    string codeshares = "This flight is also known as :\r\r ";
                    rootObject.ids.ToList().ForEach(s => codeshares += string.Format(" {0} : {1} \r\r", s.id2, s.alName));
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", codeshares);
                    // adaptiveCard = adaptiveCard.Replace("<codesharecount>", rootObject.ids.ToList().Count().ToString());
                }
                else
                {
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", string.Empty);
                }
            }
            catch (Exception ex)
            {

                adaptiveCard = string.Empty;
            }
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        public static Attachment CreateFacebookSearchFlightAdaptiveCardAttachment(FlightByCity rootObject, FlightStatusState objFlightStatusState)
        {
            string adaptiveCard = string.Empty;
            try
            {
                string[] paths = { ".", "Dialogs", "Resources", "Facebook_FlightStatus_Card.json" };

                string strCity = string.Empty, strCountry = string.Empty, strCityCode = String.Empty;
                string fullPath = Path.Combine(paths);
                Route objRoute = rootObject.routes.Where(route => route.city.ToLower().Contains(objFlightStatusState.City.ToLower())).FirstOrDefault();
                if (null != objRoute)
                {
                    strCity = objRoute.city;
                    strCityCode = objRoute.code;
                    strCountry = objRoute.cnty == "CAN" ? objRoute.region : objRoute.cnty;
                }


                adaptiveCard = File.ReadAllText(fullPath);
                adaptiveCard = adaptiveCard.Replace("<status>", Utility.ShowFlightStatus(rootObject.status));
                adaptiveCard = adaptiveCard.Replace("<flightnumber>", rootObject.id2);
                adaptiveCard = adaptiveCard.Replace("<schTime>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("HH:mm"));
                adaptiveCard = adaptiveCard.Replace("<latestTm>", Utility.GetESTCurrentTime(rootObject.latestTm).ToString("HH:mm"));
                adaptiveCard = adaptiveCard.Replace("<date>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("MMMM dd, HH:mm"));

                if (rootObject.type == "ARR")
                {
                    //adaptiveCard = adaptiveCard.Replace("<departcitycode>", rootObject.routes.FirstOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", strCityCode);
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", "YYZ");
                    //adaptiveCard = adaptiveCard.Replace("<departcity>", rootObject.routes.FirstOrDefault().cnty == "CAN" ? rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().region : rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<departcity>", $"{strCity}, {strCountry}");
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", "Toronto, ON");
                    adaptiveCard = adaptiveCard.Replace("<gate>", string.Empty);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", string.Empty);
                }

                if (rootObject.type == "DEP")
                {
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", "YYZ");
                    //adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", rootObject.routes.LastOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", strCityCode);
                    adaptiveCard = adaptiveCard.Replace("<departcity>", "Toronto, ON");
                    //adaptiveCard = adaptiveCard.Replace("<arrivecity>", rootObject.routes.LastOrDefault().cnty == "CAN" ? rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().region : rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", $"{strCity}, {strCountry}");
                    adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", "Gate");
                }

                adaptiveCard = adaptiveCard.Replace("<Airline>", rootObject.al + " (" + rootObject.alCode + ")");
                adaptiveCard = adaptiveCard.Replace("<term>", rootObject.term);
                //adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
                //
                // adaptiveCard = adaptiveCard.Replace("<type>", rootObject.type);

                if (rootObject.ids != null && rootObject.ids.Count() > 0)
                {
                    string codeshares = "This flight is also known as :\r\r ";
                    rootObject.ids.ToList().ForEach(s => codeshares += string.Format(" {0} : {1} \r\r", s.id2, s.alName));
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", codeshares);
                    // adaptiveCard = adaptiveCard.Replace("<codesharecount>", rootObject.ids.ToList().Count().ToString());
                }
                else
                {
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", string.Empty);
                }
            }
          
                catch (Exception ex)
            {

                adaptiveCard = string.Empty;
            }
       
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

    }

    
}

