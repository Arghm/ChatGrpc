using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGrpc.Client.App.Services
{
    /// <summary>
    /// Console output of incomig messages.
    /// </summary>
    public class ConsoleProcessMessageService : IIncomingMessageProcessService
    {
        /// <inheritdoc/>
        public Task ProcessIncomingMessage(string message)
        {
            PrintMessage(message);
            return Task.CompletedTask;
        }

        private void PrintMessage(string message)
        {
            if (OperatingSystem.IsWindows())
            {
                var position = Console.GetCursorPosition(); // get current cursor position
                int left = position.Left;   // left offset
                int top = position.Top;     // top offset
                // copy current message to the next line
                Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
                // set cursot position to the beginning of current line
                Console.SetCursorPosition(0, top);
                // print income message
                Console.WriteLine(message);
                // move cursor to the next line
                // user continue writing in the next line
                Console.SetCursorPosition(left, top + 1);
            }
            else 
                Console.WriteLine(message);
        }
    }
}
