//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using Domain;
//using Domain.Model;
//using Foundation;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;

//namespace Stateless.Web.Controllers
//{
//    [ApiController]
//    [Route("api/workflows")]
//    public class WorkflowController : ControllerBase
//    {
//        private readonly ILogger<WorkflowController> logger;
//        private readonly IEnumerable<IWorkflowDefinition> definitions;
//        private readonly IWorkflowContextRepository repository;
//        private readonly IMediator mediator;

//        public WorkflowController(
//            ILogger<WorkflowController> logger,
//            IEnumerable<IWorkflowDefinition> definitions,
//            IWorkflowContextRepository repository,
//            IMediator mediator)
//        {
//            this.logger = logger;
//            this.definitions = definitions;
//            this.repository = repository;
//            this.mediator = mediator;
//        }

//        [HttpGet]
//        [Route("{name}")]
//        [ProducesResponseType((int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
//        public async Task<ActionResult<IEnumerable<WorkflowContext>>> GetAll(string name)
//        {
//            return this.Ok(this.repository.FindAll()
//                .Where(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
//        }

//        [HttpGet]
//        [Route("{name}/{id}")]
//        [ProducesResponseType((int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]

//        public async Task<ActionResult<WorkflowContext>> GetById(string name, string id)
//        {
//            var result = this.repository.FindById(id);

//            if(result?.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == false)
//            {
//                result = null;
//            }

//            return result != null
//                ? this.Ok(result) : (ActionResult<WorkflowContext>)this.NotFound();
//        }


//        [HttpGet]
//        [Route("{name}/{id}/triggers")]
//        [ProducesResponseType((int)HttpStatusCode.Accepted)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]

//        public async Task<ActionResult<IEnumerable<string>>> GetTriggers(string name, string id)
//        {
//            var definition = this.definitions.Safe()
//                .FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
//            if (definition == null)
//            {
//                return this.BadRequest();
//            }

//            var context = this.repository.FindById(id);
//            if (context == null)
//            {
//                return this.NotFound();
//            }

//            if(!context.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
//            {
//                return this.BadRequest();
//            }

//            var workflow = definition.Create(context, this.mediator);

//            return this.Ok(workflow.PermittedTriggers);
//        }

//        [HttpPost]
//        [Route("{name}")]
//        [ProducesResponseType((int)HttpStatusCode.Created)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]

//        public async Task<ActionResult> CreateNew(string name)
//        {
//            var definition = this.definitions.Safe()
//                .FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
//            if (definition == null)
//            {
//                return this.BadRequest();
//            }

//            var context = new WorkflowContext() { Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
//            // TODO: place http request content somewhere (workflowInstance.Data property?)
//            var workflow = definition.Create(context, this.mediator);
//            workflow.Activate();

//            //workflowInstance.Name = workflowDefinition.Name;
//            //workflowInstance.State = machine.Instance.State;

//            this.repository.Upsert(context);

//            return this.CreatedAtAction(nameof(this.GetById), new { name, id = context.Id }, context);
//        }

//        [HttpPut]
//        [Route("{name}/{id}/triggers/{trigger}")]
//        [ProducesResponseType((int)HttpStatusCode.Accepted)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]

//        public async Task<ActionResult> FireTrigger(string name, string id, string trigger)
//        {
//            var definition = this.definitions.Safe()
//                .FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
//            if (definition == null)
//            {
//                return this.BadRequest();
//            }

//            var context = this.repository.FindById(id);
//            if (context == null)
//            {
//                return this.NotFound();
//            }

//            if (!context.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
//            {
//                return this.BadRequest();
//            }

//            if (trigger.IsNullOrEmpty())
//            {
//                return this.BadRequest();
//            }

//            var worklow = definition.Create(context, this.mediator);
//            if (worklow.Fire(trigger))
//            {
//                this.repository.Upsert(context);

//                return this.Accepted();
//            }
//            else
//            {
//                return this.BadRequest();
//            }
//        }
//    }
//}
