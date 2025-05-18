//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public abstract class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
      BorderVisible = false;
      BorderSize = 400; // Initial size
    }

    #endregion ctor

    #region public API

    public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      ModelLayer.Start(numberOfBalls);
      Observer.Dispose();
      BorderVisible = true;
      if (!_borderSizeCalculated)
      {
        CalculateBorderSize();
        _borderSizeCalculated = true;
      }
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    private bool _borderVisible;
    public bool BorderVisible
    {
      get => _borderVisible;
      private set
      {
        if (_borderVisible != value)
        {
          _borderVisible = value;
          RaisePropertyChanged();
        }
      }
    }

    private double _borderSize;
    private bool _borderSizeCalculated = false;
    public double BorderSize
    {
      get => _borderSize;
      private set
      {
        if (_borderSize != value)
        {
          _borderSize = value;
          RaisePropertyChanged();
        }
      }
    }

    private double _windowWidth;
    private double _windowHeight;

    public void UpdateWindowSize(double width, double height)
    {
      _windowWidth = width;
      _windowHeight = height;
    }

    private void CalculateBorderSize()
    {
      // Calculate the maximum square size that fits in the window
      const double TOP_PANEL_HEIGHT = 60;  // Height of the top panel
      const double MARGIN = 40;           // General margin
      const double BORDER_THICKNESS = 8;   // Total border thickness (4px on each side)
      const double EXTRA_PADDING = 20;    // Additional padding to ensure visibility
      
      double availableHeight = _windowHeight - TOP_PANEL_HEIGHT - MARGIN - BORDER_THICKNESS - EXTRA_PADDING;
      double availableWidth = _windowWidth - MARGIN - BORDER_THICKNESS - EXTRA_PADDING;
      
      double maxSize = Math.Min(availableWidth, availableHeight);
      
      // Don't go below a minimum size
      BorderSize = Math.Max(400, maxSize);

      // Update the physical border size through the business logic layer
      ModelLayer.UpdateBorderSize(BorderSize);
    }

    #endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;

    #endregion private
  }

  public class ConcreteMainWindowViewModel : MainWindowViewModel
  {
    public ConcreteMainWindowViewModel() : base() { }
    internal ConcreteMainWindowViewModel(ModelAbstractApi modelLayerAPI) : base(modelLayerAPI) { }
  }
}