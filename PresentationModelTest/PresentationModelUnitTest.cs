//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
  [TestClass]
  public class PresentationModelUnitTest
  {
    [TestMethod]
    public void DisposeTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      ModelImplementation? newInstance = null;
      using (newInstance = new(underneathLayerFixture))
      {
        newInstance.CheckObjectDisposed(x => Assert.IsFalse(x));
        newInstance.CheckUnderneathLayerAPI(x => Assert.AreSame(underneathLayerFixture, x));
        newInstance.CheckBallChangedEvent(x => Assert.IsTrue(x));
        Assert.IsFalse(underneathLayerFixture.Disposed);
      }
      newInstance.CheckObjectDisposed(x => Assert.IsTrue(x));
      newInstance.CheckUnderneathLayerAPI(x => Assert.AreSame(underneathLayerFixture, x));
      Assert.IsTrue(underneathLayerFixture.Disposed);
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
    }

    [TestMethod]
    public void StartTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      using (ModelImplementation newInstance = new(underneathLayerFixture))
      {
        newInstance.CheckBallChangedEvent(x => Assert.IsTrue(x));
        IDisposable subscription = newInstance.Subscribe(x => { });
        newInstance.CheckBallChangedEvent(x => Assert.IsFalse(x));
        newInstance.Start(10);
        Assert.AreEqual<int>(10, underneathLayerFixture.NumberOfBalls);
        subscription.Dispose();
        newInstance.CheckBallChangedEvent(x => Assert.IsTrue(x));
      }
    }

    [TestMethod]
    public void UpdateBorderSizeTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      using (ModelImplementation newInstance = new(underneathLayerFixture))
      {
        double newSize = 500;
        newInstance.UpdateBorderSize(newSize);
        Assert.AreEqual(newSize, underneathLayerFixture.LastBorderSize);
      }
    }

    [TestMethod]
    public void BallCreationTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      using (ModelImplementation newInstance = new(underneathLayerFixture))
      {
        bool ballCreated = false;
        IBall? createdBall = null;
        newInstance.BallChanged += (sender, args) =>
        {
          ballCreated = true;
          createdBall = args.Ball;
        };

        // Start the model to create a ball
        newInstance.Start(1);
        Thread.Sleep(100); // Give time for the event to be processed

        Assert.IsTrue(ballCreated, "Ball creation event was not raised");
        Assert.IsNotNull(createdBall, "Created ball is null");
      }
    }

    [TestMethod]
    public void SubscriptionTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      using (ModelImplementation newInstance = new(underneathLayerFixture))
      {
        int notificationCount = 0;
        IDisposable subscription = newInstance.Subscribe(ball => notificationCount++);

        // Start the model to create balls
        newInstance.Start(2);
        Thread.Sleep(100); // Give time for events to be processed

        Assert.AreEqual(2, notificationCount, "Expected 2 notifications but got " + notificationCount);
        subscription.Dispose();
      }
    }

    #region testing instrumentation

    private class UnderneathLayerFixture : BusinessLogicAbstractAPI
    {
      #region testing instrumentation

      internal bool Disposed = false;
      internal int NumberOfBalls = 0;
      internal double LastBorderSize = 0;
      internal Action<BusinessLogic.IPosition, BusinessLogic.IBall>? BallCreationHandler;

      #endregion testing instrumentation

      #region BusinessLogicAbstractAPI

      public override void Dispose()
      {
        Disposed = true;
      }

      public override void Start(int numberOfBalls, Action<IPosition, BusinessLogic.IBall> upperLayerHandler)
      {
        NumberOfBalls = numberOfBalls;
        BallCreationHandler = upperLayerHandler;
        Assert.IsNotNull(upperLayerHandler);

        // Simulate ball creation
        for (int i = 0; i < numberOfBalls; i++)
        {
          MockBall mockBall = new MockBall();
          MockPosition position = new MockPosition(100, 100);
          upperLayerHandler(position, mockBall);
        }
      }

      public override void UpdateBorderSize(double size)
      {
        LastBorderSize = size;
      }

      #endregion BusinessLogicAbstractAPI

      private class MockBall : BusinessLogic.IBall
      {
        public event EventHandler<BusinessLogic.IPosition>? NewPositionNotification;
      }
    }

    private class MockPosition : BusinessLogic.IPosition
    {
      public double x { get; init; }
      public double y { get; init; }

      public MockPosition(double x, double y)
      {
        this.x = x;
        this.y = y;
      }
    }

    #endregion testing instrumentation
  }
}