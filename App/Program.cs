using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.RegisterClasses())
    .ConfigureAppConfiguration(app => { app.AddUserSecrets<Program>();})
    .Build();
await host.Services.GetService<IBot>().Run(host.Services).ConfigureAwait(false);