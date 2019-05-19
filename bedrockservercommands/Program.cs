using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using HWND = System.IntPtr;
using System.Configuration;

namespace bedrockservercommands
{
    class Program
    {
        static bool is_window_active = false;

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        static void Main(string[] args)
        {
            if (args.Length == 1)
                SendToBedrock(args[0]);
            else
                SendToBedrock("help");
        }

        /// <summary>
        /// Get the Handle to the Window that is running the Bedrock Server
        /// The Config File will hold the name of the Running Server Window
        /// since this may be version specific.
        /// </summary>
        /// <returns>IntPtr of the window</returns>
        static IntPtr GetBedrockServerHandle()
        {
            IntPtr bedrock_handle = new HWND();
            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            {
                IntPtr handle = window.Key;
                if (window.Value == ConfigurationManager.AppSettings["server_window_name"])
                {
                    bedrock_handle = handle;
                    is_window_active = false;
                }
            }
            return bedrock_handle;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/15292175/c-sharp-using-sendkey-function-to-send-a-key-to-another-application
        /// </summary>
        static void SendToBedrock(string command)
        {
            try
            {
                if (is_window_active)
                {
                    IntPtr h = GetBedrockServerHandle();
                    SetForegroundWindow(h);
                    SendKeys.SendWait(command);
                    SendKeys.SendWait(Environment.NewLine);
                }
                else
                {
                    Console.WriteLine("Bedrock Server is not running.");
                }
            }
            catch
            {
                Console.WriteLine("Not Able to send command.");
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/7268302/get-the-titles-of-all-open-windows
        /// </summary>
        /// <returns></returns>
        public static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }
    }
}