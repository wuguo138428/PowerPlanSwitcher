namespace PowerPlanSwitcher
{
    using System;
    using System.Windows.Forms;
    using static Vanara.PInvoke.User32;

    public partial class ToastDlg : Form
    {
        private static readonly int DisplayDuration = 2000;

        private static SynchronizationContext? syncContext;
        private static ToastDlg? toastDlg;

        private static Color ButtonBackgroundColor =>
            ColorThemeHelper.GetActiveColorTheme() == ColorTheme.Light
            ? SystemColors.Menu
            : Color.FromArgb(0x15, 0x15, 0x15);
        private static Color ForegroundColor =>
            ColorThemeHelper.GetActiveColorTheme() == ColorTheme.Light
            ? SystemColors.WindowText
            : SystemColors.HighlightText;
        private static Color TlpPowerSchemesBackColor =>
            ColorThemeHelper.GetActiveColorTheme() == ColorTheme.Light
            ? SystemColors.ScrollBar
            : SystemColors.WindowFrame;

        public ToastDlg() => InitializeComponent();

        protected override void OnLoad(EventArgs e)
        {
            // BackColor = TlpPowerSchemesBackColor;
            DisplayTimer.Interval = DisplayDuration;

            BackColor = TlpPowerSchemesBackColor;
            tableLayoutPanel1.BackColor = ButtonBackgroundColor;

            PibAppIcon.BackColor = ButtonBackgroundColor;
            LblTitle.ForeColor = ForegroundColor;
            LblTitle.BackColor = ButtonBackgroundColor;

            PibPowerSchemeIcon.BackColor = ButtonBackgroundColor;
            LblPowerSchemeName.ForeColor = ForegroundColor;
            LblPowerSchemeName.BackColor = ButtonBackgroundColor;

            LblReason.ForeColor = ForegroundColor;
            LblReason.BackColor = ButtonBackgroundColor;

            Location = GetPositionOnTaskbar(Size);

            DisplayTimer.Stop();
            DisplayTimer.Start();

            base.OnLoad(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                // Used to hide the banner from alt+tab
                // source: https://www.csharp411.com/hide-form-from-alttab/
                cp.ExStyle |= (int)WindowStylesEx.WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        private static Point GetPositionOnTaskbar(Size windowSize)
        {
            //1. 
            // var bounds = Taskbar.CurrentBounds;
            // switch (Taskbar.Position)
            // {
                // case TaskbarPosition.Left:
                    // bounds.Location += bounds.Size;
                    // return new Point(bounds.X, bounds.Y - windowSize.Height);

                // case TaskbarPosition.Top:
                    // bounds.Location += bounds.Size;
                    // return new Point(bounds.X - windowSize.Width, bounds.Y);

                // case TaskbarPosition.Right:
                    // bounds.Location -= windowSize;
                    // return new Point(bounds.X, bounds.Y + bounds.Height);

                // case TaskbarPosition.Bottom:
                    // bounds.Location -= windowSize;
                    // return new Point(bounds.X + bounds.Width, bounds.Y);

                // case TaskbarPosition.Unknown:
                // default:
                    // return new Point(0, 0);
            // }



            //2. center the screen
            // Rectangle workArea = Screen.PrimaryScreen.WorkingArea;

            // int x = workArea.Left + (workArea.Width - windowSize.Width) / 2;
            // int y = workArea.Top + (workArea.Height - windowSize.Height) / 2;

            // return new Point(x, y);



            //3. follow system
            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;

            int y = workArea.Top + (workArea.Height - windowSize.Height) / 2;

            return new Point(workArea.Left + (workArea.Width - windowSize.Width), y);
        }

        private void Any_Click(object sender, EventArgs e) => Dispose();

        private void DisplayTimer_Tick(object sender, EventArgs e) => Dispose();

        public static void Initialize()
        {
            syncContext = SynchronizationContext.Current;
            if (syncContext is not WindowsFormsSynchronizationContext)
            {
                throw new InvalidOperationException(
                    "Initialize must be called from an UI thread");
            }
        }

        public static void ShowToastNotification(
            Guid activeSchemeGuid,
            string activationReason)
        {
            if (syncContext is null)
            {
                throw new InvalidOperationException(
                    "ToastNotification was not initialized before use");
            }

            syncContext.Send(_ =>
            {
                if (toastDlg == null)
                {
                    toastDlg = new ToastDlg();
                    toastDlg.Disposed += (_, _) => toastDlg = null;
                }

                toastDlg.PibPowerSchemeIcon.Image =
                    PowerSchemeSettings.GetSetting(activeSchemeGuid)?.Icon;
                toastDlg.LblPowerSchemeName.Text =
                    PowerManager.GetPowerSchemeName(activeSchemeGuid);
                toastDlg.LblReason.Text = activationReason;

                toastDlg.DisplayTimer.Stop();
                toastDlg.DisplayTimer.Start();

                toastDlg.Show();
            }, null);
        }
    }
}
