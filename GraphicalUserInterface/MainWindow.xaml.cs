//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
  /// <summary>
  /// View implementation
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(BallCountTextBox.Text, out int numberOfBalls))
      {
        if (numberOfBalls > 0 && numberOfBalls <= 20)
        {
          MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
          viewModel.Start(numberOfBalls);
        }
        else
        {
          MessageBox.Show("Please enter a number between 1 and 20.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      }
      else
      {
        MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    /// <summary>
    /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnClosed(EventArgs e)
    {
      if (DataContext is MainWindowViewModel viewModel)
        viewModel.Dispose();
      base.OnClosed(e);
    }
  }
}