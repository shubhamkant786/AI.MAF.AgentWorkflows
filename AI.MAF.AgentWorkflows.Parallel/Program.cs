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

AIAgent queryTravelPlannerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a travel query classifier and analysing agent. " +
        "You have to feed data to three other agents with different modes of transport viz Air, Train, and Bus." +
        $"You also provide input to Hotel Finder Agent." +
        "You also provide recommendations based on the user's preferences.");
var queryTravelPlannerAgentExecutor = queryTravelPlannerAgent.BindAsExecutor();

AIAgent airTravelPlannerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a air travel query planning agent. ");
var airTravelPlannerAgentExecutor = airTravelPlannerAgent.BindAsExecutor(new AIAgentHostOptions()
{
    EmitAgentResponseEvents = true,
    EmitAgentUpdateEvents = true,
    ForwardIncomingMessages = true
});

AIAgent trainTravelPlannerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a train travel query planning agent. ");
var trainTravelPlannerAgentExecutor = trainTravelPlannerAgent.BindAsExecutor(new AIAgentHostOptions()
{
    EmitAgentResponseEvents = true,
    EmitAgentUpdateEvents = true,
    ForwardIncomingMessages = true
});

AIAgent busTravelPlannerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a bus travel query planning agent. ");
var busTravelPlannerAgentExecutor = busTravelPlannerAgent.BindAsExecutor(new AIAgentHostOptions()
{
    EmitAgentResponseEvents = true,
    EmitAgentUpdateEvents = true,
    ForwardIncomingMessages = true
});

AIAgent hotelFinderAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a hotel finder agent. ");
var hotelFinderAgentExecutor = hotelFinderAgent.BindAsExecutor(new AIAgentHostOptions()
{
    EmitAgentResponseEvents = true,
    EmitAgentUpdateEvents = true,
    ForwardIncomingMessages = true,
     
});

AIAgent responseComposerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a response composer agent. You compose the final response based on the inputs from other agents.");
var responseComposerAgentExecutor = responseComposerAgent.BindAsExecutor(new AIAgentHostOptions()
{
    EmitAgentResponseEvents = true,
});



var workflow = new WorkflowBuilder(queryTravelPlannerAgentExecutor)
    .AddFanOutEdge(queryTravelPlannerAgentExecutor,
    [airTravelPlannerAgentExecutor, trainTravelPlannerAgentExecutor,
        busTravelPlannerAgentExecutor, hotelFinderAgentExecutor])
    .AddFanInBarrierEdge([airTravelPlannerAgentExecutor, trainTravelPlannerAgentExecutor,
        busTravelPlannerAgentExecutor, hotelFinderAgentExecutor], responseComposerAgentExecutor)
    .WithOutputFrom(responseComposerAgentExecutor)
    .Build();

var userQuery = "I want to plan a trip to London from Brussels for 5 days, in between June 10th and June 15th this year for two adults. Find me tickets.";

var streamingRun = await InProcessExecution.RunStreamingAsync(workflow, userQuery);

await foreach (WorkflowEvent evt in streamingRun.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent output)
    {
        Console.WriteLine($"Final answer: {output.Data}");
    }
}

var status = await streamingRun.GetStatusAsync();
Console.WriteLine($"Workflow run status: {status}");



var parallelWorkflow = AgentWorkflowBuilder.CreateConcurrentBuilderWith(
    [ airTravelPlannerAgent, trainTravelPlannerAgent,
        busTravelPlannerAgent, hotelFinderAgent])
   .WithName("travelChecker")
   .WithDescription("Company internal workflow for travel modes")
   //.WithIntermediateOutputFrom([ airTravelPlannerAgent, trainTravelPlannerAgent,
   //     busTravelPlannerAgent, hotelFinderAgent])
   .Build();

var workflow1 = AgentWorkflowBuilder.CreateSequentialBuilderWith(
    [queryTravelPlannerAgent, parallelWorkflow.AsAIAgent(), responseComposerAgent])
    .WithOutputFrom(responseComposerAgent)
    .Build();

var run1 = await InProcessExecution.RunAsync(workflow1, userQuery);

foreach (WorkflowEvent evt in run1.OutgoingEvents)
{
    if (evt is WorkflowOutputEvent output)
    {
        Console.Write($"{output.Data}");
    }
}
Console.WriteLine("----------------------------------------------------------");


var streamingRun1 = await InProcessExecution.RunStreamingAsync(workflow1, userQuery);

await foreach (WorkflowEvent evt in streamingRun.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent output)
    {
        Console.WriteLine($"Final answer: {output.Data}");
    }
}
