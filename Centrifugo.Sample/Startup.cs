using Centrifugo.Client;
using Centrifugo.Client.Abstractions;
using Centrifugo.Client.Options;
using Centrifugo.Sample.Models;
using Centrifugo.Sample.TokenProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace Centrifugo.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddOptions<AuthOptions>();
            services.AddSingleton<ICentrifugoTokenProvider, CentrifugoTokenProvider>();
            services.AddSingleton<IWebsocketClient>(sp => new WebsocketClient(new Uri("ws://localhost:8000/connection/websocket?format=protobuf")));
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<CentrifugoClient>>();
                var ws = sp.GetRequiredService<IWebsocketClient>();
                var tokenProvider = sp.GetRequiredService<ICentrifugoTokenProvider>();

                ICentrifugoClient client = new CentrifugoClient(ws);

                client.OnConnect(e => logger.LogInformation(e.Client.ToString()));
                // onError, OnDisconnect

                var subscription = client.CreateNewSubscription("test-channel");

                subscription.OnSubscribe(e =>
                {
                    logger.LogInformation("SUBSCRIBED! channel name: " + e.Channel);
                });
                subscription.OnUnsubscribe(e =>
                {
                    logger.LogInformation("UNSUBSCRIBED! channel name: " + e.Channel);
                });
                subscription.OnPublish(publishEvent =>
                {
                    if (publishEvent.Data != null)
                    {
                        var s = publishEvent.Data.ToStringUtf8();
                        var json = JsonConvert.DeserializeObject<TestMessage>(s);

                        logger.LogInformation("OK! data: " + json);
                    } else
                    {
                        logger.LogInformation("OK!");
                    }

                    return Task.CompletedTask;
                });

                Task.Run(async () =>
                {
                    var token = await tokenProvider
                        .GenerateTokenAsync("SomeClientId", "my name is client");

                    client.SetToken(token);
                    await client.ConnectAsync();
                    await client.SubscribeAsync(subscription);
                }).GetAwaiter().GetResult();

                return client;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
