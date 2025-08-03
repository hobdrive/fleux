namespace Fleux.Controls
{
    using System;
    using System.Drawing;
    using Core;

    public class FleuxControlPage : FleuxPage
    {
        public FleuxControl Control = new FleuxControl();

        public FleuxControlPage() : this(false)
        {
        }

        public FleuxControlPage(bool fullScreen) : base(fullScreen)
        {
            this.InitializeGenericControl();
        }

        public override void Close()
        {
            this.Control.AnimateExit();
            base.Close();
        }

        protected override void NavigateTo(FleuxPage page)
        {
            this.Control.AnimateExit();
            base.NavigateTo(page);
        }

        private void InitializeGenericControl()
        {
            theForm.SuspendLayout();

            this.Control.Anchor =
                ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                 | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right;
            this.Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Control.BackColor = System.Drawing.Color.Black;
            this.Control.Location = new Point(4, 4);
            this.Control.Name = "control1";
            this.Control.Size = new Size(233, 261);
            this.Control.TabIndex = 0;
            this.Control.Text = string.Empty;

            theForm.Controls.Add(this.Control);
            theForm.ResumeLayout(false);
        }
    }
}
