namespace Stateless.Web.Application
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Stateless.Web;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStateless(o =>
            {
                o.UseLiteDBStorage();
                o.AddStateMachine(
                    OnOffStateMachine.Name,
                    OnOffStateMachine.States.Off, c =>
                    {
                        c.Configure(OnOffStateMachine.States.Off)
                            .OnActivate(() => Console.WriteLine("OFF!"))
                            .OnEntry((t) => Console.WriteLine($"the switch is turned {t.Destination}"))
                            .Permit(OnOffStateMachine.Triggers.Break, OnOffStateMachine.States.Broken)
                            .Permit(OnOffStateMachine.Triggers.Switch, OnOffStateMachine.States.On);
                        c.Configure(OnOffStateMachine.States.On)
                            .OnActivate(() => Console.WriteLine("ON!"))
                            .OnEntry((t) => Console.WriteLine($"the switch is turned {t.Destination}"))
                            .Permit(OnOffStateMachine.Triggers.Break, OnOffStateMachine.States.Broken)
                            .Permit(OnOffStateMachine.Triggers.Switch, OnOffStateMachine.States.Off);
                        c.Configure(OnOffStateMachine.States.Broken)
                            .OnActivate(() => Console.WriteLine("BROKEN!"))
                            .OnEntry((t) => Console.WriteLine($"the switch is broken {t.Destination}"))
                            .Permit(OnOffStateMachine.Triggers.Repair, OnOffStateMachine.States.Off);
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStateless();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
