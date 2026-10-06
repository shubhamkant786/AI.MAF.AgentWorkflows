using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Executors;
using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

Console.WriteLine("Hello, World!");


const string deploymentName = "gpt-5.2";
const string endpointOpenAI = "https://shubham-ms-foundry.services.ai.azure.com/openai/v1";
const string apiKey = $"";

var openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey),
         new OpenAIClientOptions() { Endpoint = new Uri(endpointOpenAI) });


IChatClient chatClient = openAIClient.GetChatClient(deploymentName).AsIChatClient();

AIAgent analystAgent = chatClient
        .AsAIAgent(name: "analyst-agent"
        , instructions: "You are a analyst agent");

AIAgent developerAgent = chatClient
        .AsAIAgent(name: "developer-agent"
        , instructions: "You are a developer agent");

AIAgent testerAgent = chatClient
        .AsAIAgent(name: "tester-agent"
        , instructions: "You are a tester agent");

AIAgent deployAgent = chatClient
        .AsAIAgent(name: "deploy-agent"
        , instructions: "You are a deploy agent");
var options = ExecutorOptions.Default;
options.AutoSendMessageHandlerResultObject = true;
options.AutoYieldOutputHandlerResultObject = true;
var analystAgentExecutor = new AnalystAgentExecutor(analystAgent, options);
var developerAgentExecutor = new DeveloperAgentExecutor(developerAgent, options);
var testerAgentExecutor = new TesterAgentExecutor(testerAgent, options);
var deployAgentExecutor = new DeployAgentExecutor(deployAgent, options);
var notificationExecutor = new NotificationExecutor(options);

var workflow = new WorkflowBuilder(analystAgentExecutor)
    .AddEdge<AnalystAgentOutput>(analystAgentExecutor, developerAgentExecutor, t=>t is not null)
    .AddEdge<DeveloperAgentOutput>(developerAgentExecutor, testerAgentExecutor, t=>t is not null)
    .AddEdge<TesterAgentOutput>(testerAgentExecutor, deployAgentExecutor, t => t.IsPassed)
    .AddEdge(deployAgentExecutor, notificationExecutor)
    .Build();

var workflow1 = AgentWorkflowBuilder.CreateHandoffBuilderWith(analystAgent)
    .AddParticipants(analystAgent, developerAgent, testerAgent, deployAgent)
    .WithHandoffInstructions("You are a analyst agent. You need to take in the business requirements and provide user story with acceptance criteria.")
    .EnableReturnToPrevious()
    .WithToolCallFilteringBehavior(HandoffToolCallFilteringBehavior.HandoffOnly)
    .EmitAgentResponseEvents()
    .EmitAgentResponseUpdateEvents()
    .WithHandoff(analystAgent, developerAgent, handoffReason: "Analyst has completed the analysis and is handing off to developer for implementation.")
    .WithHandoff(developerAgent, testerAgent, handoffReason: "Developer has completed the implementation and is handing off to tester for testing.")
    .WithHandoff(testerAgent, deployAgent, handoffReason: "Tester has completed the testing and is handing off to deploy agent for deployment.")
    .WithIntermediateOutputFrom(new[] { developerAgent, testerAgent })
    .WithOutputFrom(new[] { deployAgent })
    .Build();

var businessRequirements = "The system should allow users to create an account, log in, and reset their password. The system should also provide an admin panel for managing users and viewing analytics.";

var run = await InProcessExecution.RunAsync(workflow, businessRequirements);

foreach (WorkflowEvent evt in run.OutgoingEvents)
{
    if (evt is WorkflowStartedEvent startEvent)
        Console.WriteLine($"{startEvent}");
    if (evt is SuperStepStartedEvent superStepStartedEvent)
        Console.WriteLine($"{superStepStartedEvent}");
    if (evt is WorkflowErrorEvent errorEvent)
        Console.WriteLine($"{errorEvent}");
    if (evt is ExecutorInvokedEvent invokedEvent)
        Console.WriteLine($"{invokedEvent}");
    if (evt is ExecutorFailedEvent failedEvent)
        Console.WriteLine($"{failedEvent}");
    if (evt is ExecutorCompletedEvent completedEvent)
        Console.WriteLine($"{completedEvent}");
    if (evt is WorkflowOutputEvent output)
    {
        Console.Write($"{output.Data}");
    }
}
Console.WriteLine("----------------------------------------------------------");

var run1 = await InProcessExecution.RunAsync(workflow1, businessRequirements);

foreach (WorkflowEvent evt in run1.OutgoingEvents)
{
    if (evt is WorkflowStartedEvent startEvent)
        Console.WriteLine($"{startEvent}");
    if (evt is SuperStepStartedEvent superStepStartedEvent)
        Console.WriteLine($"{superStepStartedEvent}");
    if (evt is WorkflowErrorEvent errorEvent)
        Console.WriteLine($"{errorEvent}");
    if (evt is ExecutorInvokedEvent invokedEvent)
        Console.WriteLine($"{invokedEvent}");
    if (evt is ExecutorFailedEvent failedEvent)
        Console.WriteLine($"{failedEvent}");
    if (evt is ExecutorCompletedEvent completedEvent)
        Console.WriteLine($"{completedEvent}");
    if (evt is WorkflowOutputEvent output)
    {
        Console.Write($"{output.Data}");
    }
}
Console.WriteLine("----------------------------------------------------------");

