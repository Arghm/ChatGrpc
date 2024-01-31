using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGrpc.Client.App.Entities
{
    public class InitializeResult
    {
        public bool IsSuccess { get; set; }
        public InitializeErrorCode? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
