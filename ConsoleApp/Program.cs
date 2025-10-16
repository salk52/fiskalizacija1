using ConsoleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables()
					 .AddCommandLine(args)
					 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					 .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

builder.Services.AddOptions();

builder.Services.Configure<FiskalizacijaOptions>(builder.Configuration.GetSection(FiskalizacijaOptions.Key));

builder.Services.AddSingleton<FiskalizacijaApp>();

var app = builder.Build();

var fiskalizacijaApp = app.Services.GetRequiredService<FiskalizacijaApp>();

await fiskalizacijaApp.Run();
