using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using LifxLib;

namespace JenkinsStatusLifx.ViewModels
{
    public class GroupAdapter : BulbAdapterBase
    {
        public static readonly Func<IEnumerable<LifxBulb>, string> ListOfBulbsAsName = bulbs => string.Join(", ", bulbs.Select(b => b.Label));

        private readonly LifxGroup _source;
        private readonly Func<IEnumerable<LifxBulb>, string> _unnamedGroupName;

        public GroupAdapter(LifxGroup source, Func<IEnumerable<LifxBulb>, string> unnamedGroupName)
        {
            _source = source;
            _unnamedGroupName = unnamedGroupName ?? (_ => UnnamedGroupName);

            _source.PropertyChanged += OnBulbOnPropertyChanged;

            UpdateLabel();
        }

        public LifxGroup Source { get { return _source; } }

        public override IEnumerable<LifxBulb> Bulbs
        {
            get { return _source.Bulbs; }
        }

        private void UpdateLabel()
        {
            try
            {
                InvokeInUiThread(() => Name = string.IsNullOrEmpty(_source.Label) ? _unnamedGroupName(_source.Bulbs) : _source.Label);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GroupAdapter.UpdateLabel: " + e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        public override bool IsBulbGroup { get { return true; } }

        private void OnBulbOnPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName.ToUpperInvariant())
            {
                case "LABEL":
                    UpdateLabel();
                    break;
            }
        }

        private void InvokeInUiThread(Action action)
        {
            DispatcherProvider.Dispatcher.RunAsyncWrapped(action);
        }

        protected bool Equals(GroupAdapter other)
        {
            return _source.Equals(other._source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }
            return Equals((GroupAdapter)obj);
        }

        public override int GetHashCode()
        {
            return _source.GetHashCode();
        }
    }
}