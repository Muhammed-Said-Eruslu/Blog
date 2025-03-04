using Domain.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utilities.Concretes
{
    public class ErrorDataResult<T> : DataResult<T> 
    {
        public ErrorDataResult(): base(default,false)
        {
            
        }
        public ErrorDataResult(string messages): base(default,false,messages)
        {
            
        }
        public ErrorDataResult(T data, string messages) : base(data, false,messages)
        {
        }
    }
}

