using System;
using System.ComponentModel;
using System.Diagnostics;
using LifxLib;

namespace JenkinsStatusLifx.ViewModels
{
    public class BulbAdapter : BulbAdapterBase, IDisposable
    {
        private readonly LifxBulb _bulb;

        public BulbAdapter(LifxBulb bulb)
            : base(bulb)
        {
            _bulb = bulb;

            UpdateLabel();

            _bulb.PropertyChanged += OnBulbPropertyChanged;
        }

        public override bool IsBulbGroup { get { return false; } }

        public LifxBulb Source { get { return _bulb; } }

        protected bool Equals(BulbAdapter other)
        {
            return _bulb.Equals(other._bulb);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }
            return Equals((BulbAdapter)obj);
        }

        public override int GetHashCode()
        {
            return _bulb.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose)
        {
            if (dispose)
            {
                _bulb.PropertyChanged -= OnBulbPropertyChanged;
            }
        }

        private void UpdateLabel()
        {
            try
            {
                InvokeInUiThread(() => Name = string.IsNullOrEmpty(_bulb.Label) ? UnnamedBulbName : _bulb.Label);
            }
            catch (Exception e)
            {
                Debug.WriteLine("BulbAdapter.UpdateLabel: " + e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void OnBulbPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName.ToUpperInvariant())
            {
                case "LABEL":
                    UpdateLabel();
                    break;
                case "HUE":
                case "SATURATION":
                case "BRIGHTNESS":
                case "KELVIN":
                    RaiseColorChanged();
                    break;
            }
        }

        private void InvokeInUiThread(Action action)
        {
            DispatcherProvider.Dispatcher.RunAsyncWrapped(action);
        }
    }
}