using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Common;
using EvoConnect.Server.Repository.Interfaces;
using FirebirdSql.Data.FirebirdClient;

namespace EvoConnect.Server.Repository
{
	public class ExecuterDA : IExecuterDA
	{

		public async Task<T> ExecuteDbOperationAsync<T>(Func<FbConnection, FbTransaction, Task<T>> operation)
		{
			FbConnection FbConnection = new(AppData.ConnectionString()?.Replace("ISO8859_1","NONE"));
			try
			{
				await FbConnection.OpenAsync();
				await using var transaction = await FbConnection.BeginTransactionAsync();
				try
				{
					var result = await operation(FbConnection, transaction);

					await transaction.CommitAsync();
					return result;
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}
			}
			catch
			{

				throw;
			}
			finally
			{
				await FbConnection.CloseAsync();

			}
		}
	}
}