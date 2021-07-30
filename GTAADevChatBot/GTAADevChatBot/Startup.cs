// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Middleware.SpellCheck;
using GTAADevChatBot.Helper;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace GTAADevChatBot
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private readonly bool _isProduction;
        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration that represents a set of key/value application configuration properties.
        /// </summary>
        /// <value>
        /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
        /// </value>
        public IConfiguration Configuration { get; }


        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> specifies the contract for a collection of service descriptors.</param>
        /// <seealso cref="IStatePropertyAccessor{T}"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/dependency-injection"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-4.0"/>
        public void ConfigureServices(IServiceCollection services)
        {
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;
            services.AddSingleton<IConfiguration>(Configuration);

            if (!File.Exists(botFilePath))
            {
                throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
            }  

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = null;
            try
            {
                botConfig = BotConfiguration.Load(botFilePath, secretKey);
            }
            catch
            {
                var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
        - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
        - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
        - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
        ";
                throw new InvalidOperationException(msg);
            }

            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. botFilePath: {botFilePath}"));

            // Retrieve current endpoint.
            var environment = _isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (service == null && _isProduction)
            {
                // Attempt to load development environment
                service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
            }

            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
            }


            AzureBlobTranscriptStore blobStore = null;

            const string StorageConfigurationId = "gtaachatbotstorage";
            var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
            if (!(blobConfig is BlobStorageService blobStorageConfig))
            {
                throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
            }

            //Fetch the secret keys from Azure Key Vault
            Utility objUtility = new Utility();
            //string blobConnectionString = objUtility.FetchVaultConnectionString().Result;
            string blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=gtaadevchatbotstorage;AccountKey=flDmWdTlzAkfKaOcRcl+MVJuKcyoY3+RH4U1uXErxZPQlXPqTLJjNj/SX8nYsl1U36bGnwGiWQZEt5q86vgFpA==;EndpointSuffix=core.windows.net";

            // Default container name.
            const string DefaultBotContainer = "botconversationstore";
            var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;

            // Default container name.
            //const string DefaultBotContainer1 = "botstatestore";
            //var storageContainer1 = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer1 : blobStorageConfig.Container;

            IStorage dataStore = new AzureBlobStorage(blobConnectionString, storageContainer);
            //IStorage dataStore = new MemoryStorage();

            //Create and add conversation state.
            var conversationState = new ConversationState(dataStore);
            services.AddSingleton(conversationState);

            var userState = new UserState(dataStore);
            services.AddSingleton(userState);


            BotServices connectedServices = InitBotServices(botConfig, blobConnectionString);
            services.AddSingleton(sp => connectedServices);
            services.AddBot<GTAADevChatBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
                
                ILogger logger = _loggerFactory.CreateLogger<GTAADevChatBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    await Task.Run(() => logger.LogError($"Exception caught : {exception}"));
                    
                   
                };

                // Enable the conversation transcript middleware.

                var storageTranscriptContainer = "gtaachatbotTranscriptstorage";
                blobStore = new AzureBlobTranscriptStore(blobConnectionString, storageTranscriptContainer);
                var transcriptMiddleware = new TranscriptLoggerMiddleware(blobStore);
                options.Middleware.Add(transcriptMiddleware);

                //options.Middleware.Add(new ShowTypingMiddleware(500, 2000));
                options.Middleware.Add(new SpellCheckMiddleware(Configuration));

            }).AddSingleton(_ => blobStore);

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
            //app.UseHttpsRedirection();
            //app.UseMvc();
        }



        private static BotServices InitBotServices(BotConfiguration config,string blobConnectionString)
        {
            TelemetryClient telemetryClient = null;
            var qnaServices = new Dictionary<string, QnAMaker>();
            var luisServices = new Dictionary<string, LuisRecognizer>();
            BlobStorageService blobStorageService1 = null;

            foreach (var service in config.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Luis:
                        {
                            if (!(service is LuisService luis))
                            {
                                throw new InvalidOperationException("The LUIS service is not configured correctly in your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(luis.AppId))
                            {
                                throw new InvalidOperationException("The LUIS Model Application Id ('appId') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(luis.AuthoringKey))
                            {
                                throw new InvalidOperationException("The Luis Authoring Key ('authoringKey') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(luis.SubscriptionKey))
                            {
                                throw new InvalidOperationException("The Subscription Key ('subscriptionKey') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(luis.Region))
                            {
                                throw new InvalidOperationException("The Region ('region') is required to run this sample. Please update your '.bot' file.");
                            }

                            var app = new LuisApplication(luis.AppId, luis.AuthoringKey, luis.GetEndpoint());
                            var recognizer = new LuisRecognizer(app, new LuisPredictionOptions() {SpellCheck = false, IncludeAllIntents = true, IncludeInstanceData = true ,Log=true});
                            luisServices.Add(luis.Name, recognizer);
                        }
                        break;

                    case ServiceTypes.Dispatch:
                        {
                            if (!(service is DispatchService dispatch))
                            {
                                throw new InvalidOperationException("The Dispatch service is not configured correctly in your '.bot' file.");
                            }

                        if (string.IsNullOrWhiteSpace(dispatch.AppId))
                        {
                            throw new InvalidOperationException("The LUIS Model Application Id ('appId') is required to run this sample. Please update your '.bot' file.");
                        }

                        if (string.IsNullOrWhiteSpace(dispatch.AuthoringKey))
                        {
                            throw new InvalidOperationException("The LUIS Authoring Key ('authoringKey') is required to run this sample. Please update your '.bot' file.");
                        }

                        if (string.IsNullOrWhiteSpace(dispatch.SubscriptionKey))
                        {
                            throw new InvalidOperationException("The Subscription Key ('subscriptionKey') is required to run this sample. Please update your '.bot' file.");
                        }

                        if (string.IsNullOrWhiteSpace(dispatch.Region))
                        {
                            throw new InvalidOperationException("The Region ('region') is required to run this sample. Please update your '.bot' file.");
                        }

                        var dispatchApp = new LuisApplication(dispatch.AppId, dispatch.AuthoringKey, dispatch.GetEndpoint());

                            // Since the Dispatch tool generates a LUIS model, we use LuisRecognizer to resolve dispatching of the incoming utterance
                            var dispatchARecognizer = new LuisRecognizer(dispatchApp, new LuisPredictionOptions() {SpellCheck = false, IncludeAllIntents = true, IncludeInstanceData = true, Log=true });
                            luisServices.Add(dispatch.Name, dispatchARecognizer);
                        }
                        
                        break;

                    case ServiceTypes.QnA:
                        {
                            if (!(service is QnAMakerService qna))
                            {
                                throw new InvalidOperationException("The QnA service is not configured correctly in your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.KbId))
                            {
                                throw new InvalidOperationException("The QnA KnowledgeBaseId ('kbId') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.EndpointKey))
                            {
                                throw new InvalidOperationException("The QnA EndpointKey ('endpointKey') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.Hostname))
                            {
                                throw new InvalidOperationException("The QnA Host ('hostname') is required to run this sample. Please update your '.bot' file.");
                            }

                            var qnaOptions = float.TryParse("0.5F", out float scoreThreshold)
                            ? new QnAMakerOptions
                            {
                                ScoreThreshold = scoreThreshold,
                                Top = 1
                            } : null;

                            var qnaEndpoint = new QnAMakerEndpoint()
                            {
                                KnowledgeBaseId = qna.KbId,
                                EndpointKey = qna.EndpointKey,
                                Host = qna.Hostname,
                            };

                            var qnaMaker = new QnAMaker(qnaEndpoint, qnaOptions);
                            qnaServices.Add(qna.Name, qnaMaker);
                            
                        }
                        break;
                    case ServiceTypes.AppInsights:
                        {
                            var appInsights = (AppInsightsService)service;
                            if (appInsights == null)
                            {
                                throw new InvalidOperationException("The Application Insights is not configured correctly in your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(appInsights.InstrumentationKey))
                            {
                                throw new InvalidOperationException("The Application Insights Instrumentation Key ('instrumentationKey') is required to run this sample.  Please update your '.bot' file.");
                            }

                            var telemetryConfig = new TelemetryConfiguration(appInsights.InstrumentationKey);
                            telemetryClient = new TelemetryClient(telemetryConfig)
                            {
                                InstrumentationKey = appInsights.InstrumentationKey,
                            };
                            
                        }
                        break;

                    case ServiceTypes.BlobStorage:
                        if (!(service is BlobStorageService blobStorageService))
                        {
                            throw new InvalidOperationException("The BlobStorage Service is not configured correctly in your '.bot' file.");
                        }
                        blobStorageService1 = new BlobStorageService() {ConnectionString= blobConnectionString, Container = blobStorageService.Container};
                        break;
                }
            }

            return new BotServices(qnaServices, luisServices, telemetryClient, blobStorageService1);
        }
    }
}
