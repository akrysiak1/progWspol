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

    public Ball(Data.IBall ball)
    {
      ball.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      double newX = e.x;
      double newY = e.y;

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

      NewPositionNotification?.Invoke(this, new Position(newX, newY));
    }

    #endregion private
  }
}