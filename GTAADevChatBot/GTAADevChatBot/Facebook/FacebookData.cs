using GTAADevChatBot.Dialogs.AirportInformation;
using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using System;
using System.Collections.Generic;

namespace GTAADevChatBot.Facebook
{
    public class FacebookData
    {
        public static FacebookChannelData ProvideFaceBookChannelData(List<IFoodRetailService> lstFoodRetailServices, FoodRetailServiceBO foodAndBeverageBO)
        {
            FacebookChannelData objFacebookChannelData = null;

            List<FaceBookElement> lstFaceBookElement = new List<FaceBookElement>();
            try
            {
                foreach (IFoodRetailService objfoodRetailService in lstFoodRetailServices)
                {
                    string strLocation = FoodAndBeverageHelper.GetRestaurantLocation(objfoodRetailService, foodAndBeverageBO);
                    FaceBookButton objFaceBookButton = new FaceBookButton
                    { title = "More", type = "web_url", url = objfoodRetailService.URL };
                    FaceBookElement objFaceBookElement = new FaceBookElement
                    {
                        buttons = new List<FaceBookButton>() { objFaceBookButton },
                        image_url = Uri.EscapeUriString(objfoodRetailService.Image),
                        subtitle = strLocation,
                        title = objfoodRetailService.Name
                    };

                    lstFaceBookElement.Add(objFaceBookElement);
                }

                FaceBookPayload objFaceBookPayload = new FaceBookPayload()
                {
                    template_type = "generic",
                    elements = lstFaceBookElement
                };

                FacebookAttachment objFacebookAttachment = new FacebookAttachment
                {
                    type = "template",
                    payload = objFaceBookPayload
                };

                objFacebookChannelData = new FacebookChannelData()
                {
                    attachment = objFacebookAttachment,
                    notification_type = "NO_PUSH"
                };
            }
            catch
            {
                throw;
            }
            return objFacebookChannelData;
        }

    }
}
