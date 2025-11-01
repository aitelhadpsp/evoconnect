using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvoConnect.Common;
using EvoConnect.Common.Helpers;


namespace EvoConnect.Server.Sync
{
    public class DataCollector : IHostedService, IDisposable
    {
        private readonly Synchronise _synchronise;
        private readonly DbContext appDbContext = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool loading;
        private readonly SemaphoreSlim _collectionLock = new SemaphoreSlim(1, 1);
        private Task? _executingTask;

        public DataCollector(Synchronise synchronise)
        {
            _synchronise = synchronise;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start the background task
            _executingTask = CollectData(_cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
                return;

            // Signal cancellation
            _cancellationTokenSource.Cancel();

            // Wait for the task to complete or timeout
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        public async Task CollectData(CancellationToken token)
        {
            await _collectionLock.WaitAsync(token);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var partner = await Getters.GetPartner();
                        if (partner is null || !AppData.IsServer())
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), token);
                            await Helpers.LogErrorToFileAsync(new Exception("ISNotServer"));
                            continue;
                        }

                        if (!partner.SmileEvo)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(10), token);
                            continue;
                        }

                        loading = true;
                        var syncTasks = new List<Func<Task>>
                        {
                            _synchronise.CollectDoctors,
                            _synchronise.SynchronizePatients,
                            _synchronise.SynchronizeNotes,
                            _synchronise.SyncTreatments,
                            _synchronise.SyncPayments,
                            _synchronise.CollectAppointments,
                            _synchronise.SyncImages,
                            _synchronise.CollectDeleted
                        };

                        foreach (var task in syncTasks)
                        {
                            try
                            {
                                if (!(partner is null || !AppData.IsServer()))
                                    await task();
                            }
                            catch (Exception ex) when (ex is not OperationCanceledException)
                            {
                                await Helpers.LogErrorToFileAsync(ex);
                            }
                        }
                        loading = false;

                        await Task.Delay(TimeSpan.FromMinutes(10), token);
                    }
                    catch (Exception ex)
                    {
                        if (ex is not OperationCanceledException)
                        {
                            await Helpers.LogErrorToFileAsync(ex);

                            if (!token.IsCancellationRequested)
                            {
                                await Task.Delay(TimeSpan.FromMinutes(1), token);
                            }
                        }
                        else
                        {
                            break;
                        }
                        loading = false;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            finally
            {
                _collectionLock.Release();
                loading = false;
            }
        }

        public async Task CheckSyncing()
        {
            var partner = await Getters.GetPartner();
            if (partner == null)
                return;
            DateTime? last = appDbContext.GetAppointmentSync();
            if (last is null || loading)
            {
                return;
            }
            if (last is not null)
            {
                TimeSpan difference = (TimeSpan)(DateTime.Now - last);
                if (difference.Hours > 1)
                {
                    // Handle sync check logic here
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _collectionLock?.Dispose();
        }
    }
}