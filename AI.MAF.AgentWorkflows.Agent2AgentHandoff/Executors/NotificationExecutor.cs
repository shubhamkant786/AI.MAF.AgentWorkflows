using AI.MAF.AgentWorkflows.Agent2AgentHandoff.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.MAF.AgentWorkflows.Agent2AgentHandoff.Executors
{
    public class NotificationExecutor : FunctionExecutor<string, string>
    {
        
        private readonly ExecutorOptions _executorOptions;
        public NotificationExecutor(ExecutorOptions executorOptions) : base(nameof(NotificationExecutor), SendEmailAsync)
        {            
            _executorOptions = executorOptions;
        }
        public async static ValueTask<string> SendEmailAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            EmailSenderService _sender= new EmailSenderService();
            await _sender.SendEmailAsync(message, cancellationToken);
            return message;
        }
    }

}
