using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace TP.ConcurrentProgramming.Data
{
    public class DataLogger : IDataLogger
    {
        private static readonly Lazy<DataLogger> _singletonInstance = new Lazy<DataLogger>(() => new DataLogger("../../../../Data/logs/balls_logs.json"));

        private readonly ConcurrentQueue<BallLogEntry> _logQueue;
        private readonly string _logFilePath;
        private readonly Thread _loggingThread;
        private bool _isLoggingActive;
        private readonly AutoResetEvent _logSignal;
        private const int MaxQueueSize = 10000;
        private readonly SemaphoreSlim _queueLimiter;

        private DataLogger(string logFilePath)
        {
            _logSignal = new AutoResetEvent(false);
            _logFilePath = logFilePath;
            _logQueue = new ConcurrentQueue<BallLogEntry>();

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

                while (_logQueue.TryDequeue(out var entry))
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(entry);
                        File.AppendAllText(_logFilePath, json + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing log entry: {ex.Message}");
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
            if (!_isLoggingActive)
                return;

            if (_queueLimiter.Wait(0))
            {
                var logEntry = new BallLogEntry(DateTime.Now, message, threadId, position, velocity);
                _logQueue.Enqueue(logEntry);
                _logSignal.Set();
            }
            else
            {
                Debug.WriteLine("Log queue is full.");
            }
        }

        public void Stop()
        {
            if (!_isLoggingActive) return;
            
            // First, stop accepting new logs
            _isLoggingActive = false;
            
            // Signal the logging thread to process remaining logs
            _logSignal.Set();
            
            // Wait for the queue to be empty
            while (!_logQueue.IsEmpty)
            {
                Thread.Sleep(100); // Small delay to prevent busy waiting
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