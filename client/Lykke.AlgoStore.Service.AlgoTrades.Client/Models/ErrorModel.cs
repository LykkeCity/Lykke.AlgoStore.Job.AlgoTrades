using System.Collections.Generic;
using System.Net;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client.Models
{
    public class ErrorModel
    {
        public string Message { get; set; }
        public IDictionary<string, IList<string>> ModelErrors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
