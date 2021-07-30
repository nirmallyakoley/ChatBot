using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Dialogs.Accessibility;
using GTAADevChatBot.Dialogs.AirlineInformation;
using GTAADevChatBot.Dialogs.AirportInformation;
using GTAADevChatBot.Dialogs.AirportInformation.CarRentals;
using GTAADevChatBot.Dialogs.AirportInformation.ConnectionsGuide;
using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using GTAADevChatBot.Dialogs.AirportInformation.Parking;
using GTAADevChatBot.Dialogs.AirportInformation.ParkingAndTransportation;
using GTAADevChatBot.Dialogs.AirportInformation.PublicTransit;
using GTAADevChatBot.Dialogs.AirportInformation.RetailAndDutyFree;
using GTAADevChatBot.Dialogs.AirportInformation.SecurityScreening;
using GTAADevChatBot.Dialogs.AirportInformation.ServiceAmenities;
using GTAADevChatBot.Dialogs.AirportInformation.TaxiAndLimosOrRideShare;
using GTAADevChatBot.Dialogs.AirportInformation.UPExpress;
using GTAADevChatBot.Dialogs.ByCity;
using GTAADevChatBot.Dialogs.CustomsAndImmigrations;
using GTAADevChatBot.Dialogs.CustomsAndImmigrations.ForCanada;
using GTAADevChatBot.Dialogs.CustomsAndImmigrations.ForUSA;
using GTAADevChatBot.Dialogs.FaQDialog;
using GTAADevChatBot.Dialogs.FlightBranchDialog;
using GTAADevChatBot.Dialogs.FlightStatus;
using GTAADevChatBot.Dialogs.FlightStatus.ByFlightID;
using GTAADevChatBot.Dialogs.LostAndFound;
using GTAADevChatBot.Dialogs.YesNoDialog;
using GTAADevChatBot.Helper;
using GTAADevChatBot.Social_Miner;
using GTAADevChatBot.Welcome;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot
{
    public class GTAADevChatBot : IBot
    {
        #region Constants
        // Supported LUIS Intents

        public const string GreetingIntent = "Greetings";
        public const string CancelIntent = "Cancel";
        public const string HelpIntent = "Help";
        public const string NoneIntent = "None";
        public const string SearchFlightIntent = "Search_for_Flights";
        public const string FlightStatusIntent = "Flight_Information";
        public const string FlightAndAirlineInfoIntent = "Flight_and_Airline_Info";
        public const string AirportInformationIntent = "Airport_Information";
        public const string ParkingIntent = "Parking";
        public const string CarRentalsIntent = "Car_Rentals";
        public const string TaxiLimosRideShareIntent = "TaxiLimosRideShare";
        public const string PublicTransitIntent = "Public_Transit";
        public const string UPExpressIntent = "UP_Express";
        public const string LostAndFoundIntent = "Lost_&_Found";
        public const string CustomsandImmigrationsIntent = "Customs_and_Immigrations";
        public const string FaqQuestionsIntent = "faqquestions";
        public const string UsCustomsIntent = "US_Customs";
        public const string CanadaCustomsIntent = "Canada_Customs";
        public const string AirlineInformationIntent = "Airline_Information";
        public const string BaggageIntent = "Baggage";
        public const string ReservationsIntent = "Reservations";
        public const string AirlineInfoCheckInIntent = "AirlineInfo_CheckIn";
        public const string LuisConfiguration = GtaaChatConstant.LUIS_CONFIGURATION;
        public const string QnAMakerKey = "gtaadevchatbotqnamaker-aseqg4mb6nonjwe";
        public const string accessibilityIntent = "Accessibility";
        public const string MagnuscardsIntent = "Magnus_cards";
        public const string mobileAssistanceIntent = "Mobility_Assistance";
        public const string withAnimalIntent = "Travelling_with_Animal";
        public const string WashroomsIntent = "Washrooms";
        public const string SecurityScreeningIntent = "Security_Screening";
        public const string ConnectionsGuideIntent = "Connections_Guide";
        public const string FoodAndBeverageIntent = "Food_and_Beverage";
        public const string ParkingAndTransportationIntent = "Parking_and_Transportation";
        public const string RetailAndDutyFreeIntent = "Retail_and_Duty_Free";
        public const string ServiceAmenitiesIntent = "Service_Amenities";
        #endregion

        #region Variables
        public string SocialMinerUrl = null;
        private DialogSet Dialogs { get; set; }
        public string ChatTranscriptContainer = null;
        #endregion







        #region ReadOnly

        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<FlightStatusState> _flightStatusStateAccessor;
        private readonly IStatePropertyAccessor<WelcomeUserState> _welcomeStateAccessor;
        private readonly IStatePropertyAccessor<AirlineInformationBO> _airlineInfoStateAccessor;
        private readonly IStatePropertyAccessor<FoodRetailServiceBO> _foodAndBeverageStateAccessor;
        private readonly IStatePropertyAccessor<CounterState> _socilaMinerStateAccessor;
        private readonly IStatePropertyAccessor<CSMState> _csmStateAccessor;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;
        private readonly ILogger _logger;
        IConfiguration _configuration;
        #endregion

      


        #region constructor
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        /// 
        public GTAADevChatBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            {
                _services = services ?? throw new ArgumentNullException(nameof(services));
                _userState = userState ?? throw new ArgumentNullException(nameof(userState));
                _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                SocialMinerUrl = _configuration["SocialMinerUrl"];
                ChatTranscriptContainer =_configuration["transcriptContainer"];
                if (loggerFactory == null)
                {
                    throw new System.ArgumentNullException(nameof(loggerFactory));
                }

                _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
                _logger.LogTrace("Turn start.");

                _socilaMinerStateAccessor = _userState.CreateProperty<CounterState>(nameof(CounterState));
                _flightStatusStateAccessor = _userState.CreateProperty<FlightStatusState>(nameof(FlightStatusState));
                _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
                _welcomeStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
                _airlineInfoStateAccessor = _userState.CreateProperty<AirlineInformationBO>(nameof(AirlineInformationBO));
                _foodAndBeverageStateAccessor = _userState.CreateProperty<FoodRetailServiceBO>(nameof(FoodRetailServiceBO));
                _csmStateAccessor = _userState.CreateProperty<CSMState>(nameof(CSMState));
                // Verify LUIS configuration.
                if (!_services.LuisServices.ContainsKey(LuisConfiguration))
                {
                    throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
                }

                if (!_services.QnAServices.ContainsKey(QnAMakerKey))
                {
                    throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a QnA service named '{QnAMakerKey}'.");
                }

                Dialogs = new DialogSet(_dialogStateAccessor);
                Dialogs.Add(new FlightStatusDialog(_flightStatusStateAccessor, _services, loggerFactory));
                Dialogs.Add(new FlightStatusByNumberDialog(_flightStatusStateAccessor, _services, loggerFactory));
                Dialogs.Add(new FlightStatusByCityDialog(_services));
                Dialogs.Add(new BranchFlightDialog(_flightStatusStateAccessor, _airlineInfoStateAccessor, _services, loggerFactory));

                Dialogs.Add(new CustomsAndImmigrationsDialog(loggerFactory));
                Dialogs.Add(new USACustomsDialog(loggerFactory));
                Dialogs.Add(new CanadaCustomsDialog(loggerFactory));
                Dialogs.Add(new AirlineInformationDialog(_airlineInfoStateAccessor, loggerFactory, _services));
                Dialogs.Add(new FaqQuestionsDialog(loggerFactory, _services));
                Dialogs.Add(new LostandFoundDialog(_services, loggerFactory));
                Dialogs.Add(new YesAndNoStatusDialog());

                //Airport information
                Dialogs.Add(new AirportInformationDialog(_services, _csmStateAccessor, loggerFactory));
                //Parking
                Dialogs.Add(new ParkingAndTransaportationDialog(loggerFactory));
                Dialogs.Add(new ParkingDialog(loggerFactory));
                Dialogs.Add(new CarRentalsDialog(loggerFactory));
                Dialogs.Add(new TaxiLimosRideShareDialog(loggerFactory));
                Dialogs.Add(new UPExpressDialog(loggerFactory));
                Dialogs.Add(new PublicTransitDialog(loggerFactory));
                //Food And Beverage
                Dialogs.Add(new FoodAndBeverageDialog(_foodAndBeverageStateAccessor, loggerFactory, _services));
                //Retail and Duty Free
                Dialogs.Add(new RetailAndDutyFreeDialog(_foodAndBeverageStateAccessor, loggerFactory, _services));
                //Service Amenities
                Dialogs.Add(new ServiceAmenitiesDialog(_foodAndBeverageStateAccessor, loggerFactory, _services));

                //Accessibility Flow	             
                Dialogs.Add(new AccessibilityDialog(_services, loggerFactory));
                Dialogs.Add(new MagnusCardsDialog(loggerFactory));
                Dialogs.Add(new WashRoomsDialog(loggerFactory));
                Dialogs.Add(new TravellingWithAnimalDialog(loggerFactory));
                Dialogs.Add(new MobilityAssistanceDialog(_services, loggerFactory));


                //Security Screening Flow
                Dialogs.Add(new SecurityScreeningDialog(loggerFactory));

                //Connections Flow
                Dialogs.Add(new ConnectionsGuideDialog(loggerFactory));

            }
        }
        #endregion



        #region Events
        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var activity = turnContext.Activity;
                // Create a dialog context
                var dc = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dc.ContinueDialogAsync(cancellationToken);
                var didBotWelcomeUser = await _welcomeStateAccessor.GetAsync(turnContext, () => new WelcomeUserState());
                var objCounterState = await _socilaMinerStateAccessor.GetAsync(turnContext, () => new CounterState());
                var objCSMState = await _csmStateAccessor.GetAsync(turnContext, () => new CSMState());
                var objFlightState = await _flightStatusStateAccessor.GetAsync(turnContext, () => new FlightStatusState());
                //if (activity.Type==ActivityTypes.ConversationUpdate)
                //{
                //    if (!didBotWelcomeUser.DidBotWelcomeUser)
                //    {
                //        TelemetryLog.LogCustomTrace(_services, null, "Chat Initiated");
                //        await WelcomeMessage.SendWelcomeMsgForStartAsync(turnContext, cancellationToken: cancellationToken);
                //        didBotWelcomeUser.DidBotWelcomeUser = true;
                //    }
                //}

                if (activity.Type == ActivityTypes.Message)
                {
                    var message = activity.AsMessageActivity();
                    if (message.ChannelId == "facebook")
                    {
                        var postBack = message.ChannelData != null ? message.ChannelData.postback?.payload.ToString() : "";
                        if (postBack == "GET_STARTED_PAYLOAD")
                        {
                            TelemetryLog.LogCustomTrace(_services, null, "Chat Initiated");
                            await WelcomeMessage.SendWelcomeMsgForStartAsync(turnContext, cancellationToken: cancellationToken);
                            didBotWelcomeUser.DidBotWelcomeUser = true;
                        }
                    }
                    if (!objCounterState.RequestedtoAgent)
                    {

                        if (!didBotWelcomeUser.DidBotWelcomeUser)
                        {
                            TelemetryLog.LogCustomTrace(_services, null, "Chat Initiated");
                            await WelcomeMessage.SendWelcomeMsgForStartAsync(turnContext, cancellationToken: cancellationToken);
                            didBotWelcomeUser.DidBotWelcomeUser = true;
                        }
                        else
                        {
                            string topIntent = string.Empty;
                            string CustomerFeedback = string.Empty;

                            switch (activity.Text.ToLower().Trim())
                            {
                                case "help":
                                case "?":
                                case var restart when new Regex(@"^[\s$&+,:;=?@#|'<>.-^*()%!]*restart[\s$&+,:;=?@#|'<>.-^*()%!]*$").IsMatch(activity.Text.ToLower().Trim()):
                                    objCSMState.CSQ = "General Inquiry";
                                    objCSMState.ID = "82";
                                    await AllDialogEnd.RestartConversationAsync(turnContext, cancellationToken: cancellationToken);
                                    break;

                                case "support":
                                    TelemetryLog.LogCustomEvents(_services, objCSMState);
                                    await new LiveChat().CallCustomerCareAsync(_socilaMinerStateAccessor, turnContext, _userState, _conversationState, objCSMState, _services, SocialMinerUrl, ChatTranscriptContainer);
                                    break;
                                case "exit":
                                case "cancel":
                                case "quit":
                                    objCSMState.CSQ = "General Inquiry";
                                    objCSMState.ID = "82";
                                    TelemetryLog.LogCustomTrace(_services, null, "Chat Terminated");
                                    await AllDialogEnd.EndConversationAsync(turnContext, cancellationToken);
                                    break;

                                case "👍":
                                case "yes-positive-feedback":
                                    CustomerFeedback = "Positive";
                                    TelemetryLog.LogCustomerFeedback(_services, CustomerFeedback);
                                    await turnContext.SendActivityAsync("Thanks for your feedback!");
                                    break;

                                case "👎":
                                case "no-negative-feedback":
                                    CustomerFeedback = "Negative";
                                    TelemetryLog.LogCustomerFeedback(_services, CustomerFeedback);
                                    await turnContext.SendActivityAsync("Thanks for your feedback!");
                                    break;
                                case "accessibility​":
                                    objCSMState.CSQ = "Accessibility";
                                    objCSMState.ID = "76";
                                    TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                    await dc.BeginDialogAsync(nameof(AccessibilityDialog));
                                    break;
                                case "flight & airline info..":
                                case "flight & airline info":
                                    objCSMState.CSQ = "Flight & Airline Information";
                                    objCSMState.ID = "73";
                                    TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                    await dc.BeginDialogAsync(nameof(BranchFlightDialog));
                                    break;

                                case "lost & found":
                                    objCSMState.CSQ = "Lost & Found";
                                    objCSMState.ID = "74";
                                    TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                    await dc.BeginDialogAsync(nameof(LostandFoundDialog));
                                    break;
                                case "airport information":
                                    objCSMState.CSQ = "Flight & Airline Information";
                                    objCSMState.ID = "73";
                                    TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                    await dc.BeginDialogAsync(nameof(AirportInformationDialog));
                                    break;
                                case "customs & immigration":
                                case "customs & immigration...":
                                    objCSMState.CSQ = "Customs & Immigration";
                                    objCSMState.ID = "75";
                                    TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                    await dc.BeginDialogAsync(nameof(CustomsAndImmigrationsDialog));
                                    break;


                            }
                            // if no one has responded,
                            if (!dc.Context.Responded)
                            {
                                QueryResult[] response;
                                // examine results from active dialog
                                switch (results.Status)

                                {
                                    case DialogTurnStatus.Empty:

                                        // Perform a call to LUIS to retrieve results for the current activity message.
                                        var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(turnContext, cancellationToken);

                                        if (luisResults.GetTopScoringIntent().score > 0.70)
                                        {
                                            var topScoringIntent = luisResults.GetTopScoringIntent().intent.ToString();                                           
                                            topIntent = topScoringIntent;
                                            if (topIntent == FlightStatusIntent || topIntent == FlightAndAirlineInfoIntent && luisResults.Entities.Count > 0)
                                            {                                                
                                                objFlightState.FlightNumber = luisResults.Entities["flight#"]?.FirstOrDefault().ToString();
                                                objFlightState.City = luisResults.Entities["city"]?.FirstOrDefault().ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                                                objFlightState.TT = luisResults.Entities["JourneyDay"]?.FirstOrDefault().ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                                                objFlightState.AD = luisResults.Entities["ArrOrDep"]?.FirstOrDefault().ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();         
                                            }
                                            else
                                            {
                                                if (topIntent == CustomsandImmigrationsIntent && luisResults.Entities.Count > 0)
                                                {
                                                    if (luisResults.Entities["Customs"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() == "US")
                                                        topIntent = UsCustomsIntent;
                                                    else if (luisResults.Entities["Customs"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() == "Canada")
                                                        topIntent = CanadaCustomsIntent;
                                                    else
                                                        topIntent = CustomsandImmigrationsIntent;
                                                }
                                                if (topIntent == FlightAndAirlineInfoIntent && luisResults.Entities.Count > 0)
                                                {
                                                    if (luisResults.Entities.ContainsKey("FlightInfo") && luisResults.Entities.ContainsKey("AirlineInfo"))
                                                        topIntent = FlightAndAirlineInfoIntent;
                                                    else
                                                    {

                                                        if (luisResults.Entities["FlightInfo"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim().ToLower() == turnContext.Activity.Text.ToLower())
                                                            topIntent = FlightStatusIntent;
                                                        else if (luisResults.Entities["AirlineInfo"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim().ToLower() == turnContext.Activity.Text.ToLower())
                                                            topIntent = AirlineInformationIntent;
                                                        else
                                                            topIntent = FlightAndAirlineInfoIntent;
                                                    }
                                                }
                                            }

                                        }
                                        switch (topIntent)
                                        {
                                            case GreetingIntent:
                                                await WelcomeMessage.SendWelcomeMsgForStartAsync(turnContext, cancellationToken: cancellationToken);
                                                break;

                                            case NoneIntent:
                                                await dc.BeginDialogAsync(nameof(FaqQuestionsDialog));
                                                break;

                                            case FlightAndAirlineInfoIntent:
                                                objCSMState.CSQ = "Flight & Airline Information";
                                                objCSMState.ID = "73";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                if(luisResults.Entities.Count > 0)
                                                {
                                                    await dc.BeginDialogAsync(nameof(FlightStatusDialog), objFlightState);
                                                }
                                                else
                                                    await dc.BeginDialogAsync(nameof(BranchFlightDialog));
                                                break;

                                            case FlightStatusIntent:
                                                objCSMState.CSQ = "Flight & Airline Information";
                                                objCSMState.ID = "73";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(FlightStatusDialog),objFlightState);
                                                break;

                                            case AirlineInformationIntent:
                                                objCSMState.CSQ = "Flight & Airline Information";
                                                objCSMState.ID = "73";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(AirlineInformationDialog));
                                                break;

                                            case BaggageIntent:
                                                objCSMState.CSQ = "Flight & Airline Information";
                                                objCSMState.ID = "73";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(AirlineInformationDialog));
                                                break;

                                            case ReservationsIntent:
                                                objCSMState.CSQ = "Flight & Airline Information";
                                                objCSMState.ID = "73";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(AirlineInformationDialog));
                                                break;

                                            case AirlineInfoCheckInIntent:
                                                objCSMState.CSQ = "Flight & Airline Information";
                                                objCSMState.ID = "73";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await turnContext.SendActivityAsync(" Most airlines offer online check in through their website", cancellationToken: cancellationToken);
                                                await dc.BeginDialogAsync(nameof(AirlineInformationDialog));
                                                break;


                                            case AirportInformationIntent:
                                                await dc.BeginDialogAsync(nameof(AirportInformationDialog));
                                                break;

                                            case ParkingAndTransportationIntent:
                                                objCSMState.CSQ = "Transportation";
                                                objCSMState.ID = "79";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(ParkingAndTransaportationDialog));
                                                break;

                                            case ParkingIntent:
                                                objCSMState.CSQ = "Transportation";
                                                objCSMState.ID = "79";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(ParkingDialog));
                                                break;

                                            case CarRentalsIntent:
                                                objCSMState.CSQ = "Transportation";
                                                objCSMState.ID = "79";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(CarRentalsDialog));
                                                break;

                                            case TaxiLimosRideShareIntent:
                                                objCSMState.CSQ = "Transportation";
                                                objCSMState.ID = "79";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(TaxiLimosRideShareDialog));
                                                break;

                                            case PublicTransitIntent:
                                                objCSMState.CSQ = "Transportation";
                                                objCSMState.ID = "79";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(PublicTransitDialog));
                                                break;

                                            case UPExpressIntent:
                                                objCSMState.CSQ = "Transportation";
                                                objCSMState.ID = "79";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(UPExpressDialog));
                                                break;

                                            case LostAndFoundIntent:
                                                objCSMState.CSQ = "Lost & Found";
                                                objCSMState.ID = "74";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(LostandFoundDialog));
                                                break;

                                            case FoodAndBeverageIntent:
                                                objCSMState.CSQ = "Food & Beverage";
                                                objCSMState.ID = "77";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(FoodAndBeverageDialog));
                                                break;

                                            case RetailAndDutyFreeIntent:
                                                objCSMState.CSQ = "Retail";
                                                objCSMState.ID = "78";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(RetailAndDutyFreeDialog));
                                                break;

                                            case ServiceAmenitiesIntent:
                                                objCSMState.CSQ = "Service Amenities";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(ServiceAmenitiesDialog));
                                                break;

                                            case CustomsandImmigrationsIntent:
                                                objCSMState.CSQ = "Customs & Immigration";
                                                objCSMState.ID = "75";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(CustomsAndImmigrationsDialog));
                                                break;

                                            case UsCustomsIntent:
                                                objCSMState.CSQ = "Customs & Immigration";
                                                objCSMState.ID = "75";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(USACustomsDialog));
                                                break;

                                            case CanadaCustomsIntent:
                                                objCSMState.CSQ = "Customs & Immigration";
                                                objCSMState.ID = "75";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(CanadaCustomsDialog));
                                                break;

                                            case FaqQuestionsIntent:
                                                await dc.BeginDialogAsync(nameof(FaqQuestionsDialog));
                                                break;

                                            // Accessibility flow		

                                            case accessibilityIntent:
                                                objCSMState.CSQ = "Accessibility";
                                                objCSMState.ID = "76";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(AccessibilityDialog));
                                                break;

                                            case MagnuscardsIntent:
                                                objCSMState.CSQ = "Accessibility";
                                                objCSMState.ID = "76";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(MagnusCardsDialog));
                                                break;

                                            case mobileAssistanceIntent:
                                                objCSMState.CSQ = "Accessibility";
                                                objCSMState.ID = "76";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(MobilityAssistanceDialog));
                                                break;

                                            case withAnimalIntent:
                                                objCSMState.CSQ = "Accessibility";
                                                objCSMState.ID = "76";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(TravellingWithAnimalDialog));
                                                break;

                                            case WashroomsIntent:
                                                objCSMState.CSQ = "Accessibility";
                                                objCSMState.ID = "76";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(WashRoomsDialog));
                                                break;


                                            //Security Screening Dialog
                                            case SecurityScreeningIntent:
                                                objCSMState.CSQ = "Security";
                                                objCSMState.ID = "80";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(SecurityScreeningDialog));
                                                break;

                                            case ConnectionsGuideIntent:
                                                objCSMState.CSQ = "Connection Guide";
                                                objCSMState.ID = "81";
                                                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                                                await dc.BeginDialogAsync(nameof(ConnectionsGuideDialog));
                                                break;

                                            default:
                                                await dc.BeginDialogAsync(nameof(FaqQuestionsDialog));
                                                break;
                                        }

                                        break;

                                    case DialogTurnStatus.Waiting:
                                        // The active dialog is waiting for a response from the user, so do nothing.
                                        break;

                                    case DialogTurnStatus.Complete:
                                        await dc.EndDialogAsync();
                                        break;

                                    default:
                                        await dc.CancelAllDialogsAsync();
                                        break;
                                }
                            }
                        }
                    }
                    else if ((!objCounterState.RequestAccepted) && objCounterState.RequestedtoAgent)
                    {
                        if (turnContext.Activity.Text.ToLower() == "disconnect" || turnContext.Activity.Text.ToLower() == "exit")
                        {
                            await new LiveChat().DisconnectAgentAsync(_socilaMinerStateAccessor, turnContext, _userState, _conversationState, _services, SocialMinerUrl);
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(SocialMinerVerbiages.REQUEST_TO_AGENT_ALREADY_SENT);
                        }
                    }
                    else
                    {
                        if (objCounterState.RequestAccepted)
                        {
                            if (turnContext.Activity.Text.ToLower() == "disconnect" || turnContext.Activity.Text.ToLower() == "exit")
                            {
                                await new LiveChat().DisconnectAgentAsync(_socilaMinerStateAccessor, turnContext, _userState, _conversationState, _services, SocialMinerUrl);

                            }
                            else
                            {
                                await new LiveChat().SendChatToAgentAsync(_socilaMinerStateAccessor, turnContext, turnContext.Activity.Text, _services,SocialMinerUrl);
                            }
                        }
                    }
                }
               
                await _conversationState.SaveChangesAsync(turnContext);
                await _userState.SaveChangesAsync(turnContext);
            }
            catch(Exception ex)
              {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);               
                TelemetryLog.LogException(_services, ex, objDictionary, "GTAADevChatBot", "OnTurnAsync");
            }
        }
        #endregion
    }
}
