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
                o.Register(
                    OnOffWorkflow.Name,
                    OnOffWorkflow.States.Off, c =>
                    {
                        c.Configure(OnOffWorkflow.States.Off)
                            .OnActivate(() => Console.WriteLine("OFF!"))
                            .OnEntry((t) => Console.WriteLine($"the switch is turned {t.Destination}"))
                            .Permit(OnOffWorkflow.Triggers.Break, OnOffWorkflow.States.Broken)
                            .Permit(OnOffWorkflow.Triggers.Switch, OnOffWorkflow.States.On);
                        c.Configure(OnOffWorkflow.States.On)
                            .OnActivate(() => Console.WriteLine("ON!"))
                            .OnEntry((t) => Console.WriteLine($"the switch is turned {t.Destination}"))
                            .Permit(OnOffWorkflow.Triggers.Break, OnOffWorkflow.States.Broken)
                            .Permit(OnOffWorkflow.Triggers.Switch, OnOffWorkflow.States.Off);
                        c.Configure(OnOffWorkflow.States.Broken)
                            .OnActivate(() => Console.WriteLine("BROKEN!"))
                            .OnEntry((t) => Console.WriteLine($"the switch is broken {t.Destination}"))
                            .Permit(OnOffWorkflow.Triggers.Repair, OnOffWorkflow.States.Off);
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
