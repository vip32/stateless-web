namespace Stateless.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class StatelessMiddleware
    {
        private readonly RequestDelegate next;
        private readonly StatelessMiddlewareOptions options;
        private readonly ILogger<StatelessMiddleware> logger;
        private readonly RouteMatcher routeMatcher;

        public StatelessMiddleware(RequestDelegate next, StatelessMiddlewareOptions options, ILogger<StatelessMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
            this.routeMatcher = new RouteMatcher();
        }

        public async Task Invoke(
            HttpContext httpContext,
            IStateMachineContextStorage contextStorage,
            IStateMachineContentStorage contentStorage,
            IEnumerable<IStatemachineDefinition> definitions)
        {
            if (httpContext.Request.Path.StartsWithSegments(this.options.RoutePrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (httpContext.Request.Method.SafeEquals("get"))
                {
                    if (await this.HandleGetAllRequest(httpContext, contextStorage).ConfigureAwait(false))
                    {
                        return;
                    }

                    if (await this.HandleGetByIdRequest(httpContext, contextStorage).ConfigureAwait(false))
                    {
                        return;
                    }

                    if (await this.HandleGetTriggersRequest(httpContext, contextStorage, definitions, httpContext.RequestServices.GetService<ITransitionDispatcher>()).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                else if (httpContext.Request.Method.SafeEquals("post"))
                {
                    if (await this.HandleCreateNewRequest(httpContext, contextStorage, contentStorage, definitions, httpContext.RequestServices.GetService<ITransitionDispatcher>()).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                else if (httpContext.Request.Method.SafeEquals("put"))
                {
                    if (await this.HandleFireTriggerRequest(httpContext, contextStorage, contentStorage, definitions, httpContext.RequestServices.GetService<ITransitionDispatcher>()).ConfigureAwait(false))
                    {
                        return;
                    }
                }
            }

            await this.next(httpContext).ConfigureAwait(false);
        }

        private async Task<bool> HandleGetAllRequest(
            HttpContext httpContext,
            IStateMachineContextStorage contextStorage)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true)
            {
                this.logger.LogInformation($"statemachine: get all (name={segments["name"]})");
                httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(contextStorage.FindAll(), JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        private async Task<bool> HandleGetByIdRequest(
            HttpContext httpContext,
            IStateMachineContextStorage contextStorage)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}/{id}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true && segments?.ContainsKey("id") == true)
            {
                this.logger.LogInformation($"statemachine: get by id (name={segments["name"]}, id={segments["id"]})");
                var context = contextStorage.FindById(segments["id"] as string);

                if (context?.Name?.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase) == true)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(context, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);
                }
                else
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                return true;
            }

            return false;
        }

        private async Task<bool> HandleGetTriggersRequest(
            HttpContext httpContext,
            IStateMachineContextStorage contextStorage,
            IEnumerable<IStatemachineDefinition> definitions,
            ITransitionDispatcher dispatcher)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}/{id}/triggers", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true && segments?.ContainsKey("id") == true)
            {
                this.logger.LogInformation($"statemachine: get triggers (name={segments["name"]}, id={segments["id"]})");
                var definition = definitions.Safe()
                    .FirstOrDefault(d => d.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var details = new ProblemDetails()
                    {
                        Title = "state machine definition not found",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"{this.options.RoutePrefix}/{segments["name"]}/{segments["id"]}",
                    };

                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(details, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                    return true;
                }

                var context = contextStorage.FindById(segments["id"] as string);
                if (context == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return true;
                }

                if (!context.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return true;
                }

                var instance = definition.CreateInstance(context, dispatcher);

                httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Headers.Add("Location", $"{this.options.RoutePrefix}/{segments["name"]}/{context.Id}");
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(instance.PermittedTriggers, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        private async Task<bool> HandleCreateNewRequest(
            HttpContext httpContext,
            IStateMachineContextStorage contextStorage,
            IStateMachineContentStorage contentStorage,
            IEnumerable<IStatemachineDefinition> definitions,
            ITransitionDispatcher dispatcher)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true)
            {
                this.logger.LogInformation($"statemachine: create new (name={segments["name"]})");
                var definition = definitions.Safe()
                    .FirstOrDefault(d => d.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var details = new ProblemDetails()
                    {
                        Title = "state machine definition not found",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"{this.options.RoutePrefix}/{segments["name"]}",
                    };

                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(details, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                    return true;
                }

                var context = new StateMachineContext() { Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
                var instance = definition.CreateInstance(context, dispatcher);
                instance.Activate();
                await this.StoreContent(httpContext, "_current", context, contextStorage, contentStorage).ConfigureAwait(false);
                await this.StoreContent(httpContext, context.State, context, contextStorage, contentStorage).ConfigureAwait(false);

                httpContext.Response.StatusCode = (int)HttpStatusCode.Created;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Headers.Add("Location", $"{this.options.RoutePrefix}/{segments["name"]}/{context.Id}");
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(context, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        private async Task<bool> HandleFireTriggerRequest(
            HttpContext httpContext,
            IStateMachineContextStorage contextStorage,
            IStateMachineContentStorage contentStorage,
            IEnumerable<IStatemachineDefinition> definitions,
            ITransitionDispatcher dispatcher)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}/{id}/triggers/{trigger}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true && segments?.ContainsKey("id") == true && segments?.ContainsKey("trigger") == true)
            {
                this.logger.LogInformation($"statemachine: fire trigger (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]})");

                var definition = definitions.Safe()
                    .FirstOrDefault(d => d.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var details = new ProblemDetails()
                    {
                        Title = "state machine definition not found",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"{this.options.RoutePrefix}/{segments["name"]}/{segments["id"]}",
                    };

                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(details, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);
                    return true;
                }

                var context = contextStorage.FindById(segments["id"] as string);
                if (context == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return true;
                }

                if (context.IsExpired())
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var details = new ProblemDetails()
                    {
                        Title = "state machine is expired",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"{this.options.RoutePrefix}/{context.Name}/{context.Id}",
                    };

                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(details, JsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                    return true;
                }

                if (!context.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return true;
                }

                var instance = definition.CreateInstance(context, dispatcher);
                try
                {
                    await this.StoreContent(httpContext, "_current", context, contextStorage, contentStorage).ConfigureAwait(false);
                    if (await instance.FireAsync(segments["trigger"] as string).ConfigureAwait(false))
                    {
                        await this.StoreContent(httpContext, context.State, context, contextStorage, contentStorage).ConfigureAwait(false);
                        this.logger.LogInformation($"statemachine: trigger successfull (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]})");
                        contextStorage.Save(context);

                        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        this.logger.LogError($"statemachine: trigger invalid (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]})");
                        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch(Exception ex)
                {
                    this.logger.LogCritical(ex, $"statemachine: trigger failed (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]}) {ex.Message}");
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                finally
                {
                    this.logger.LogInformation($"statemachine: permitted triggers={string.Join("|", instance.PermittedTriggers)}");
                    contextStorage.Save(context);
                }

                return true;
            }

            return false;
        }

        private async Task StoreContent(HttpContext httpContext, string key, StateMachineContext context, IStateMachineContextStorage contextStorage, IStateMachineContentStorage contentStorage)
        {
            var contentLength = httpContext.Request.ContentLength ?? 0;
            if (httpContext.Request.Body != null && contentLength > 0)
            {
                httpContext.Request.EnableBuffering(); // allow multiple reads
                httpContext.Request.Body.Position = 0;
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        await httpContext.Request.Body.CopyToAsync(stream).ConfigureAwait(false);
                        this.logger.LogInformation($"statemachine: store content (name={context.Name}, id={context.Id}, key={key}, size={stream.Length}, type={httpContext.Request.ContentType})");
                        contentStorage.Save(context, $"{key}", stream, httpContext.Request.ContentType);
                        context.AddContent(key, httpContext.Request.ContentType, stream.Length);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(ex, $"save content failed: {ex.Message}");
                }

                httpContext.Request.Body.Position = 0;
            }

            contextStorage.Save(context);
        }
    }
}
