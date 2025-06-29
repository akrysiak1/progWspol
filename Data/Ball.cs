﻿//____________________________________________________________________________________________________________________________________
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
using System.Timers;
using System.Threading.Tasks;

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
            
            // Create and configure timer
            _moveTimer = new System.Timers.Timer();
            UpdateTimerInterval(); // Set initial interval based on velocity
            _moveTimer.Elapsed += Move;
            _moveTimer.AutoReset = true;
            _moveTimer.Start();
            
            // Log ball creation
            IDataLogger.CreateDefault().Log(LogEventType.BallCreated, Thread.CurrentThread.ManagedThreadId, _position, _velocity);
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
                    UpdateTimerInterval(); // Update timer interval when velocity changes
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

        private readonly System.Timers.Timer _moveTimer;
        private bool _isRunning;
        private readonly object _lockObject = new object();
        private DateTime _lastUpdateTime = DateTime.Now;

        // Constants for refresh rate calculation
        private const double MIN_INTERVAL = 8.0;  // Minimum interval in ms (120 FPS)
        private const double MAX_INTERVAL = 20.0;  // Maximum interval in ms (50 FPS)

        private void UpdateTimerInterval()
        {
            if (!_isRunning) return;

            double speed = Math.Sqrt(_velocity.x * _velocity.x + _velocity.y * _velocity.y);
            
            // Calculate interval based on speed
            // Faster speed = shorter interval (higher refresh rate)
            double interval = MAX_INTERVAL - (speed * (MAX_INTERVAL - MIN_INTERVAL) / 5);
            
            // Ensure interval stays within bounds
            interval = Math.Max(MIN_INTERVAL, Math.Min(MAX_INTERVAL, interval));
            
            // Update timer interval
            _moveTimer.Interval = interval;
        }

        private void Move(object sender, ElapsedEventArgs e)
        {
            if (!_isRunning) return;

            DateTime currentTime = DateTime.Now;
            double deltaTime = (currentTime - _lastUpdateTime).TotalMilliseconds / 1000.0; // Convert to seconds
            _lastUpdateTime = currentTime;

            lock (_lockObject)
            {
                IVector currentPosition = Position;
                // Scale up velocity by multiplying by 100 to make movement more noticeable
                Position = new Vector(
                    currentPosition.x + Velocity.x * deltaTime * 100,
                    currentPosition.y + Velocity.y * deltaTime * 100
                );
            }

            RaiseNewPositionChangeNotification();
        }

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        public void Stop()
        {
            _isRunning = false;
            _moveTimer.Stop();
            _moveTimer.Dispose();
            // Log ball stop
            IDataLogger.CreateDefault().Log(LogEventType.BallStopped, Thread.CurrentThread.ManagedThreadId, Position, Velocity).ConfigureAwait(false);
        }

        #endregion private
    }
}