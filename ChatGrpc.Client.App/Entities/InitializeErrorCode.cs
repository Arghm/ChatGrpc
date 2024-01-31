using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGrpc.Client.App.Entities
{
    /// <summary>
    /// Error codes in initialization process.
    /// </summary>
    public enum InitializeErrorCode
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// User registration error.
        /// </summary>
        UserRegister = 1,

        /// <summary>
        /// Connection to server error.
        /// </summary>
        Connection = 2,
    }
}
