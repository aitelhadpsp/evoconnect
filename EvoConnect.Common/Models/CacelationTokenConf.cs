using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class CancelationConf
    {
        public CancellationTokenSource Token { get; set; } = new CancellationTokenSource();

        public void Reset()
        {
            Token.Cancel();
            Token = new CancellationTokenSource();
        }
    }
}