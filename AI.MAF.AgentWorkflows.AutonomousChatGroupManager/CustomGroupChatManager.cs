using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.AutonomousChatGroupManager
{
    public class CustomGroupChatManager : GroupChatManager
    {
        private IReadOnlyList<AIAgent> Agents { get; set; }
        public CustomGroupChatManager(IReadOnlyList<AIAgent> agents)
        {
            Agents = agents;
        }
        protected override ValueTask<AIAgent> SelectNextAgentAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
        {
            if (IterationCount == MaximumIterationCount - 1)
            {
                //select most senior agent to provide final output.
            }
            //you can provide your own logic to select next agent from the list of agents.
            if (history.Select(m => m.WithAgentRequestMessageSource(AgentRequestMessageSourceType.AIContextProvider)).Any())
            {
                return ValueTask.FromResult(Agents[0]);
            }
            var lastMessageAgent = history.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            if (lastMessageAgent != null)
            {
                var lastAgent = Agents.FirstOrDefault(a => a.Id == lastMessageAgent.AuthorName);
                var agentsArray = Agents.ToArray();
                for (int i = 0; i < agentsArray.Length; i++)
                {
                    if (agentsArray[i].Id == lastMessageAgent.AuthorName)
                    {
                        var nextAgentIndex = (i + 1) % agentsArray.Length;
                        return ValueTask.FromResult(agentsArray[nextAgentIndex]);
                    }
                }
            }
            return ValueTask.FromResult(Agents[0]);
        }

        protected internal virtual ValueTask<bool> ShouldTerminateAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken = default)
        {
            //you can provide your own logic to determine if the chat should continue or not.
            return ValueTask.FromResult(this.IterationCount <= MaximumIterationCount);
        }
    }
}
