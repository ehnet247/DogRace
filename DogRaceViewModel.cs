using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Automation;

namespace DogRace
{
    internal partial class DogRaceViewModel : ObservableRecipient
    {
        CancellationTokenSource _cts = new CancellationTokenSource();

        private Dog _dog1;
        private Dog _dog2;
        private Dog _dog3;
        private Dog _dog4;
        private Dog _dog5;

        public List<int> Ranking { get; } = new List<int>();

        public int Dog1Position => _dog1.Position;
        public int Dog2Position => _dog2.Position;
        public int Dog3Position => _dog3.Position;
        public int Dog4Position => _dog4.Position;
        public int Dog5Position => _dog5.Position;

        [ObservableProperty]
        private Visibility _rank1Visibility = Visibility.Collapsed,
                           _rank2Visibility = Visibility.Collapsed,
                           _rank3Visibility = Visibility.Collapsed,
                           _rank4Visibility = Visibility.Collapsed,
                           _rank5Visibility = Visibility.Collapsed,
                           _dog4Visibility = Visibility.Collapsed,
                           _dog5Visibility = Visibility.Collapsed;

        [ObservableProperty]
        private string _dog1Name = "", _dog2Name = "", _dog3Name = "", _dog4Name = "", _dog5Name = "";

        [ObservableProperty]
        private string _dog1Rank = "", _dog2Rank = "", _dog3Rank = "", _dog4Rank = "", _dog5Rank = "";

        [RelayCommand]
        private void Start()
        {
            // Assign dog names
            _dog1.Name = Dog1Name;
            _dog2.Name = Dog2Name;
            _dog3.Name = Dog3Name;
            _dog4.Name = Dog4Name;
            _dog5.Name = Dog5Name;
            // Generate random steps for each dog
            _dog1.SetVelocity();
            _dog2.SetVelocity();
            _dog3.SetVelocity();
            _dog4.SetVelocity();
            _dog5.SetVelocity();

            // Create running tasks using thread pool
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc),  _dog1);
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc), _dog2);
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc), _dog3);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc), _dog4);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc), _dog5);
        }

        [RelayCommand]
        private void Stop()
        {
            _cts.Cancel();
        }

        [RelayCommand]
        private void Reset()
        {
            ResetVelocities();
            _dog1.Position = 0;
            _dog2.Position = 0;
            _dog3.Position = 0;
            _dog4.Position = 0;
            _dog5.Position = 0;
            Rank1Visibility = Visibility.Collapsed;
            Rank2Visibility = Visibility.Collapsed;
            Rank3Visibility = Visibility.Collapsed;
            Ranking.Clear();
        }

        private void OnDogPositionChanged()
        {
                    OnPropertyChanged(nameof(Dog1Position));
                    OnPropertyChanged(nameof(Dog2Position));
                    OnPropertyChanged(nameof(Dog3Position));
                    OnPropertyChanged(nameof(Dog4Position));
                    OnPropertyChanged(nameof(Dog5Position));
        }

        private void ResetVelocities()
        {
            _dog1.SetVelocity();
            _dog2.SetVelocity();
            _dog3.SetVelocity();
            _dog4.SetVelocity();
            _dog5.SetVelocity();
        }

        private void Arrived(object number)
        {
            if (number is int dogNumber)
            {
                if (!Ranking.Contains(dogNumber))
                {
                    Ranking.Add(dogNumber);
                }
            }
            if (Ranking.Contains(1))
                Dog1Rank = $"{Ranking.IndexOf(1) + 1}";
            if (Ranking.Contains(2))
                Dog2Rank = $"{Ranking.IndexOf(2) + 1}";
            if (Ranking.Contains(3))
                Dog3Rank = $"{Ranking.IndexOf(3) + 1}";
            if (Ranking.Contains(4))
                Dog4Rank = $"{Ranking.IndexOf(4) + 1}";
            if (Ranking.Contains(5))
                Dog5Rank = $"{Ranking.IndexOf(5) + 1}";
            Rank1Visibility = Visibility.Visible;
            Rank2Visibility = Visibility.Visible;
            Rank3Visibility = Visibility.Visible;
        }

        static void RunProc(object? dog)
        {
            int runCounter = 0;
            if (dog is not Dog dogInstance)
                return;
            if (dogInstance.CancelRequested)
                return;
            if (dogInstance.Position >= 1000)
                return;
            while (dogInstance.Position < 1000)
            {
                dogInstance.Position += dogInstance.Step;
                if (dogInstance.Position >= 1000)
                {

                    dogInstance.Arrived();
                }
                Thread.Sleep(100); // Simulate time taken for each step
                runCounter++;
                // Reset velocities from time to time
                if (runCounter >= 10)
                {
                    dogInstance.ResetVelocities();
                    runCounter = 0;
                }
            }
        }

        public DogRaceViewModel()
        {
            _dog1 = new Dog(1, _cts.Token, OnDogPositionChanged, ResetVelocities, Arrived);
            _dog2 = new Dog(2, _cts.Token, OnDogPositionChanged, ResetVelocities, Arrived);
            _dog3 = new Dog(3, _cts.Token, OnDogPositionChanged, ResetVelocities, Arrived);
            _dog4 = new Dog(4, _cts.Token, OnDogPositionChanged, ResetVelocities, Arrived);
            _dog5 = new Dog(5, _cts.Token, OnDogPositionChanged, ResetVelocities, Arrived);
        }
    }

    public partial class Dog : ObservableObject
    {
        private CancellationToken _token;
        private Action _onDogPositionChanged;
        private Action _resetVelocities;
        private Action<object> _arrived;
        private int _number;
        [ObservableProperty]
        private int _position;
        partial void OnPositionChanged(int value)
        {
            _onDogPositionChanged?.Invoke();
        }
        [ObservableProperty]
        private string _name;

        public int Step { get; private set; }

        public Dog(int number, CancellationToken token, Action onDogPositionChanged, Action resetVelocities, Action<object> arrived)
        {
            _number = number;
            _token = token;
            _onDogPositionChanged = onDogPositionChanged;
            _resetVelocities = resetVelocities;
            _arrived = arrived;

            _position = 0;
            SetVelocity();
        }

        public void SetVelocity()
        {
            // Generate random steps for each dog
            Random random = new Random();
            Step = random.Next(1, 10);
        }

        internal void ResetVelocities()
        {
            _resetVelocities?.Invoke();
        }

        internal void Arrived()
        {
            _arrived?.Invoke(_number);
        }

        public bool CancelRequested => _token.IsCancellationRequested;
    }
}
