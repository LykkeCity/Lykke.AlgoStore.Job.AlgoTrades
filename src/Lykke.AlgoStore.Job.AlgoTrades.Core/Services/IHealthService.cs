using System.Collections.Generic;
using Lykke.AlgoStore.Job.AlgoTrades.Core.Domain.Health;

namespace Lykke.AlgoStore.Job.AlgoTrades.Core.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public interface IHealthService
    {
        string GetHealthViolationMessage();
        IEnumerable<HealthIssue> GetHealthIssues();

        // TODO: Place health tracing methods declarations here
    }
}