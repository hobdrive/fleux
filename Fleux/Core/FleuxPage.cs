namespace Fleux.Core
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Scaling;

    /// <summary>
    /// Base class for any page on a Fleux Application.
    /// A FleuxPage is a wrapper for the classic Windows Form class.
    /// Fleux pages can hold FleuxControls and two soft key actions
    /// </summary>
    public class FleuxPage : IDisposable
    {
        protected Form theForm;
        protected bool fullscreen;

        public FleuxPage()
            : this(false)
        {
        }

        public FleuxPage(bool fullScreen)
        {
            this.BackColor = Color.Black;
            this.fullscreen = fullScreen;
            this.InitializeComponents();
        }

        public Size Size 
        { 
            get { return new Size(this.theForm.Size.Width.ToLogic(), this.theForm.Size.Height.ToLogic()); } 
        }

        public Color BackColor { get; set; }

        public Form TheForm
        {
            get { return this.theForm; }
        }

        protected MenuHandler LeftMenu { get; private set; }

        protected MenuHandler RightMenu { get; private set; }

        public virtual void Dispose()
        {
            this.theForm.Dispose();
        }

        public virtual void Close()
        {
            this.TheForm.Close();
        }

        protected virtual void NavigateTo(FleuxPage page)
        {
            page.theForm.Owner = this.theForm;
            page.theForm.Show();
        }

        protected virtual void OnActivated()
        {
        }

        private void MenuPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.SyncWinFormsMenuItems();
        }

        private void SyncWinFormsMenuItems()
        {
            if (this.theForm.Menu.MenuItems.Count == 0)
            {
                // Create Both
                this.theForm.Menu.MenuItems.Add(new MenuItem());
                this.theForm.Menu.MenuItems.Add(new MenuItem());
                this.theForm.Menu.MenuItems[0].Click += this.LeftWinFormsMenuClick;
                this.theForm.Menu.MenuItems[1].Click += this.RightWinFormsMenuClick;
            }

            // Left
            var lmi = this.theForm.Menu.MenuItems[0];
            lmi.Text = !this.LeftMenu.Visible ? string.Empty : this.LeftMenu.DisplayText;
            lmi.Enabled = this.LeftMenu.Enabled;

            // Right
            var rmi = this.theForm.Menu.MenuItems[1];
            rmi.Text = !this.RightMenu.Visible ? string.Empty : this.RightMenu.DisplayText;
            rmi.Enabled = this.RightMenu.Enabled;
        }

        private void LeftWinFormsMenuClick(object sender, EventArgs e)
        {
            if (this.LeftMenu.Enabled && this.LeftMenu.Visible && this.LeftMenu.OnClickAction != null)
            {
                this.LeftMenu.OnClickAction();
            }
        }

        private void RightWinFormsMenuClick(object sender, EventArgs e)
        {
            if (this.RightMenu.Enabled && this.RightMenu.Visible && this.RightMenu.OnClickAction != null)
            {
                this.RightMenu.OnClickAction();
            }
        }

        private void InitializeComponents()
        {
            this.theForm = new Form { BackColor = this.BackColor, WindowState = this.fullscreen ? FormWindowState.Maximized : FormWindowState.Normal };
            this.theForm.Text = "Fleux.Net";

            if (!this.fullscreen)
            {
                var mainMenu1 = new System.Windows.Forms.MainMenu();
                this.theForm.Menu = mainMenu1;

                // MenuItems
                this.LeftMenu = new MenuHandler { Visible = true, Enabled = true };
                this.RightMenu = new MenuHandler { Visible = true, Enabled = true };
                this.LeftMenu.PropertyChanged += this.MenuPropertyChanged;
                this.RightMenu.PropertyChanged += this.MenuPropertyChanged;
                this.SyncWinFormsMenuItems();
            }

            this.theForm.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.theForm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.theForm.AutoScroll = true;
            this.theForm.BackColor = System.Drawing.Color.Black;
            this.theForm.ClientSize = new System.Drawing.Size(240, 268);

            // DpiHelper Set
            if (!FleuxApplication.Initialized)
            {
                FleuxApplication.Initialize(this.theForm.CreateGraphics());
            }

            this.theForm.Activated += (s, e) => this.OnActivated();
        }
    }
}
