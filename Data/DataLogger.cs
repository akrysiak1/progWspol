using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    public class DataLogger : IDataLogger, IDisposable
    {
        private static readonly Lazy<DataLogger> _singletonInstance = new Lazy<DataLogger>(() => new DataLogger("../../../../Data/logs/balls_logs.json"));
        private bool _disposed = false;

        private readonly BlockingCollection<BallLogEntry> _logQueue;
        private readonly string _logFilePath;
        private readonly Task _processingTask;
        private bool _isLoggingActive;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private const int MaxQueueSize = 10000;
        private readonly StreamWriter _logFileWriter;

        private DataLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            _logQueue = new BlockingCollection<BallLogEntry>(MaxQueueSize);
            _cancellationTokenSource = new CancellationTokenSource();
            _isLoggingActive = true;

            // Open file once at startup
            _logFileWriter = new StreamWriter(_logFilePath, true);

            // Start the processing task
            _processingTask = Task.Run(ProcessLogQueueAsync, _cancellationTokenSource.Token);
        }

        private async Task ProcessLogQueueAsync()
        {
            try
            {
                while (_isLoggingActive || !_logQueue.IsCompleted)
                {
                    if (_logQueue.TryTake(out BallLogEntry entry))
                    {
                        try
                        {
                            string json = JsonSerializer.Serialize(entry);
                            await _logFileWriter.WriteLineAsync(json);
                            await _logFileWriter.FlushAsync();
                        }
                        catch (Exception ex)
                        {
                            await Log("Error writing log entry: " + ex.Message, Thread.CurrentThread.ManagedThreadId, new Vector(0, 0), new Vector(0, 0));
                        }
                    }
                    else
                    {
                        // If no items in queue, wait a bit before checking again
                        await Task.Delay(50, _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, do nothing
            }
        }

        public static DataLogger LoggerInstance
        {
            get
            {
                return _singletonInstance.Value;
            }
        }

        public async Task Log(string message, int threadId, IVector position, IVector velocity)
        {
            if (!_isLoggingActive || _disposed)
                return;

            try
            {
                BallLogEntry logEntry = new BallLogEntry(DateTime.Now, message, threadId, position, velocity);
                _logQueue.Add(logEntry);
            }
            catch (InvalidOperationException)
            {
                // Queue is full
                await Log("Log entry was not logged - buffer is full", Thread.CurrentThread.ManagedThreadId, new Vector(0, 0), new Vector(0, 0));
            }
        }

        public async Task Flush()
        {
            if (!_isLoggingActive) return;
            
            _logQueue.CompleteAdding();
            await _processingTask;
        }

        public async Task WaitForFlush()
        {
            if (!_isLoggingActive) return;
            
            _logQueue.CompleteAdding();
            await _processingTask;
        }

        public void Stop()
        {
            if (!_isLoggingActive) return;
            
            _isLoggingActive = false;
            _logQueue.CompleteAdding();
            
            try
            {
                _processingTask.Wait();
            }
            catch (AggregateException)
            {
                // Task was cancelled or failed, which is expected during shutdown
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                    _cancellationTokenSource.Cancel();
                    _logFileWriter?.Dispose();
                    _logQueue?.Dispose();
                    _cancellationTokenSource?.Dispose();
                }

                _disposed = true;
            }
        }

        ~DataLogger()
        {
            Dispose(false);
        }

        internal class BallLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Message { get; set; }
            public int ThreadId { get; set; }
            public IVector Position { get; set; }
            public IVector Velocity { get; set; }

            internal BallLogEntry(DateTime timestamp, string message, int threadId, IVector position, IVector velocity)
            {
                Timestamp = timestamp;
                Message = message;
                Position = position;
                Velocity = velocity;
                ThreadId = threadId;
            }
        }
    }
} 
