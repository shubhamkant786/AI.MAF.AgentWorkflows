using AI.MAF.AgentWorkflows.AutonomousChatGroupManager;
using AI.MAF.AgentWorkflows.AutonomousChatGroupManager.Executors;
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


AIAgent scrumMasterAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a scrum master agent. " +
        "You have to manage different jira ticket issues and get inputs from other agents." +
        "Sample input: How should we approach new epic about email integration?" +
        "Sample output: Architect: Solution should be designed to handle email integration efficiently." +
        "Developer: Implementaion wise prerequisite are smptp mail server. Do we have one already?" +
        "Tester: For testing, we can create also a test server. Will it need user verification?");


AIAgent architectAgent = chatClient
        .AsAIAgent(name: "architectagent"
        , instructions: "You are a architect agent");

AIAgent developerAgent = chatClient
        .AsAIAgent(name: "developer-agent"
        , instructions: "You are a developer agent");

AIAgent testerAgent = chatClient
        .AsAIAgent(name: "tester-agent"
        , instructions: "You are a tester agent");

AIAgent analystAgent = chatClient
        .AsAIAgent(name: "analyst-agent"
        , instructions: "You are a functional analyst agent");

var options = ExecutorOptions.Default;
options.AutoSendMessageHandlerResultObject = true;
options.AutoYieldOutputHandlerResultObject = true;

var scrumMasterAgentExecutor = new ScrumMasterAgentExecutor(scrumMasterAgent, options);
var architectAgentExecutor = new ArchitectAgentExecutor(architectAgent, options);
var developerAgentExecutor = new DeveloperAgentExecutor(developerAgent, options);
var testerAgentExecutor = new TesterAgentExecutor(testerAgent, options);
var analystAgentExecutor = new AnalystAgentExecutor(analystAgent, options);

var workflow = new WorkflowBuilder(scrumMasterAgentExecutor)
                .AddFanOutEdge(scrumMasterAgentExecutor,
                    [architectAgentExecutor, developerAgentExecutor, testerAgentExecutor, analystAgentExecutor])
                .WithOutputFrom([architectAgentExecutor, developerAgentExecutor, testerAgentExecutor, analystAgentExecutor])
                .Build();

Console.WriteLine($"{workflow.ToMermaidString()}");
var userQuery = "How should we approach new epic about sms integration?";
var run = await InProcessExecution.RunAsync(workflow, userQuery);

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



var workflow1 = new WorkflowBuilder(scrumMasterAgentExecutor)
                .AddFanOutEdge(scrumMasterAgentExecutor,
                    [analystAgentExecutor, architectAgentExecutor, developerAgentExecutor, testerAgentExecutor])
                .WithOutputFrom([analystAgentExecutor, architectAgentExecutor, developerAgentExecutor, testerAgentExecutor])
                .Build();

var run1 = await InProcessExecution.RunAsync(workflow1, userQuery);

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
