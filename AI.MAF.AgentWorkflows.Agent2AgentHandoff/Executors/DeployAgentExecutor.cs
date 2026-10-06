using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.Agent2AgentHandoff.Executors
{
    public class DeployAgentExecutor : Executor<TesterAgentOutput, string>
    {
        private readonly AIAgent _aIAgent;
        private readonly ExecutorOptions _executorOptions;
        public DeployAgentExecutor(AIAgent aIAgent, ExecutorOptions executorOptions) : base(nameof(DeployAgentExecutor))
        {
            _aIAgent = aIAgent;
            _executorOptions = executorOptions;
        }
        public async override ValueTask<string> HandleAsync(TesterAgentOutput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var session = await _aIAgent.CreateSessionAsync(cancellationToken);
            var options = new AgentRunOptions()
            {
                AllowBackgroundResponses = true
            };
            var result = await _aIAgent.RunAsync(message.Output, session, options, cancellationToken);
            if (message.IsPassed)
            {
                await context.AddEventAsync(new WorkflowEvent($"Tester Agent has passed the test. Output: {result.Text}. Deployment will proceed."), cancellationToken);
            }
            else
            {
                await context.AddEventAsync(new WorkflowEvent($"Tester Agent has failed the test. Output: {result.Text}. Deployment on hold."), cancellationToken);
            }
            await context.YieldOutputAsync(result.Text, cancellationToken);
            return result.Text;
        }
    }
}
