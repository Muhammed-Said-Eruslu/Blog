using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utilities.Concretes
{
    public class Result : IResult
    {
        public bool IsSucces { get; set; }
        public string Message { get; set; }

        public Result()
        {
            IsSucces = false;
            Message = string.Empty;
        }
        public Result(bool IsSucceed)
        {
            IsSucces = IsSucceed;
        }
        public Result(bool IsSucceed, string message) : this (IsSucceed)
        {
            Message = message;
        }
    }
}
