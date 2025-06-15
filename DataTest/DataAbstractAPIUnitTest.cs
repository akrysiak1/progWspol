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
using System;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class DataAbstractAPIUnitTest
  {
    [TestMethod]
    public void ConstructorTestTestMethod()
    {
      DataAbstractAPI instance1 = DataAbstractAPI.GetDataLayer();
      DataAbstractAPI instance2 = DataAbstractAPI.GetDataLayer();
      Assert.AreSame<DataAbstractAPI>(instance1, instance2);
      
      // Check initial state
      bool isDisposed = false;
      ((DataImplementation)instance1).CheckObjectDisposed(x => isDisposed = x);
      Assert.IsFalse(isDisposed);

      // First dispose
      instance1.Dispose();
      ((DataImplementation)instance1).CheckObjectDisposed(x => isDisposed = x);
      Assert.IsTrue(isDisposed);

      // Second dispose should not throw
      instance2.Dispose();
      ((DataImplementation)instance2).CheckObjectDisposed(x => isDisposed = x);
      Assert.IsTrue(isDisposed);
    }
  }
}