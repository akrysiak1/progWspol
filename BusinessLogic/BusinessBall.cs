//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private static double BORDER_WIDTH = 400;
        private static double BORDER_HEIGHT = 420;
        private const double BALL_RADIUS_RATIO = 0.025; // Ball radius will be 2.5% of border size
        private static double BALL_RADIUS => BORDER_WIDTH * BALL_RADIUS_RATIO;
        private static double VISUAL_RADIUS => BALL_RADIUS; // Used for drawing
        private readonly object lockObject = new object();
        private readonly Data.IBall dataBall;
        private static readonly List<Ball> AllBalls = new List<Ball>();
        private readonly IDataLogger logger;

        public static void SetBorderSize(double size)
        {
            BORDER_WIDTH = size;
            BORDER_HEIGHT = size;
        }

        public static double GetBallRadius()
        {
            return BALL_RADIUS;
        }

        public Ball(Data.IBall ball)
        {
            dataBall = ball;
            ball.NewPositionNotification += RaisePositionChangeEvent;
            logger = IDataLogger.CreateDefault();
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
                Data.IVector position = e;
                double newX = position.x;
                double newY = position.y;

                const double EPSILON = 0.0001;

                // Border collision detection (with velocity check to avoid sticking)
                if (newX - BALL_RADIUS < 0 + EPSILON && dataBall.Velocity.x < 0)
                {
                    newX = BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(-dataBall.Velocity.x, dataBall.Velocity.y);
                    logger.Log(LogEventType.LeftBorderCollision, Thread.CurrentThread.ManagedThreadId, dataBall.Position, dataBall.Velocity);
                }
                else if (newX + BALL_RADIUS > BORDER_WIDTH - EPSILON && dataBall.Velocity.x > 0)
                {
                    newX = BORDER_WIDTH - BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(-dataBall.Velocity.x, dataBall.Velocity.y);
                    logger.Log(LogEventType.RightBorderCollision, Thread.CurrentThread.ManagedThreadId, dataBall.Position, dataBall.Velocity);
                }

                if (newY - BALL_RADIUS < 0 + EPSILON && dataBall.Velocity.y < 0)
                {
                    newY = BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(dataBall.Velocity.x, -dataBall.Velocity.y);
                    logger.Log(LogEventType.TopBorderCollision, Thread.CurrentThread.ManagedThreadId, dataBall.Position, dataBall.Velocity);
                }
                else if (newY + BALL_RADIUS > BORDER_HEIGHT - EPSILON && dataBall.Velocity.y > 0)
                {
                    newY = BORDER_HEIGHT - BALL_RADIUS;
                    dataBall.Velocity = new Data.Vector(dataBall.Velocity.x, -dataBall.Velocity.y);
                    logger.Log(LogEventType.BottomBorderCollision, Thread.CurrentThread.ManagedThreadId, dataBall.Position, dataBall.Velocity);
                }

                // Ball-to-ball collisions
                CheckBallCollisions(ref newX, ref newY);

                // Notify view about new position
                NewPositionNotification?.Invoke(this, new Position(newX, newY));
            
        }

        private void CheckBallCollisions(ref double newX, ref double newY)
        {
            lock (AllBalls)
            {
                foreach (Ball otherBall in AllBalls)
                {
                    if (otherBall == this) continue;

                    Data.IVector otherPosition = otherBall.dataBall.Position;
                    double dx = newX - otherPosition.x;
                    double dy = newY - otherPosition.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < 2 * VISUAL_RADIUS && distance > 0) // Avoid division by 0
                    {
                        // Normalize vector
                        double nx = dx / distance;
                        double ny = dy / distance;

                        // Calculate relative velocity
                        double dvx = dataBall.Velocity.x - otherBall.dataBall.Velocity.x;
                        double dvy = dataBall.Velocity.y - otherBall.dataBall.Velocity.y;

                        // Calculate relative velocity along normal
                        double velocityAlongNormal = dvx * nx + dvy * ny;

                        // Ignore if balls moving away
                        if (velocityAlongNormal > 0) continue;

                        // Calculate impulse with energy conservation
                        double restitution = 1.0; // Perfectly elastic collision
                        double j = -(1 + restitution) * velocityAlongNormal;

                        // Calculate new velocities while preserving total momentum and energy
                        double totalMass = 2.0; // Assuming equal masses
                        double impulseX = j * nx / totalMass;
                        double impulseY = j * ny / totalMass;

                        // Apply impulse with energy conservation
                        double newVx1 = dataBall.Velocity.x + impulseX;
                        double newVy1 = dataBall.Velocity.y + impulseY;
                        double newVx2 = otherBall.dataBall.Velocity.x - impulseX;
                        double newVy2 = otherBall.dataBall.Velocity.y - impulseY;

                        // Calculate speed before collision
                        double speed1Before = Math.Sqrt(dataBall.Velocity.x * dataBall.Velocity.x + 
                                                      dataBall.Velocity.y * dataBall.Velocity.y);
                        double speed2Before = Math.Sqrt(otherBall.dataBall.Velocity.x * otherBall.dataBall.Velocity.x + 
                                                      otherBall.dataBall.Velocity.y * otherBall.dataBall.Velocity.y);

                        // Calculate speed after collision
                        double speed1After = Math.Sqrt(newVx1 * newVx1 + newVy1 * newVy1);
                        double speed2After = Math.Sqrt(newVx2 * newVx2 + newVy2 * newVy2);

                        // Calculate total energy before and after
                        double energyBefore = speed1Before * speed1Before + speed2Before * speed2Before;
                        double energyAfter = speed1After * speed1After + speed2After * speed2After;

                        // If energy increased, scale down the velocities
                        if (energyAfter > energyBefore)
                        {
                            double scale = Math.Sqrt(energyBefore / energyAfter);
                            newVx1 *= scale;
                            newVy1 *= scale;
                            newVx2 *= scale;
                            newVy2 *= scale;
                        }

                        // Apply the new velocities
                        dataBall.Velocity = new Data.Vector(newVx1, newVy1);
                        otherBall.dataBall.Velocity = new Data.Vector(newVx2, newVy2);

                        // Log the collision
                        logger.Log(LogEventType.BallCollision, Thread.CurrentThread.ManagedThreadId, dataBall.Position, dataBall.Velocity);

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

