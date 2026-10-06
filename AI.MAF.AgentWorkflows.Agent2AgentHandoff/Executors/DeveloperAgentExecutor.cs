using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.Agent2AgentHandoff.Executors
{
    public class DeveloperAgentExecutor : Executor<string, DeveloperAgentOutput>
    {
        private readonly AIAgent _aIAgent;
        private readonly ExecutorOptions _executorOptions;
        public DeveloperAgentExecutor(AIAgent aIAgent, ExecutorOptions executorOptions) : base(nameof(DeveloperAgentExecutor))
        {
            _aIAgent = aIAgent;
            _executorOptions = executorOptions;
        }
        public async override ValueTask<DeveloperAgentOutput> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {

            var session = await _aIAgent.CreateSessionAsync(cancellationToken);
            var options = new AgentRunOptions()
            {
                AllowBackgroundResponses = true
            };
            var result = await _aIAgent.RunAsync(message, session, options, cancellationToken);
            var developerAgentResponse = new DeveloperAgentOutput()
            {
                Id = _aIAgent.Id,
                Output = result.Text
            };
            await context.AddEventAsync(new WorkflowEvent(developerAgentResponse), cancellationToken);
            await context.QueueStateUpdateAsync("developerResponse", developerAgentResponse, cancellationToken);
            return developerAgentResponse;
        }
    }
}
