//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testinVector = new Vector(0.0, 0.0);
      Ball newInstance = new(testinVector, testinVector);
      Assert.IsNotNull(newInstance);
      Assert.AreEqual(testinVector, newInstance.Position);
      Assert.AreEqual(testinVector, newInstance.Velocity);
    }

    [TestMethod]
    public void MoveTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Vector initialVelocity = new(1.0, 1.0);
      Ball newInstance = new(initialPosition, initialVelocity);
      IVector currentPosition = initialPosition;
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); currentPosition = position; numberOfCallBackCalled++; };
      
      // Wait a bit for the background thread to move the ball
      Thread.Sleep(100);
      
      Assert.IsTrue(numberOfCallBackCalled > 0);
      Assert.AreNotEqual(initialPosition, currentPosition);
      
      // Clean up
      newInstance.Stop();
    }

    [TestMethod]
    public void VelocityChangeTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Vector initialVelocity = new(1.0, 1.0);
      Vector newVelocity = new(2.0, 2.0);
      Ball newInstance = new(initialPosition, initialVelocity);
      
      // Change velocity
      newInstance.Velocity = newVelocity;
      Assert.AreEqual(newVelocity, newInstance.Velocity);
      
      // Clean up
      newInstance.Stop();
    }

    [TestMethod]
    public void StopTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Vector initialVelocity = new(1.0, 1.0);
      Ball newInstance = new(initialPosition, initialVelocity);
      
      // Get initial position
      IVector positionBeforeStop = newInstance.Position;
      
      // Stop the ball
      newInstance.Stop();
      
      // Wait a bit
      Thread.Sleep(100);
      
      // Position should not change after stop
      Assert.AreEqual(positionBeforeStop, newInstance.Position);
    }
  }
}