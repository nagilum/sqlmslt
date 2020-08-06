using System;
using System.Windows.Forms;

namespace sqlmslt {
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new fmMain(
                GetArgsValue(args, "h"),
                GetArgsValue(args, "u"),
                GetArgsValue(args, "p")));
        }

        /// <summary>
        /// Get arg value.
        /// </summary>
        private static string GetArgsValue(string[] args, string key) {
            if (args == null ||
                args.Length == 1) {

                return null;
            }

            for (var i = 0; i < args.Length - 1; i++) {
                if (args[i] == "-" + key) {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}