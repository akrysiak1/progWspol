//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            _isRunning = true;
            _moveThread = new Thread(Move);
            _moveThread.IsBackground = true;
            _moveThread.Start();
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        private IVector _velocity;
        private IVector _position;

        public IVector Velocity 
        { 
            get 
            {
                lock (_lockObject)
                {
                    return _velocity;
                }
            }
            set 
            {
                lock (_lockObject)
                {
                    _velocity = value;
                }
            }
        }

        public IVector Position 
        { 
            get 
            {
                lock (_lockObject)
                {
                    return _position;
                }
            }
            private set 
            {
                lock (_lockObject)
                {
                    _position = value;
                }
            }
        }

        #endregion IBall

        #region private

        private readonly Thread _moveThread;
        private bool _isRunning;
        private readonly object _lockObject = new object();
        private const double BASE_REFRESH_RATE = 60.0; // Base refresh rate in Hz
        private const double MIN_SLEEP_TIME = 5; // Minimum sleep time in ms
        private const double MAX_SLEEP_TIME = 50; // Maximum sleep time in ms

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        private double CalculateSleepTime()
        {
            double speed = Math.Sqrt(Velocity.x * Velocity.x + Velocity.y * Velocity.y);
            
            // Faster balls will have shorter sleep time (higher refresh rate)
            double sleepTime = MAX_SLEEP_TIME - (speed * (MAX_SLEEP_TIME - MIN_SLEEP_TIME) / 5.0);
            // Ensure sleep time stays within bounds
            return Math.Max(MIN_SLEEP_TIME, Math.Min(MAX_SLEEP_TIME, sleepTime));
        }

        private void Move()
        {
            while (_isRunning)
            {
                IVector currentPosition;
                
                currentPosition = Position;
                Position = new Vector(currentPosition.x + Velocity.x, currentPosition.y + Velocity.y);
                
                RaiseNewPositionChangeNotification();
                Thread.Sleep((int)CalculateSleepTime());
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _moveThread.Join();
        }

        #endregion private
    }
}