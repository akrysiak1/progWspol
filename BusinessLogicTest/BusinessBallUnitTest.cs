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
    [TestInitialize]
    public void Initialize()
    {
      // Set default border size for tests
      Ball.SetBorderSize(400);
    }

    [TestMethod]
    public void BallConstructorTest()
    {
      MockDataBall dataBall = new MockDataBall();
      Ball ball = new Ball(dataBall);
      Assert.IsNotNull(ball);
      Assert.AreEqual(Ball.GetBallRadius(), 400 * 0.025); // Check if radius is calculated correctly
    }

    [TestMethod]
    public void BallPositionUpdateTest()
    {
      MockDataBall dataBall = new MockDataBall();
      Ball ball = new Ball(dataBall);
      bool positionUpdated = false;
      IPosition? lastPosition = null;
      ball.NewPositionNotification += (sender, position) => 
      {
        positionUpdated = true;
        lastPosition = position;
      };

      // Simulate position update
      dataBall.SimulatePositionUpdate(new Vector(150, 150));
      Assert.IsTrue(positionUpdated);
      Assert.IsNotNull(lastPosition);
      Assert.AreEqual(150, lastPosition.x);
      Assert.AreEqual(150, lastPosition.y);
    }

    [TestMethod]
    public void BallBorderCollisionTest()
    {
      MockDataBall dataBall = new MockDataBall();
      Ball ball = new Ball(dataBall);
      IPosition? lastPosition = null;
      ball.NewPositionNotification += (sender, position) => lastPosition = position;

      // Test left border collision
      dataBall.Velocity = new Vector(-10, 0);
      dataBall.SimulatePositionUpdate(new Vector(5, 100));
      Assert.IsNotNull(lastPosition);
      Assert.IsTrue(lastPosition.x >= Ball.GetBallRadius());
      Assert.IsTrue(dataBall.Velocity.x > 0); // Velocity should be reversed

      // Test right border collision
      dataBall.Velocity = new Vector(10, 0);
      dataBall.SimulatePositionUpdate(new Vector(395, 100));
      Assert.IsTrue(lastPosition.x <= 400 - Ball.GetBallRadius());
      Assert.IsTrue(dataBall.Velocity.x < 0); // Velocity should be reversed
    }

    [TestMethod]
    public void BallCollisionTest()
    {
      MockDataBall dataBall1 = new MockDataBall();
      MockDataBall dataBall2 = new MockDataBall();
      Ball ball1 = new Ball(dataBall1);
      Ball ball2 = new Ball(dataBall2);

      double ballRadius = Ball.GetBallRadius();
      
      // Set initial positions
      dataBall1.SimulatePositionUpdate(new Vector(100, 100));
      dataBall2.SimulatePositionUpdate(new Vector(100 + 2.1 * ballRadius, 100));

      // Set velocities to make balls move towards each other
      dataBall1.Velocity = new Vector(10, 0);
      dataBall2.Velocity = new Vector(-10, 0);

      // Calculate initial energy
      double initialEnergy = dataBall1.Velocity.x * dataBall1.Velocity.x + 
                           dataBall2.Velocity.x * dataBall2.Velocity.x;

      // Simulate movement that will cause collision
      dataBall1.SimulatePositionUpdate(new Vector(100 + 5, 100));
      dataBall2.SimulatePositionUpdate(new Vector(100 + 2.1 * ballRadius - 5, 100));

      // Calculate final energy
      double finalEnergy = dataBall1.Velocity.x * dataBall1.Velocity.x + 
                          dataBall2.Velocity.x * dataBall2.Velocity.x;

      // Verify velocities changed after collision
      Assert.AreNotEqual(10, dataBall1.Velocity.x, "Ball 1 velocity should change after collision");
      Assert.AreNotEqual(-10, dataBall2.Velocity.x, "Ball 2 velocity should change after collision");
      
      // Verify that the total momentum is conserved
      double totalMomentumX = dataBall1.Velocity.x + dataBall2.Velocity.x;
      Assert.AreEqual(0, totalMomentumX, 0.001, "Total momentum should be conserved");

      // Verify energy conservation (allowing for small numerical errors)
      Assert.AreEqual(initialEnergy, finalEnergy, 0.001, "Energy should be conserved");
    }

    [TestMethod]
    public void BorderSizeChangeTest()
    {
      // Test changing border size
      Ball.SetBorderSize(500);
      Assert.AreEqual(500 * 0.025, Ball.GetBallRadius(), "Ball radius should update with border size");

      // Test ball behavior with new border size
      MockDataBall dataBall = new MockDataBall();
      Ball ball = new Ball(dataBall);
      IPosition? lastPosition = null;
      ball.NewPositionNotification += (sender, position) => lastPosition = position;

      // Test right border collision with new size
      dataBall.Velocity = new Vector(10, 0);
      dataBall.SimulatePositionUpdate(new Vector(495, 100));
      Assert.IsTrue(lastPosition.x <= 500 - Ball.GetBallRadius());
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