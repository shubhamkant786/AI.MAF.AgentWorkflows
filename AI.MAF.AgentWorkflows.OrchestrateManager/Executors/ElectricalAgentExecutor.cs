using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AI.MAF.AgentWorkflows.OrchestrateManager.Executors;

public class ElectricalAgentExecutor : Executor<string, string>
{
    private readonly AIAgent _aIAgent;
    private readonly ExecutorOptions _executorOptions;

    public ElectricalAgentExecutor(AIAgent aIAgent, ExecutorOptions executorOptions)
        : base(nameof(ElectricalAgentExecutor))
    {
        _aIAgent = aIAgent;
        _executorOptions = executorOptions;
    }

    public override async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var session = await _aIAgent.CreateSessionAsync(cancellationToken);
        var options = new AgentRunOptions { AllowBackgroundResponses = true };
        var result = await _aIAgent.RunAsync(message, session, options, cancellationToken);
        await context.AddEventAsync(new WorkflowEvent(result.Text), cancellationToken);
        await context.QueueStateUpdateAsync("electricalResponse", result.Text, cancellationToken);
        return result.Text;
    }
}
