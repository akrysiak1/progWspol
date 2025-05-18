//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BusinessBallUnitTest
  {
    [TestMethod]
    public void BallConstructorTest()
    {
      var dataBall = new MockDataBall();
      var ball = new Ball(dataBall);
      Assert.IsNotNull(ball);
    }

    [TestMethod]
    public void BallPositionUpdateTest()
    {
      var dataBall = new MockDataBall();
      var ball = new Ball(dataBall);
      bool positionUpdated = false;
      ball.NewPositionNotification += (sender, position) => positionUpdated = true;

      // Simulate position update
      dataBall.SimulatePositionUpdate(new Vector(150, 150));
      Assert.IsTrue(positionUpdated);
    }

    [TestMethod]
    public void BallBorderCollisionTest()
    {
      var dataBall = new MockDataBall();
      var ball = new Ball(dataBall);
      IPosition? lastPosition = null;
      ball.NewPositionNotification += (sender, position) => lastPosition = position;

      // Set initial position near border
      dataBall.Velocity = new Vector(-10, 0); // Moving left
      dataBall.SimulatePositionUpdate(new Vector(10, 100)); // Near left border

      Assert.IsNotNull(lastPosition);
      Assert.IsTrue(lastPosition.x >= Ball.GetBallRadius()); // Should not go beyond border
    }

    [TestMethod]
    public void BallCollisionTest()
    {
      var dataBall1 = new MockDataBall();
      var dataBall2 = new MockDataBall();
      var ball1 = new Ball(dataBall1);
      var ball2 = new Ball(dataBall2);

      // Position balls very close to each other to ensure collision
      double ballRadius = Ball.GetBallRadius();
      
      // Set initial positions
      dataBall1.SimulatePositionUpdate(new Vector(100, 100));
      dataBall2.SimulatePositionUpdate(new Vector(100 + 2.1 * ballRadius, 100));

      // Set velocities to make balls move towards each other
      dataBall1.Velocity = new Vector(10, 0);
      dataBall2.Velocity = new Vector(-10, 0);

      // Simulate movement that will cause collision
      dataBall1.SimulatePositionUpdate(new Vector(100 + 5, 100)); // Move ball1 right
      dataBall2.SimulatePositionUpdate(new Vector(100 + 2.1 * ballRadius - 5, 100)); // Move ball2 left

      // Verify velocities changed after collision
      Assert.AreNotEqual(10, dataBall1.Velocity.x, "Ball 1 velocity should change after collision");
      Assert.AreNotEqual(-10, dataBall2.Velocity.x, "Ball 2 velocity should change after collision");
      
      // Verify that the total momentum is conserved
      double totalMomentumX = dataBall1.Velocity.x + dataBall2.Velocity.x;
      Assert.AreEqual(0, totalMomentumX, 0.001, "Total momentum should be conserved");
    }

    private class MockDataBall : Data.IBall
    {
      public IVector Velocity { get; set; } = new Vector(0, 0);
      public IVector Position { get; private set; } = new Vector(0, 0);
      public event EventHandler<IVector>? NewPositionNotification;

      public void SimulatePositionUpdate(IVector newPosition)
      {
        Position = newPosition;
        NewPositionNotification?.Invoke(this, newPosition);
      }

      public void Stop()
      {
        // No implementation needed for mock
      }
    }

    private record Vector : IVector
    {
      public double x { get; init; }
      public double y { get; init; }

      public Vector(double x, double y)
      {
        this.x = x;
        this.y = y;
      }
    }
  }
}