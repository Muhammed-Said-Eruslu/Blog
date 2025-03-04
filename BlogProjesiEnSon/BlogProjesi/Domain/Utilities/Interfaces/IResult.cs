using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utilities.Interfaces
{
    public interface IResult
    {
        public bool IsSucces { get; set; }
        public string Message { get; set; }
    }
}
