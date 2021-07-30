using System.Collections.Generic;

namespace GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages
{
    public class FoodRetailServiceBO
    {
        public string Terminal { get; set; }
        public string DepartureArea { get; set; }
        public string RestaurantType { get; set; }
        public int Skip { get; set; } = 0;
        public List<IFoodRetailService> FoodRetailServiceCollection { get; set; }
    }
}
