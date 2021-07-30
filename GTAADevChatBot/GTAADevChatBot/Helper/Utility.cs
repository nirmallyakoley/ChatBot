using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GTAADevChatBot.Helper
{
    public class Utility
    {

        static public string TranFormDateDDMMYY(string strDate)
        {
            if (!String.IsNullOrEmpty(strDate))
            {
                string[] _date = strDate.Split(new char[] { '-' });
                DateTime dt = new DateTime(int.Parse(_date[0]), int.Parse(_date[1]), int.Parse(_date[2]));
                return dt.ToString("dd/MM/yy");
            }
            return string.Empty;
        }



        static public string ShowFlightStatus(string status)
        {
            string _status = "";
            switch (status)
            {
                case "ONT":
                    _status = GtaaChatConstant.ONT;
                    break;
                case "DEL":
                    _status = GtaaChatConstant.DEL;
                    break;
                case "ARR":
                    _status = GtaaChatConstant.ARR;
                    break;
                case "ERL":
                    _status = GtaaChatConstant.ERL;
                    break;
                case "CAN":
                    _status = GtaaChatConstant.CAN;
                    break;
                case "LND":
                    _status = GtaaChatConstant.LND;
                    break;
                case "DEP":
                    _status = GtaaChatConstant.DEP;
                    break;
                case "DIV":
                    _status = GtaaChatConstant.DIV;
                    break;
            }

            return _status;
        }

        public static Dictionary<string, string> type = new Dictionary<string, string>()
        {
            { "ARR", "arrive" },
            { "DEP", "depart" }
        };


        public static string GetTypeDefination(string typecode)
        {
            string defination = string.Empty;
            defination = (type.Keys.Contains(typecode.ToUpper())) ? type[typecode.ToUpper()] : string.Empty;
            return defination;
        }
        public static DateTime GetESTCurrentTime()
        {
            var timeUtc = DateTime.UtcNow;
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var esttimenow = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            return esttimenow;
        }

        public static DateTime GetESTCurrentTime(DateTime dateTime)
        {
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");          
            var esttimenow = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), easternZone);
            return esttimenow;
        }

        public static string GetFlightType(string flightType)
        {
            string type = string.Empty;
            if (flightType == "ARR")
                type = "arriving";
            else
                type = "departing";
            return type;

        }

        public static string ConvertToTime(string strTime)
        {
            try
            {
                if (Regex.IsMatch(strTime, "\\d+\\s+\\d+"))
                {
                    strTime = strTime.Replace(" ", "-");
                }
                int charLocation, time, colonLocation;
                string strTimeRange, stopAt, strValidTime, strMyTime, result1, result, strMinutes = string.Empty;
                if (!String.IsNullOrWhiteSpace(strTime))
                {
                    if (!(strTime.Contains("-") || strTime.ToLower().Contains("to")))
                    {
                        strTime = strTime + "-" + strTime;
                    }
                    if (strTime.Contains("-"))
                        stopAt = "-";
                    else if (strTime.Contains("to"))
                        stopAt = "to";
                    else if (strTime.Contains("To"))
                        stopAt = "To";
                    else
                        return strTime;


                    charLocation = strTime.IndexOf(stopAt, StringComparison.Ordinal);

                    if (charLocation >= 2)
                    {
                        string strAmorPM = strTime.Substring(0, charLocation);
                        if (strAmorPM.ToLower().Contains("am") || strAmorPM.ToLower().Contains("pm"))
                        {
                            return strTime;
                        }
                        else
                        {
                            if (strAmorPM.Contains(":"))
                            {
                                colonLocation = strTime.IndexOf(":", StringComparison.Ordinal);
                                strTimeRange = strTime.Substring(0, colonLocation);
                                strMinutes = strTime.Substring(colonLocation+1,charLocation-colonLocation-1);
                                time = Convert.ToInt32(strTimeRange.Trim()?.Replace(" ", string.Empty));
                            }
                            else
                            {
                                time = Convert.ToInt32(strAmorPM.Trim()?.Replace(" ", string.Empty));
                                strMinutes = strAmorPM.Substring(2);
                            }


                            //string after '-' or 'to'

                            strTimeRange = strTime.Substring(charLocation);
                            if (strTimeRange.ToLower().Contains("am") || strTimeRange.ToLower().Contains("pm"))
                            {
                                strMyTime = strTimeRange;
                            } else
                            {
                                strTimeRange = strTimeRange.Replace(":", string.Empty);
                                if (strTimeRange.Substring(strTimeRange.Length - 2) == "00")
                                {
                                    strMyTime = strTimeRange.Substring(0, strTimeRange.Length - 2);
                                   
                                }
                                else
                                    strMyTime = strTimeRange.Insert(strTimeRange.Length - 2, ":");

                                    //strMyTime = strTimeRange.Substring(0, strTimeRange.Length - 2) + ":" + strTimeRange.Substring(strTimeRange.Length - 2);
                            }

                            //end of string after '-' or 'to'--------------------------------------//

                            if (Enumerable.Range(01, 2300).Contains(time))
                            {
                                result = Convert.ToString(time);
                                result1 = Enumerable.Range(1, 23).Contains(Convert.ToInt32(result)) ? result : result.Substring(0, result.Length - 2);
                                if (Enumerable.Range(1, 11).Contains(Convert.ToInt32(result1)))
                                {
                                    strValidTime = result1 +":"+ strMinutes + " am " + strMyTime;
                                    return strValidTime;
                                }
                                else
                                {
                                    strValidTime = result1 + ":" + strMinutes + " pm " + strMyTime;
                                    return strValidTime;
                                }

                            }
                        }
                    }
                    return strTime;
                }
                return strTime;
            }
            catch (Exception ex)
            {
                return strTime;
            }

        }

        public static string ModifyCityFlightNoByRegex(string flightNumberOrCity)
        {
            string flightNumberPattern = @"^[A-Za-z]{2}[0-9]+$";
            string cityPattern = @"^[A-Za-z]+$";
            Match forCity = Regex.Match(flightNumberOrCity, cityPattern);
            Match forflight = Regex.Match(flightNumberOrCity, flightNumberPattern);
            if (forCity.Success || forflight.Success)
            {
                if (forCity.Success)
                {
                    string city = char.ToUpper(flightNumberOrCity.First()) + flightNumberOrCity.Substring(1).ToLower();
                    return city;
                }
                if (forflight.Success)
                {
                    bool IsCapital = true;
                    var result = new StringBuilder(flightNumberOrCity.Length);

                    for (int i = 0; i < flightNumberOrCity.Length; i++)
                    {
                        if (IsCapital)
                        {
                            result.Append(char.ToUpper(flightNumberOrCity[i]));
                            if (i == 1)
                                IsCapital = false;
                        }
                        else
                            result.Append(flightNumberOrCity[i]);
                    }
                    return result.ToString();
                }
            }
            else
            {
                string noMatch = char.ToUpper(flightNumberOrCity.First()) + flightNumberOrCity.Substring(1).ToLower();
                return noMatch;
            }
            return flightNumberOrCity;
        }

        public async Task<string> FetchVaultConnectionString()
        {
            try
            {
                string blobConnectionString = string.Empty;
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync("https://gtaachatbotdevkeyvault.vault.azure.net/secrets/BlobConnectionString/4551d32a0c464e6ab814dd15da9c4b50").ConfigureAwait(false);
                blobConnectionString = secret.Value;
                return blobConnectionString;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
