using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Command;
using LifxLib;
using LifxLib.Enums;
using LifxLib.Messages;

namespace JenkinsStatusLifx.ViewModels
{
    public class MainViewModel : BulbListBaseViewModel
    {
        private readonly ObservableCollection<string> _jobs;
        private readonly RelayCommand _observeCommand;
        private BuildStatus? _previousBuildStatus;
        private string _selectedServer;
        private string _selectedJob;
        private bool _isObserving;
        private bool _isObservingStopRequested;

        public MainViewModel()
        {
            _jobs = new ObservableCollection<string>();
            _observeCommand = new RelayCommand(Observe);

            Bulbs.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(() => SelectedBulb);
                RaisePropertyChanged(() => IsBulbAvailable);
            };
        }

        public ICommand ObserveCommand { get { return _observeCommand; } }

        public IEnumerable<string> Jobs { get { return _jobs; } }

        public bool IsJobAvailable { get { return _jobs.Count >= 1; } }

        public bool IsBulbAvailable { get { return Bulbs.Any(); } }

        public bool IsObserving
        {
            get { return _isObserving; }
            set
            {
                if (Set(ref _isObserving, value))
                {
                    RaisePropertyChanged(() => ObserveButtonText);
                }
            }
        }

        public string ObserveButtonText
        {
            get { return _isObserving ? "Stop" : "Observe"; }
        }

        public string SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                Set(ref _selectedJob, value);
                _previousBuildStatus = null;
            }
        }

        public string SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                if (Set(ref _selectedServer, value))
                {
                    GetJobs();
                }

                _previousBuildStatus = null;
            }
        }

        private void Observe()
        {
            if (string.IsNullOrEmpty(SelectedServer) || string.IsNullOrEmpty(SelectedJob) || SelectedBulb == null)
            {
                return;
            }

            if (_isObservingStopRequested)
            {
                return;
            }

            if (_isObserving)
            {
                _isObservingStopRequested = true;
            }
            else
            {
                IsObserving = true;
                var thread = new Thread(_ => ObserveInThread());
                thread.Start();
            }
        }

        private void GetJobs()
        {
            _jobs.Clear();
            SelectedJob = null;

            Uri uri;
            if (string.IsNullOrEmpty(SelectedServer) || !Uri.TryCreate(SelectedServer, UriKind.Absolute, out uri))
            {
                return;
            }

            try
            {
                using (var client = new WebClient())
                {
                    var url = SelectedServer.TrimEnd('/') + "/api/xml";

                    var xml = client.DownloadString(url);
                    var doc = XDocument.Parse(xml);

                    var jobs = doc.Descendants("job");
                    foreach (var job in jobs)
                    {
                        var name = job.Descendants("name").Single().Value;
                        _jobs.Add(name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            RaisePropertyChanged(() => IsJobAvailable);
        }

        private void ObserveInThread()
        {
            try
            {
                var url = SelectedServer.TrimEnd('/') + "/api/xml";

                using (var client = new WebClient())
                {
                    while (!_isObservingStopRequested)
                    {
                        var xml = client.DownloadString(url);
                        var doc = XDocument.Parse(xml);

                        var jobs = doc.Descendants("job");
                        foreach (var job in jobs)
                        {
                            var name = job.Descendants("name").Single().Value;
                            var color = job.Descendants("color").Single().Value;

                            if (!string.Equals(name, SelectedJob, StringComparison.Ordinal))
                            {
                                continue;
                            }

                            var currentBuildStatus = ParseBuildStatus(color);

                            if (_previousBuildStatus.HasValue && _previousBuildStatus.Value.Equals(currentBuildStatus))
                            {
                                continue;
                            }

                            _previousBuildStatus = currentBuildStatus;

                            switch (currentBuildStatus)
                            {
                                case BuildStatus.BlueAnimated:
                                //SetPulse(110);
                                //break;
                                case BuildStatus.Blue:
                                    SetColor(110);
                                    break;
                                case BuildStatus.RedAnimated:
                                //SetPulse(0);
                                //break;
                                case BuildStatus.Red:
                                    SetColor(0);
                                    break;
                                default:
                                    SetColor(40);
                                    break;
                            }
                        }

                        for (int i = 0; i < 10 && !_isObservingStopRequested; i++)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(0.5));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            IsObserving = false;
            _isObservingStopRequested = false;
        }

        private BuildStatus ParseBuildStatus(string color)
        {
            switch (color.ToLowerInvariant())
            {
                case "blue":
                    return BuildStatus.Blue;
                case "red":
                    return BuildStatus.Red;
                case "blue_anime":
                    return BuildStatus.BlueAnimated;
                case "red_anime":
                    return BuildStatus.RedAnimated;
                default:
                    return BuildStatus.Unknown;
            }
        }

        private void SetColor(ushort hue)
        {
            foreach (var bulb in SelectedBulb.Bulbs)
            {
                SwitchOn(bulb);
                SetColor(bulb, hue);
            }
        }

        private void SetPulse(ushort hue)
        {
            foreach (var bulb in SelectedBulb.Bulbs)
            {
                SwitchOn(bulb);
                SetPulse(bulb, hue);
            }
        }

        private void SetColor(LifxBulb bulb, ushort hue)
        {
            var packet = (LifxSetLightColor)LifxPacketFactory.GetPacket((ushort)AppToBulb.SetLightColor);
            packet.Brightness = 255;
            packet.FadeTime = 0;
            packet.Hue = hue;
            packet.Kelvin = 3500;
            packet.Saturation = 100;

            LifxNetwork.Instance.SendToBulb(bulb, packet);
        }

        private void SetPulse(LifxBulb bulb, ushort hue)
        {
            var setLightColor = (LifxSetLightColor)LifxPacketFactory.GetPacket((ushort)AppToBulb.SetLightColor);
            setLightColor.Brightness = 100;
            setLightColor.FadeTime = 0;
            setLightColor.Hue = hue;
            setLightColor.Kelvin = 3500;
            setLightColor.Saturation = 100;

            LifxNetwork.Instance.SendToBulb(bulb, setLightColor);

            var setWaveform = (LifxSetWaveForm)LifxPacketFactory.GetPacket((ushort)AppToBulb.SetWaveform);
            setWaveform.Period = 2000; // grösser = länger
            setWaveform.Kelvin = 3500;
            setWaveform.Hue = hue;
            setWaveform.Saturation = 100;
            setWaveform.Brightness = 100;
            setWaveform.Cycles = 50000;
            setWaveform.DutyCycles = 1;
            setWaveform.Stream = 0;
            setWaveform.Transient = 1; // true??
            setWaveform.Waveform = 1;

            LifxNetwork.Instance.SendToBulb(bulb, setWaveform);
        }

        private void SwitchOn(LifxBulb bulb)
        {
            var packet = (LifxSetPowerState)LifxPacketFactory.GetPacket((ushort)AppToBulb.SetPowerState);
            packet.OnOff = OnOff.On;

            LifxNetwork.Instance.SendToBulb(bulb, packet);
        }
    }
}
