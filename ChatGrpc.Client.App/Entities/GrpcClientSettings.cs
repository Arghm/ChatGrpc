using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGrpcClient.Entities
{
    public class GrpcClientSettings
    {
        public string GrpcChannel { get; set; }
        public int ConnectionRetryTimeSeconds { get; set; }
    }
}
