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
        private const double BALL_RADIUS = 10;
        private const double VISUAL_RADIUS = 10; // Used for drawing
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

                const double EPSILON = 0.0001;

                // Border collision detection (with velocity check to avoid sticking)
                if (newX - BALL_RADIUS < 0 + EPSILON && dataBall.Velocity.x < 0)
                {
                    newX = BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(-dataBall.Velocity.x, dataBall.Velocity.y);
                }
                else if (newX + BALL_RADIUS > BORDER_WIDTH - EPSILON && dataBall.Velocity.x > 0)
                {
                    newX = BORDER_WIDTH - BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(-dataBall.Velocity.x, dataBall.Velocity.y);
                }

                if (newY - BALL_RADIUS < 0 + EPSILON && dataBall.Velocity.y < 0)
                {
                    newY = BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(dataBall.Velocity.x, -dataBall.Velocity.y);
                }
                else if (newY + BALL_RADIUS > BORDER_HEIGHT - EPSILON && dataBall.Velocity.y > 0)
                {
                    newY = BORDER_HEIGHT - BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(dataBall.Velocity.x, -dataBall.Velocity.y);
                }

                // Ball-to-ball collisions
                CheckBallCollisions(ref newX, ref newY);

                // Notify view about new position
                NewPositionNotification?.Invoke(this, new Position(newX, newY));
            }
        }

        private void CheckBallCollisions(ref double newX, ref double newY)
        {
            lock (AllBalls)
            {
                foreach (var otherBall in AllBalls)
                {
                    if (otherBall == this) continue;

                    double dx = newX - otherBall.dataBall.Position.x;
                    double dy = newY - otherBall.dataBall.Position.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < 2 * VISUAL_RADIUS && distance > 0) // Avoid division by 0
                    {
                        // Normalize vector
                        double nx = dx / distance;
                        double ny = dy / distance;

                        // Relative velocity
                        double dvx = dataBall.Velocity.x - otherBall.dataBall.Velocity.x;
                        double dvy = dataBall.Velocity.y - otherBall.dataBall.Velocity.y;

                        // Relative velocity along normal
                        double velocityAlongNormal = dvx * nx + dvy * ny;

                        // Ignore if balls moving away
                        if (velocityAlongNormal > 0) continue;

                        // Impulse scalar (elastic collision)
                        double j = -(1 + 1) * velocityAlongNormal / 2;

                        // Apply impulse
                        dataBall.Velocity = new Data.Vector(
                            dataBall.Velocity.x + j * nx,
                            dataBall.Velocity.y + j * ny
                        );

                        otherBall.dataBall.Velocity = new Data.Vector(
                            otherBall.dataBall.Velocity.x - j * nx,
                            otherBall.dataBall.Velocity.y - j * ny
                        );

                        // Separate balls slightly to avoid overlap
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

