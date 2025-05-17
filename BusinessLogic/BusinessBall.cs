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
    private readonly object lockObject = new object();
    private readonly Data.IBall dataBall;

    public Ball(Data.IBall ball)
    {
      dataBall = ball;
      ball.NewPositionNotification += RaisePositionChangeEvent;
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

        NewPositionNotification?.Invoke(this, new Position(newX, newY));
      }
    }

    #endregion private
  }
}