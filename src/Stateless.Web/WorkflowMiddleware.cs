namespace Stateless.Web.Application.Workflow.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public class WorkflowMiddleware
    {
        private readonly RequestDelegate next;
        private readonly WorkflowMiddlewareOptions options;
        private readonly ILogger<WorkflowMiddleware> logger;
        private readonly RouteMatcher routeMatcher;

        public WorkflowMiddleware(RequestDelegate next, WorkflowMiddlewareOptions options, ILogger<WorkflowMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
            this.routeMatcher = new RouteMatcher();
        }

        public async Task Invoke(
            HttpContext httpContext,
            IWorkflowContextStorage contextStorage,
            IWorkflowContentStorage contentStorage,
            IEnumerable<IWorkflowDefinition> definitions)
        {
            if (!httpContext.Request.Path.StartsWithSegments(this.options.RoutePrefix, StringComparison.OrdinalIgnoreCase))
            {
                await this.next(httpContext).ConfigureAwait(false);
            }

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

            await this.next(httpContext).ConfigureAwait(false);
        }

        private async Task<bool> HandleGetAllRequest(
            HttpContext httpContext,
            IWorkflowContextStorage contextStorage)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true)
            {
                this.logger.LogInformation($"workflow: get all (name={segments["name"]})");
                httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(contextStorage.FindAll(), DefaultJsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        private async Task<bool> HandleGetByIdRequest(
            HttpContext httpContext,
            IWorkflowContextStorage contextStorage)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}/{id}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true && segments?.ContainsKey("id") == true)
            {
                this.logger.LogInformation($"workflow: get by id (name={segments["name"]}, id={segments["id"]})");
                var context = contextStorage.FindById(segments["id"] as string);

                if (context?.Name?.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase) == true)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(context, DefaultJsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);
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
            IWorkflowContextStorage contextStorage,
            IEnumerable<IWorkflowDefinition> definitions,
            ITransitionDispatcher dispatcher)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}/{id}/triggers", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true && segments?.ContainsKey("id") == true)
            {
                this.logger.LogInformation($"workflow: get triggers (name={segments["name"]}, id={segments["id"]})");
                var definition = definitions.Safe()
                    .FirstOrDefault(d => d.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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

                var workflow = definition.Create(context, dispatcher);

                httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(workflow.PermittedTriggers, DefaultJsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        private async Task<bool> HandleCreateNewRequest(
            HttpContext httpContext,
            IWorkflowContextStorage contextStorage,
            IWorkflowContentStorage contentStorage,
            IEnumerable<IWorkflowDefinition> definitions,
            ITransitionDispatcher dispatcher)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true)
            {
                this.logger.LogInformation($"workflow: create new (name={segments["name"]})");
                var definition = definitions.Safe()
                    .FirstOrDefault(d => d.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return true;
                }

                var context = new WorkflowContext() { Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
                // TODO: place http request content somewhere (workflowInstance.Data property?)
                var workflow = definition.Create(context, dispatcher);
                workflow.Activate();

                // store content
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
                            contentStorage.Save(context, $"{context.State}", stream, httpContext.Request.ContentType);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogWarning(ex, $"save content failed: {ex.Message}");
                    }

                    httpContext.Request.Body.Position = 0;
                }

                contextStorage.Save(context);

                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(context, DefaultJsonSerializerSettings.Create()), Encoding.UTF8).ConfigureAwait(false);
                // TODO: add location header >> this.CreatedAtAction(nameof(this.GetById), new { name, id = context.Id }, context);

                return true;
            }

            return false;
        }

        private async Task<bool> HandleFireTriggerRequest(
            HttpContext httpContext,
            IWorkflowContextStorage contextStorage,
            IWorkflowContentStorage contentStorage,
            IEnumerable<IWorkflowDefinition> definitions,
            ITransitionDispatcher dispatcher)
        {
            var segments = this.routeMatcher.Match(this.options.RoutePrefix + "/{name}/{id}/triggers/{trigger}", httpContext.Request.Path);
            if (segments?.ContainsKey("name") == true && segments?.ContainsKey("id") == true && segments?.ContainsKey("trigger") == true)
            {
                this.logger.LogInformation($"workflow: fire trigger (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]})");

                var definition = definitions.Safe()
                    .FirstOrDefault(d => d.Name.Equals(segments["name"] as string, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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

                var worklow = definition.Create(context, dispatcher);
                try
                {
                    if (await worklow.FireAsync(segments["trigger"] as string).ConfigureAwait(false))
                    {
                        this.logger.LogInformation($"workflow: trigger successfull (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]})");
                        contextStorage.Save(context);

                        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        this.logger.LogError($"workflow: trigger invalid (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]})");
                        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch(Exception ex)
                {
                    this.logger.LogCritical(ex, $"workflow: trigger failed (name={segments["name"]}, id={segments["id"]}, trigger={segments["trigger"]}) {ex.Message}");
                }
                finally
                {
                    this.logger.LogInformation($"workflow: permitted triggers={string.Join("|", worklow.PermittedTriggers)}");
                    contextStorage.Save(context);
                }

                return true;
            }

            return false;
        }
    }
}
