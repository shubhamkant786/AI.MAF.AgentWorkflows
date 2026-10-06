using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.Agent2AgentHandoff.Executors
{
    public class TesterAgentExecutor : Executor<DeveloperAgentOutput, TesterAgentOutput>
    {
        private readonly AIAgent _aIAgent;
        private readonly ExecutorOptions _executorOptions;
        public TesterAgentExecutor(AIAgent aIAgent, ExecutorOptions executorOptions) : base(nameof(TesterAgentExecutor))
        {
            _aIAgent = aIAgent;
            _executorOptions = executorOptions;
        }
        public async override ValueTask<TesterAgentOutput> HandleAsync(DeveloperAgentOutput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {

            var session = await _aIAgent.CreateSessionAsync(cancellationToken);
            var options = new AgentRunOptions()
            {
                AllowBackgroundResponses = true
            };
            var result = await _aIAgent.RunAsync(message.Output, session, options, cancellationToken);
            var testerAgentResponse = new TesterAgentOutput()
            {
                Id = _aIAgent.Id,
                Output = result.Text,
                IsPassed = result.Text.Contains("pass", StringComparison.OrdinalIgnoreCase)
            };
            await context.AddEventAsync(new WorkflowEvent(testerAgentResponse), cancellationToken);
            await context.QueueStateUpdateAsync("testerResponse", testerAgentResponse, cancellationToken);
            return testerAgentResponse;
        }
    }
}
