﻿namespace ChatGrpc.Server.Host.Entities
{
    public class RegisterResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public string Token { get; set; }
    }
}
