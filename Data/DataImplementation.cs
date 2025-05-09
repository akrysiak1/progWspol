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
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            if (numberOfBalls <= 0)
                throw new ArgumentException("Number of balls must be greater than 0", nameof(numberOfBalls));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                // Create balls with random positions within the table boundaries
                double x = random.Next(50, 350); // Keep some margin from the edges
                double y = random.Next(50, 350);
                Vector startingPosition = new(x, y);

                // Create initial velocity
                double vx = (random.NextDouble() - 0.5) * 5; // Random velocity between -2.5 and 2.5
                double vy = (random.NextDouble() - 0.5) * 5;
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
                    MoveTimer.Dispose();
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        //private bool disposedValue;
        private bool Disposed = false;
        private const double BORDER_WIDTH = 400;
        private const double BORDER_HEIGHT = 420;
        private const double BALL_RADIUS = 20; // Assuming ball diameter is 40 based on the XAML

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = [];

        private void Move(object? x)
        {
            foreach (Ball item in BallsList)
            {
                IVector currentPosition = item.Position;

                // Generate random movement
                double randomX = (RandomGenerator.NextDouble() - 0.5) * 10;
                double randomY = (RandomGenerator.NextDouble() - 0.5) * 10;

                // Calculate new position
                double newX = currentPosition.x + randomX;
                double newY = currentPosition.y + randomY;

                // Check for border collisions and adjust position if needed
                if (newX - BALL_RADIUS < 0)
                {
                    newX = BALL_RADIUS;
                }
                else if (newX + BALL_RADIUS > BORDER_WIDTH)
                {
                    newX = BORDER_WIDTH - BALL_RADIUS;
                }

                if (newY - BALL_RADIUS < 0)
                {
                    newY = BALL_RADIUS;
                }
                else if (newY + BALL_RADIUS > BORDER_HEIGHT)
                {
                    newY = BORDER_HEIGHT - BALL_RADIUS;
                }

                // Move the ball to the new position
                item.Move(new Vector(newX - currentPosition.x, newY - currentPosition.y));
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}