using AI.MAF.AgentWorkflows.OrchestrateManager.Executors;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Specialized.Magentic;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Text;

Console.WriteLine("Hello, World!");

const string deploymentName = "gpt-5.2";
const string endpointOpenAI = "https://shubham-ms-foundry.services.ai.azure.com/openai/v1";
const string apiKey = $"";

var openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey),
         new OpenAIClientOptions() { Endpoint = new Uri(endpointOpenAI) });


IChatClient chatClient = openAIClient.GetChatClient(deploymentName).AsIChatClient();

AIAgent hotelmanagerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a hotel room guest query classifier and analysing agent. " +
        "You have to manage input classified guest query data to four other agents with different modes of work type viz Plumbing, HouseKeeping, Electrical or Restaurant." +
        "You also provide assurance to user on quick resolution" +
        "Sample output: The guest in room 101 has reported a plumbing issue with the bathroom sink. Please provide a time within the guest's stay to fix the issue.");


AIAgent plumbingAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a plumbing query planning agent. You need to take in Room Number and the plumbing issue details. Provide a time within stay of guest to fix issue");

AIAgent housekeepingAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a housekeeping query planning agent. You need to take in Room Number and the housekeeping request details.");

AIAgent electricalAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are an electrical query planning agent. You need to take in Room Number and the electrical issue details.");

AIAgent restaurantAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a restaurant query planning agent. You need to take in Room Number and the food order request details.");

var workflow = AgentWorkflowBuilder.CreateMagenticBuilderWith(hotelmanagerAgent)
                .AddParticipants(plumbingAgent, housekeepingAgent, electricalAgent, restaurantAgent)
                .WithMaxStalls(3)
                .WithMaxRounds(3)
                .WithMaxResets(3)
                .WithIntermediateOutputFrom(new[] { plumbingAgent, housekeepingAgent, electricalAgent, restaurantAgent })
                .WithOutputFrom([plumbingAgent, housekeepingAgent, electricalAgent, restaurantAgent])
                //.WithResponseLanguage("English")
                .Build();

var checkpointManager = CheckpointManager.CreateInMemory();
var userQuery = "Hey, my bathroom washbasin is leaking. I am in room 101.";
var session = await workflow.AsAIAgent().CreateSessionAsync();
Console.WriteLine($"{workflow.ToMermaidString()}");


var run = await InProcessExecution.RunAsync(workflow, userQuery, checkpointManager);

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

    if (evt is MagenticPlanCreatedEvent magenticPlanCreatedEvent)
    {
        Console.Write($"{magenticPlanCreatedEvent.Data}");
    }
    if (evt is RequestInfoEvent requestInfoEvent)
    {
        Console.Write($"{requestInfoEvent.Data}");
    }



    if (evt is SuperStepCompletedEvent superStepCompleted)
    {
        Console.Write($"{superStepCompleted.Data}");
    }
}
Console.WriteLine("----------------------------------------------------------");

//run breaks
//var checkpointToResume = await checkpointManager.GetLatestCheckpointAsync(run.SessionId);
//var resumeRun = await InProcessExecution.ResumeAsync(workflow, checkpointToResume, checkpointManager);

var responseAgent = chatClient.AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a hotel room guest query responder agent. " +
        "You have to gather output from four other agents with different modes of work type viz Plumbing, HouseKeeping, Electrical or Restaurant." +
        "You also provide assurance to user on quick resolution" +
        "Sample output: Please be assured,We have informed plumbing team and plumbing issue with the bathroom sink. It will be fixed this afternoon.");
var options = ExecutorOptions.Default;
options.AutoSendMessageHandlerResultObject = true;
options.AutoYieldOutputHandlerResultObject = true;
var hotelManagerAgentExecutor = new HotelManagerAgentExecutor(hotelmanagerAgent, options);
var plumbingAgentExecutor = new PlumbingAgentExecutor(plumbingAgent, options);
var housekeepingAgentExecutor = new HousekeepingAgentExecutor(housekeepingAgent, options);
var electricalAgentExecutor = new ElectricalAgentExecutor(electricalAgent, options);
var restaurantAgentExecutor = new RestaurantAgentExecutor(restaurantAgent, options);
var responseAgentExecutor = new ResponseAgentExecutor(responseAgent, options);

var workflow1 = new WorkflowBuilder(hotelManagerAgentExecutor)
    .AddFanOutEdge(hotelManagerAgentExecutor, [plumbingAgentExecutor, housekeepingAgentExecutor, electricalAgentExecutor, restaurantAgentExecutor])
    .AddFanInBarrierEdge([plumbingAgentExecutor, housekeepingAgentExecutor, electricalAgentExecutor, restaurantAgentExecutor], responseAgentExecutor)
    .WithOutputFrom([responseAgentExecutor])
    .Build();

var streamingRun1 = await InProcessExecution.RunStreamingAsync(workflow1    , userQuery);

await foreach (WorkflowEvent evt in streamingRun1.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent output)
    {
        Console.WriteLine($"Final answer: {output.Data}");
    }
}
