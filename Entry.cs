using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogRace
{
    public class Entry : TextBox
    {
        public static readonly DependencyProperty placeHolderProperty = DependencyProperty.Register(nameof(PlaceHolder), typeof(string), typeof(Entry), new PropertyMetadata(string.Empty));
        private bool _loadedOnce = false;
        private Brush _foregroundBackup;
        public string PlaceHolder
        {
            get => (string)GetValue(placeHolderProperty);
            set => SetValue(placeHolderProperty, value);
        }



        public Entry()
        {
            this.Loaded += Entry_Loaded;
        }

        private void Entry_Loaded(object sender, RoutedEventArgs e)
        {
            // Backup the original foreground color and set the placeholder text
            _foregroundBackup = this.Foreground;
            this.Foreground = Brushes.LightGray;
            Text = PlaceHolder;
            _loadedOnce = true;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (_loadedOnce && Text == PlaceHolder)
            {
                // Restore the original foreground color and clear the placeholder text
                Text = string.Empty;
                this.Foreground = _foregroundBackup;
            }
            base.OnGotFocus(e);
        }

    }
}
