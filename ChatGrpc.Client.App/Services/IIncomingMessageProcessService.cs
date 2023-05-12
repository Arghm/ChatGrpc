using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGrpc.Client.App.Services
{
    /// <summary>
    /// Service for processing (print/log) incoming messages.
    /// </summary>
    public interface IIncomingMessageProcessService
    {
        /// <summary>
        /// Process incoming message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task ProcessIncomingMessage(string message);
    }
}
