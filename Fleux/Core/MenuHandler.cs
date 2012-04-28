namespace Fleux.Core
{
    using System;
    using System.ComponentModel;

    public class MenuHandler : INotifyPropertyChanged
    {
        private string displayText;
        private Action onClickAction;
        private bool enabled;
        private bool visible;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Visible
        {
            get
            {
                return this.visible;
            }

            set
            {
                if (value != this.visible)
                {
                    this.visible = value;
                    this.RaisePropertyChanged("Visible");
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }

            set
            {
                if (value != this.enabled)
                {
                    this.enabled = value;
                    this.RaisePropertyChanged("Enabled");
                }
            }
        }

        public string DisplayText
        {
            get
            {
                return this.displayText;
            }

            set
            {
                if (value != this.displayText)
                {
                    this.displayText = value;
                    this.RaisePropertyChanged("DisplayText");
                }
            }
        }

        public Action OnClickAction
        {
            get
            {
                return this.onClickAction;
            }

            set
            {
                if (value != this.onClickAction)
                {
                    this.onClickAction = value;
                    this.RaisePropertyChanged("OnClickAction");
                }
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}