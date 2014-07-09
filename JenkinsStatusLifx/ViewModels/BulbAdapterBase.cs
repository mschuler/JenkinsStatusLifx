using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using LifxLib;

namespace JenkinsStatusLifx.ViewModels
{
    public abstract class BulbAdapterBase : ObservableObject
    {
        private static string _unnamedBulb;
        private static string _unnamedGroup;

        public static string UnnamedBulbName
        {
            get
            {
                if (string.IsNullOrEmpty(_unnamedBulb))
                {
                    // TODO: i18n
                    _unnamedBulb = "Unnamed Bulb";
                }

                return _unnamedBulb;
            }
        }

        public static string UnnamedGroupName
        {
            get
            {
                if (string.IsNullOrEmpty(_unnamedGroup))
                {
                    // TODO: i18n
                    _unnamedGroup = "Unnamed Group";
                }

                return _unnamedGroup;
            }
        }

        private readonly IList<LifxBulb> _bulbs;
        private string _name;

        protected BulbAdapterBase(LifxBulb bulb)
        {
            _bulbs = new List<LifxBulb> { bulb };
        }

        protected BulbAdapterBase()
        {
            _bulbs = new List<LifxBulb>();
        }

        public abstract bool IsBulbGroup { get; }

        public bool IsNotBulbGroup { get { return !IsBulbGroup; } }

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public LifxColor BulbColor
        {
            get
            {
                if (!Bulbs.Any())
                {
                    return new LifxColor(0, 0, 0);
                }

                var hue = (ushort)Bulbs.Average(b => b.Hue);
                var saturation = (ushort)Bulbs.Average(b => b.Saturation);
                var brightness = (ushort)Bulbs.Average(b => b.Brightness);
                return new LifxColor(hue, saturation, brightness);
            }
        }

        public virtual IEnumerable<LifxBulb> Bulbs { get { return _bulbs; } }

        public void AddBulb(LifxBulb bulb)
        {
            _bulbs.Add(bulb);
            RaiseColorChanged();
        }

        public void RaiseColorChanged()
        {
            DispatcherProvider.Dispatcher.RunAsyncWrapped(() => RaisePropertyChanged(() => BulbColor));
        }
    }
}