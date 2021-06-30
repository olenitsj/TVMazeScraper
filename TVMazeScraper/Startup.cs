﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TVMazeScraper.Client;
using TVMazeScraper.Interfaces;

[assembly: FunctionsStartup(typeof(TVMazeScraper.Startup))]

namespace TVMazeScraper
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddTransient<ITvMazeApiClient, TvMazeApiClient>();
        }
    }
}