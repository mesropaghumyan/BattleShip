using BattleShip.App;
using BattleShip.App.Services;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Fb = BattleShip.Grpc;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// BattleShip.API's own address (AD-8: no ProjectReference, network calls only). Game creation and state
// reads travel over plain HTTP; FireShot travels over gRPC-Web on the same origin (AD-4).
// Plain HTTP (API's "http" launch profile, the one dotnet run uses without an explicit --launch-profile) so
// running both projects with the default profile just works, with no dev-certs trust step required.
const string ApiBaseAddress = "http://localhost:5268";

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(ApiBaseAddress) });
builder.Services.AddScoped<GameHttpClient>();

builder.Services.AddScoped(_ =>
{
    var channel = GrpcChannel.ForAddress(ApiBaseAddress, new GrpcChannelOptions
    {
        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
    });

    return new Fb.Battlefield.BattlefieldClient(channel);
});
builder.Services.AddScoped<ShotGrpcClient>();

await builder.Build().RunAsync();
