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

namespace Game.Web
{
    public struct Echo { }

    public class EchoActor : ReceiveActor
    {
        private readonly string _entityId;
        public EchoActor(string entityId)
        {
            _entityId = entityId;
            ReceiveAny(message => {
                Sender.Tell($"{Self} rcv {message}");
            });
        }
    }

    public class Program
    {
        private const int NumberOfShards = 5;

        private static Option<(string, object)> ExtractEntityId(object message)
            => message switch
            {
                string id => (id, id),
                _ => Option<(string, object)>.None
            };

        private static string? ExtractShardId(object message)
            => message switch
            {
                string id => (id.GetHashCode() % NumberOfShards).ToString(),
                _ => null
            };

        private static Props PropsFactory(string entityId)
            => Props.Create(() => new EchoActor(entityId));

        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllersWithViews();
            builder.Services.AddAkka("MyActorSystem", configurationBuilder =>
            {
                configurationBuilder
                    .WithRemoting(hostname: "localhost", port: 8110)
                    .WithClustering(new ClusterOptions { SeedNodes = new[] { Address.Parse("akka.tcp://MyActorSystem@localhost:8110"), } })
                    .WithShardRegion<Echo>(
                        typeName: "myRegion",
                        entityPropsFactory: PropsFactory,
                        extractEntityId: ExtractEntityId,
                        extractShardId: ExtractShardId,
                        shardOptions: new ShardOptions());
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
                var echo = context.RequestServices.GetRequiredService<ActorRegistry>().Get<Echo>();
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
