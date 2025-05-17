//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private const double BORDER_WIDTH = 400;
        private const double BORDER_HEIGHT = 420;
        private const double BALL_RADIUS = 20;
        private const double VISUAL_RADIUS = 10; // Half of the visual diameter
        private readonly object lockObject = new object();
        private readonly Data.IBall dataBall;
        private static readonly List<Ball> AllBalls = new List<Ball>();

        public Ball(Data.IBall ball)
        {
            dataBall = ball;
            ball.NewPositionNotification += RaisePositionChangeEvent;
            lock (AllBalls)
            {
                AllBalls.Add(this);
            }
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            lock (lockObject)
            {
                double newX = e.x;
                double newY = e.y;

                // Check for border collisions and adjust position if needed
                if (newX - BALL_RADIUS < 0)
                {
                    newX = BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(-dataBall.Velocity.x, dataBall.Velocity.y);
                }
                else if (newX + BALL_RADIUS > BORDER_WIDTH)
                {
                    newX = BORDER_WIDTH - BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(-dataBall.Velocity.x, dataBall.Velocity.y);
                }

                if (newY - BALL_RADIUS < 0)
                {
                    newY = BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(dataBall.Velocity.x, -dataBall.Velocity.y);
                }
                else if (newY + BALL_RADIUS > BORDER_HEIGHT)
                {
                    newY = BORDER_HEIGHT - BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(dataBall.Velocity.x, -dataBall.Velocity.y);
                }

                // Check for ball-to-ball collisions
                CheckBallCollisions(newX, newY);

                NewPositionNotification?.Invoke(this, new Position(newX, newY));
            }
        }

        private void CheckBallCollisions(double newX, double newY)
        {
            lock (AllBalls)
            {
                foreach (var otherBall in AllBalls)
                {
                    if (otherBall == this) continue;

                    double dx = newX - otherBall.dataBall.Position.x;
                    double dy = newY - otherBall.dataBall.Position.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < 2 * VISUAL_RADIUS) // Use visual radius for ball-to-ball collisions
                    {
                        // Calculate collision normal
                        double nx = dx / distance;
                        double ny = dy / distance;

                        // Calculate relative velocity
                        double dvx = dataBall.Velocity.x - otherBall.dataBall.Velocity.x;
                        double dvy = dataBall.Velocity.y - otherBall.dataBall.Velocity.y;

                        // Calculate relative velocity along normal
                        double velocityAlongNormal = dvx * nx + dvy * ny;

                        // Don't resolve if balls are moving apart
                        if (velocityAlongNormal > 0) continue;

                        // Calculate impulse scalar
                        double j = -(1 + 1) * velocityAlongNormal; // 1 is the coefficient of restitution (perfectly elastic)
                        j /= 2; // Since both balls have equal mass

                        // Apply impulse
                        dataBall.Velocity = new Data.Vector(
                          dataBall.Velocity.x + j * nx,
                          dataBall.Velocity.y + j * ny
                        );

                        otherBall.dataBall.Velocity = new Data.Vector(
                          otherBall.dataBall.Velocity.x - j * nx,
                          otherBall.dataBall.Velocity.y - j * ny
                        );

                        // Move balls apart to prevent sticking
                        double overlap = 2 * VISUAL_RADIUS - distance;
                        double moveX = overlap * nx / 2;
                        double moveY = overlap * ny / 2;

                        newX += moveX;
                        newY += moveY;
                    }
                }
            }
        }

        #endregion private
    }
}