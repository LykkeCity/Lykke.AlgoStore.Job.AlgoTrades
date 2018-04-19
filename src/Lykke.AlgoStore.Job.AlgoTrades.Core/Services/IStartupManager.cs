using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.AlgoTrades.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}