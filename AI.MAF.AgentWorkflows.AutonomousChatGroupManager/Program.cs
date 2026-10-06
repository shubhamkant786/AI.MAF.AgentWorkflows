using AI.MAF.AgentWorkflows.AutonomousChatGroupManager;
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
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a architect agent");

AIAgent developerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a developer agent");

AIAgent testerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a tester agent");

AIAgent analystAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a functional analyst agent");
//var agents= new[] { architectAgent, developerAgent, testerAgent, analystAgent };

var workflow = AgentWorkflowBuilder.CreateGroupChatBuilderWith(
                (agents) => new RoundRobinGroupChatManager(agents)
                {
                    MaximumIterationCount = 5
                })
                .AddParticipants(architectAgent, developerAgent, testerAgent, analystAgent)
                //.WithIntermediateOutputFrom(new[] { architectAgent, developerAgent, testerAgent, analystAgent })
                .WithOutputFrom(new[] { architectAgent, developerAgent, testerAgent, analystAgent })
                //.WithResponseLanguage("English")
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

var workflow1 = AgentWorkflowBuilder.CreateGroupChatBuilderWith(
                (agents) => new CustomGroupChatManager(agents)
                {
                    MaximumIterationCount = 5
                })
                .AddParticipants(architectAgent, developerAgent, testerAgent, analystAgent)
                .WithOutputFrom(new[] { architectAgent, developerAgent, testerAgent, analystAgent })
                //.WithResponseLanguage("English")
                .Build();
