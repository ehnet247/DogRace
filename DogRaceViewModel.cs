using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
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

        public int Dog1Position => _dog1.Position;
        public int Dog2Position => _dog2.Position;
        public int Dog3Position => _dog3.Position;
        public int Dog4Position => _dog4.Position;
        public int Dog5Position => _dog5.Position;

        [RelayCommand]
        private void Start()
        {
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
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc), _dog4);
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProc), _dog5);
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
                Thread.Sleep(500); // Simulate time taken for each step
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
            _dog1 = new Dog("Dog 1", _cts.Token, OnDogPositionChanged, ResetVelocities);
            _dog2 = new Dog("Dog 2", _cts.Token, OnDogPositionChanged, ResetVelocities);
            _dog3 = new Dog("Dog 3", _cts.Token, OnDogPositionChanged, ResetVelocities);
            _dog4 = new Dog("Dog 4", _cts.Token, OnDogPositionChanged, ResetVelocities);
            _dog5 = new Dog("Dog 5", _cts.Token, OnDogPositionChanged, ResetVelocities);
        }
    }

    public partial class Dog : ObservableObject
    {
        private CancellationToken _token;
        private Action _onDogPositionChanged;
        private Action _resetVelocities;
        private string _name;
        [ObservableProperty]
        private int _position;
        partial void OnPositionChanged(int value)
        {
            _onDogPositionChanged?.Invoke();
        }   
        [ObservableProperty]
        private int _step;

        public Dog(string name, CancellationToken token, Action onDogPositionChanged, Action resetVelocities)
        {
            _name = name;
            _token = token;
            _onDogPositionChanged = onDogPositionChanged;
            _resetVelocities = resetVelocities;
            _position = 0;
            SetVelocity();
        }

        public void SetVelocity()
        {
            // Generate random steps for each dog
            Random random = new Random();
            _step = random.Next(1, 10);
        }

        internal void ResetVelocities()
        {
            _resetVelocities?.Invoke();
        }

        public bool CancelRequested => _token.IsCancellationRequested;
    }
}
