using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.Agent2AgentHandoff.Executors
{
    public class AnalystAgentExecutor : Executor<string, AnalystAgentOutput>
    {
        private readonly AIAgent _aIAgent;
        private readonly ExecutorOptions _executorOptions;
        public AnalystAgentExecutor(AIAgent aIAgent, ExecutorOptions executorOptions) : base(nameof(AnalystAgentExecutor))
        {
            _aIAgent = aIAgent;
            _executorOptions = executorOptions;
        }
        public async override ValueTask<AnalystAgentOutput> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {

            var session = await _aIAgent.CreateSessionAsync(cancellationToken);
            var options = new AgentRunOptions()
            {
                AllowBackgroundResponses = true
            };
            var result = await _aIAgent.RunAsync(message, session, options, cancellationToken);
            var analystAgentResponse = new AnalystAgentOutput()
            {
                Id = _aIAgent.Id,
                UserStoryDefinition = result.Text
            };
            await context.AddEventAsync(new WorkflowEvent(analystAgentResponse), cancellationToken);
            await context.QueueStateUpdateAsync("analystResponse", analystAgentResponse, cancellationToken);
            return analystAgentResponse;
        }
    }
}
