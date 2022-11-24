using Akka.Hosting;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Cluster.Hosting;
using Akka.Event;
using Akka.Hosting.Logging;
using Akka.Logger.Serilog;
using Serilog;
using LogLevel = Akka.Event.LogLevel;
using System;
using Akka.Cluster.Sharding;
using Akka.Remote.Hosting;
using Akka.Util;
using Game.ActorModel.Actors;
using Game.ActorModel.ExternalSystems;
using Game.Web.Models;

namespace Game.Web
{
    public class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            ReceiveAny(message => {
                Sender.Tell($"{Self} rcv {message}");
            });
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllersWithViews();
            builder.Services.AddAkka("MyActorSystem", configurationBuilder =>
            {
                configurationBuilder
                    .WithActors((system, registry) =>
                    {
                        var gameController = system.ActorOf(Props.Create(() => new GameControllerActor()), "gameController");
                        registry.Register<GameControllerActor>(gameController);

                        IGameEventsPusher gameEventsPusher = new SignalRGameEventPusher();
                        var signalRBridge = system.ActorOf(Props.Create(() => new SignalRBridgeActor(gameEventsPusher, gameController)), "signalRBridge");
                        registry.Register<SignalRBridgeActor>(signalRBridge);
                    })
                    .WithActors((system, registry) =>
                    {
                        var echo = system.ActorOf(Props.Create(() => new EchoActor()), "echo");
                        registry.Register<EchoActor>(echo);
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.MapGet("/", async (context) =>
            {
                var echo = context.RequestServices.GetRequiredService<ActorRegistry>().Get<EchoActor>();
                var body = await echo.Ask<string>(
                        message: context.TraceIdentifier,
                        cancellationToken: context.RequestAborted)
                    .ConfigureAwait(false);
                await context.Response.WriteAsync(body);
            });
            
            app.Run();
        }
    }
}
