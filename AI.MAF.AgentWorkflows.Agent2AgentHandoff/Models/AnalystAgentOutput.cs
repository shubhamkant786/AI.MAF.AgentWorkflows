using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.Agent2AgentHandoff.Models
{
    public class AnalystAgentOutput
    {
        public string Id { get; set; }

        public string UserStoryDefinition { get; set; }

        public string BusinessRequestId { get; set; }

        public string OriginalBusinessRequest { get; set; }

        public List<string> AcceptanceCriterias { get; set; } = new List<string>();
    }
}
