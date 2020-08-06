using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace sqlmslt {
    public partial class fmMain : Form {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static string Hostname { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }

        public string ExecPath { get; set; }

        public Timer WaitTimer { get; set; }
        public int MsWaited;

        public fmMain(string hostname, string username, string password) {
            Hostname = hostname;
            Username = username;
            Password = password;

            InitializeComponent();

            this.KeyUp += OnKeyUp;
            this.Shown += OnShown;
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Application.Exit();
            }
        }

        private void OnShown(object sender, EventArgs e) {
            // Locate.
            this.lbStatus.Text = "Locating local installation of SQL Management Studio";
            Application.DoEvents();

            this.ExecPath = GetSqlManagementStudioExecutable();

            if (this.ExecPath == null) {
                MessageBox.Show(
                    "Unable to locate local installation of SQL Management Studio",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Application.Exit();
                return;
            }

            // Init.
            this.lbStatus.Text = "Starting a new instance of SSMS";
            Process.Start(this.ExecPath);

            if (Hostname == null &&
                Username == null &&
                Password == null) {

                Application.Exit();
                return;
            }

            // Wait for window.
            this.lbStatus.Text = "Waiting for running instance and window to manipulate...";

            this.WaitTimer = new Timer {
                Enabled = true,
                Interval = 250
            };

            this.WaitTimer.Tick += TimerOnTick;
        }

        /// <summary>
        /// Cycle and see if we can find the correct window to manipulate.
        /// </summary>
        private void TimerOnTick(object sender, EventArgs e) {
            this.MsWaited += 250;
            this.lbStatus.Text = string.Format(
                "Waiting for running instance and window to manipulate... {0} seconds",
                this.MsWaited / 1000);

            // Check if we can find a window to manipulate.
            var handle = FindConnectToServerWindow();

            if (!handle.HasValue) {
                return;
            }

            this.lbStatus.Text = "Found window. Attempting to fill in details and login...";
            this.WaitTimer.Enabled = false;

            // Attempt to fill in the details and login.
            FillInDetailsAndLogin();
        }

        /// <summary>
        /// Attempt to find executable for SQL Management Studio.
        /// </summary>
        private string GetSqlManagementStudioExecutable() {
            var paths = new List<string> {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            var index = -1;

            while (true) {
                index++;

                if (index == paths.Count) {
                    break;
                }

                try {
                    var files = Directory.GetFiles(
                        paths[index],
                        "ssms.exe",
                        SearchOption.TopDirectoryOnly);

                    if (files.Length > 0) {
                        return files[0];
                    }
                }
                catch {
                    //
                }

                try {
                    paths.AddRange(
                        Directory.GetDirectories(
                            paths[index],
                            "*",
                            SearchOption.TopDirectoryOnly));
                }
                catch {
                    //
                }
            }

            return null;
        }

        /// <summary>
        /// Find the correct window.
        /// </summary>
        private IntPtr? FindConnectToServerWindow() {
            var handle = FindWindow(null, "Connect to Server");

            if (handle == IntPtr.Zero) {
                return null;
            }

            return handle;
        }

        /// <summary>
        /// Attempt to fill in the details and login.
        /// </summary>
        private void FillInDetailsAndLogin() {
            SendKeys.SendWait(Hostname);
            SendKeys.SendWait("\t\t");

            SendKeys.SendWait(Username);
            SendKeys.SendWait("\t");

            SendKeys.SendWait(Password);
            SendKeys.SendWait("\t");

            SendKeys.SendWait(" ");
            SendKeys.SendWait("\t");

            SendKeys.SendWait("\r");
            Application.Exit();
        }
    }
}