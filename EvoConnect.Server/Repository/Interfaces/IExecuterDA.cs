using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IExecuterDA 
    {
        public Task<T> ExecuteDbOperationAsync<T>(Func<FbConnection,FbTransaction, Task<T>> operation);
        
    }
}