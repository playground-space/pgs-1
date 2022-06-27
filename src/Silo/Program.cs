using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

var host = Host
    .CreateDefaultBuilder(args)
    .UseOrleans(builder =>
    {

        builder
            .UseLocalhostClustering(
                siloPort: 11_111,
                gatewayPort: 30_000,
                primarySiloEndpoint: null,
                serviceId: "dev",
                clusterId: "dev");
    })
    .Build();

await host.RunAsync();