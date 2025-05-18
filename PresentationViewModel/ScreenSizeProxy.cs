using System.ComponentModel;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class ScreenSizeProxy : INotifyPropertyChanged
    {
        private double _width;
        private double _height;

        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Width)));
                }
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}