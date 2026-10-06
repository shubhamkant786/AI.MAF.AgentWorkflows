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
        , instructions: "You are a travel query classifier analysing agent.");
var queryTravelPlannerAgentExecutor = queryTravelPlannerAgent.BindAsExecutor();

AIAgent queryItineraryReviewerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a travel itinerary reviewer and analysing agent.");
var queryItineraryReviewerAgentExecutor = queryItineraryReviewerAgent.BindAsExecutor();

AIAgent queryItineraryComposerAgent = chatClient
        .AsAIAgent(name: "shubham-sample-agent"
        , instructions: "You are a travel itinerary composer agent.");
var queryItineraryComposerAgentExecutor = queryItineraryComposerAgent.BindAsExecutor();

var workflow = new WorkflowBuilder(queryTravelPlannerAgentExecutor)
    .AddChain(queryTravelPlannerAgentExecutor,
    [queryItineraryReviewerAgentExecutor, queryItineraryComposerAgentExecutor])
    .Build();

var userQuery = "I want to plan a trip to Paris for 5 days, including sightseeing and local cuisine experiences.";
var run = await InProcessExecution.RunAsync(workflow, userQuery);

Console.WriteLine("User query: " + userQuery);
foreach (WorkflowEvent evt in run.OutgoingEvents)
{
    if (evt is WorkflowOutputEvent output)
    {
        Console.Write($"{output.Data}");        
    }
}
Console.WriteLine("----------------------------------------------------------");
await run.DisposeAsync();

var streamingRun = await InProcessExecution.RunStreamingAsync(workflow, userQuery);
await foreach (WorkflowEvent evt in streamingRun.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent output)
    {
        Console.WriteLine($"Final answer: {output.Data}");
    }
}
Console.WriteLine("----------------------------------------------------------");

var workflow1 = AgentWorkflowBuilder.CreateSequentialBuilderWith(
    [queryTravelPlannerAgent, queryItineraryReviewerAgent, queryItineraryComposerAgent])
    .WithName("travelAdvisor")
    .WithDescription("Company internal workflow for travel planning")
    .WithChainOnlyAgentResponses(true)
    .WithIntermediateOutputFrom([queryTravelPlannerAgent])
    .WithOutputFrom(queryItineraryComposerAgent)
    .Build();
var result = await InProcessExecution.RunAsync(workflow1, chatClient, userQuery);

var status = await result.GetStatusAsync();
while (status != RunStatus.Ended)
{
    Console.WriteLine($"Workflow status: {status}. Waiting for completion...");
    await Task.Delay(5000); // Wait for 5 seconds before checking again
    status = await result.GetStatusAsync();
}
foreach (var message in result.NewEvents)
{
    Console.WriteLine($"New Events Response: {message.Data}");
}
foreach (var message in result.OutgoingEvents)
{
    Console.WriteLine($"Outgoing Events Response: {message.Data}");
}
Console.WriteLine("----------------------------------------------------------");
