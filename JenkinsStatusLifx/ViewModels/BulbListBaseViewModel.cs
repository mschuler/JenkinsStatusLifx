using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using LifxLib;

namespace JenkinsStatusLifx.ViewModels
{
    public abstract class BulbListBaseViewModel : ViewModelBase
    {
        private static readonly ObservableCollection<BulbAdapterBase> _bulbs = new ObservableCollection<BulbAdapterBase>();
        private static readonly object _bulbListLock = new object();
        private static BulbAdapterBase _selectedBulb;

        static BulbListBaseViewModel()
        {
            LifxNetwork.Instance.BulbCollectionChanged += (s, e) => DispatcherProvider.Dispatcher.RunAsyncWrapped(UpdateBulbAdapters);
            LifxNetwork.Instance.GroupCollectionChanged += (s, e) => DispatcherProvider.Dispatcher.RunAsyncWrapped(UpdateBulbAdapters);

            UpdateBulbAdapters();
        }

        private static void UpdateBulbAdapters()
        {
            try
            {
                var selectedBulb = _selectedBulb;

                lock (_bulbListLock)
                {
                    foreach (var adapter in _bulbs.OfType<BulbAdapter>())
                    {
                        adapter.Dispose();
                    }
                    _bulbs.Clear();

                    var newList = new List<BulbAdapterBase>();
                    var allBulbsAdapter = new AllBulbAdapter();

                    foreach (var group in LifxNetwork.Instance.Groups.ToList())
                    {
                        newList.Add(new GroupAdapter(group, GroupAdapter.ListOfBulbsAsName));
                    }

                    foreach (var lifxBulb in LifxNetwork.Instance.Bulbs.ToList())
                    {
                        allBulbsAdapter.AddBulb(lifxBulb);
                        newList.Add(new BulbAdapter(lifxBulb));
                    }

                    if (newList.Any())
                    {
                        newList.Insert(0, allBulbsAdapter);
                    }

                    foreach (var adapter in newList)
                    {
                        _bulbs.Add(adapter);
                    }
                }

                _selectedBulb = _bulbs.FirstOrDefault(a => a.Equals(selectedBulb));
                if (selectedBulb == null)
                {
                    _selectedBulb = _bulbs.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine(e.StackTrace);
            }
        }

        public ObservableCollection<BulbAdapterBase> Bulbs { get { return _bulbs; } }

        public virtual BulbAdapterBase SelectedBulb
        {
            get
            {
                if (_selectedBulb == null)
                {
                    lock (_bulbListLock)
                    {
                        _selectedBulb = Bulbs.FirstOrDefault();
                    }
                }
                return _selectedBulb;
            }
            set { Set(() => SelectedBulb, ref _selectedBulb, value); }
        }

        protected LifxNetwork Network { get { return LifxNetwork.Instance; } }

        protected Dispatcher Dispatcher { get { return DispatcherProvider.Dispatcher; } }
    }
}