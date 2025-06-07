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
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                return; // Return silently instead of throwing
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            if (numberOfBalls <= 0)
                throw new ArgumentException("Number of balls must be greater than 0", nameof(numberOfBalls));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                // Create balls with random positions within the table boundaries
                double x = random.Next(50, 350); 
                double y = random.Next(50, 350);
                Vector startingPosition = new(x, y);

                // Create initial velocity with more variation
                double speedMultiplier = random.NextDouble() < 0.3 ? 2 :
                                       random.NextDouble() < 0.5 ? 1.8 :
                                       1.5;
                
                double angle = random.NextDouble() * 2 * Math.PI;
                double vx = Math.Cos(angle) * speedMultiplier;
                double vy = Math.Sin(angle) * speedMultiplier;
                Vector initialVelocity = new(vx, vy);

                Ball newBall = new(startingPosition, initialVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    try
                    {
                        foreach (var ball in BallsList)
                        {
                            if (ball is Ball concreteBall)
                            {
                                concreteBall.Stop();
                            }
                        }
                        BallsList.Clear();

                        // Stop the logger
                        var logger = DataLogger.LoggerInstance;
                        if (logger != null)
                        {
                            logger.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error during disposal: {ex.Message}");
                    }
                }
                Disposed = true;
            }
        }

        public override void Dispose()
        {
            if (!Disposed)
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private readonly List<IBall> BallsList = new List<IBall>();
        private readonly object _disposeLock = new object();

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            if (!Disposed)
            {
                returnBallsList(BallsList);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            if (!Disposed)
            {
                returnNumberOfBalls(BallsList.Count);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}