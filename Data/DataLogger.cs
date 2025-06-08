using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace TP.ConcurrentProgramming.Data
{
    public class DataLogger : IDataLogger, IDisposable
    {
        private static readonly Lazy<DataLogger> _singletonInstance = new Lazy<DataLogger>(() => new DataLogger("../../../../Data/logs/balls_logs.json"));
        private bool _disposed = false;

        private readonly ConcurrentQueue<BallLogEntry> _logQueue;
        private readonly string _logFilePath;
        private readonly Thread _loggingThread;
        private bool _isLoggingActive;
        private readonly AutoResetEvent _logSignal;
        private const int MaxQueueSize = 10000;
        private readonly SemaphoreSlim _queueLimiter;
        private readonly StreamWriter _logFileWriter;

        private DataLogger(string logFilePath)
        {
            _logSignal = new AutoResetEvent(false);
            _logFilePath = logFilePath;
            _logQueue = new ConcurrentQueue<BallLogEntry>();

            // Open file once at startup
            _logFileWriter = new StreamWriter(_logFilePath, true);

            _queueLimiter = new SemaphoreSlim(MaxQueueSize);
            _isLoggingActive = true;
            _loggingThread = new Thread(ProcessLogQueue);
            _loggingThread.Start();
        }

        private void ProcessLogQueue()
        {
            while (_isLoggingActive || !_logQueue.IsEmpty)
            {
                _logSignal.WaitOne();

                while (_logQueue.TryDequeue(out BallLogEntry entry))
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(entry);
                        _logFileWriter.WriteLine(json);
                        _logFileWriter.Flush(); // Ensure data is written to disk
                    }
                    catch (Exception ex)
                    {
                        Log("Error writing log entry: " + ex.Message, Thread.CurrentThread.ManagedThreadId, new Vector(0, 0), new Vector(0, 0));
                    }
                    finally
                    {
                        _queueLimiter.Release();
                    }
                }
            }
        }

        public static DataLogger LoggerInstance
        {
            get
            {
                return _singletonInstance.Value;
            }
        }

        public void Log(string message, int threadId, IVector position, IVector velocity)
        {
            if (!_isLoggingActive || _disposed)
                return;

            if (_queueLimiter.Wait(0))
            {
                BallLogEntry logEntry = new BallLogEntry(DateTime.Now, message, threadId, position, velocity);
                _logQueue.Enqueue(logEntry);
                _logSignal.Set();
            }
            else
            {
                Log("Log queue is full", Thread.CurrentThread.ManagedThreadId, new Vector(0, 0), new Vector(0, 0));
            }
        }

        public void Stop()
        {
            if (!_isLoggingActive) return;
            
            // First, stop accepting new logs
            _isLoggingActive = false;
            
            // Signal the logging thread to process remaining logs
            _logSignal.Set();
            
            // Wait for the queue to be empty with a timeout
            int timeout = 5000; // 5 seconds timeout
            int elapsed = 0;
            while (!_logQueue.IsEmpty && elapsed < timeout)
            {
                Thread.Sleep(50); // Shorter sleep time
                elapsed += 50;
            }
            
            // Wait for the logging thread to finish
            _loggingThread.Join();
            
            // Release any remaining semaphore slots
            while (_queueLimiter.CurrentCount < MaxQueueSize)
            {
                try
                {
                    _queueLimiter.Release();
                }
                catch (SemaphoreFullException)
                {
                    break;
                }
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
                    // First stop accepting new logs and process remaining ones
                    Stop();
                    
                    // Close the file writer
                    _logFileWriter?.Dispose();
                    
                    // Dispose other managed resources
                    _logSignal?.Dispose();
                    _queueLimiter?.Dispose();
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