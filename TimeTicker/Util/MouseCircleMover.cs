using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace TimeTicker
{
    public class MouseCircleMover
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public int Radius { get; set; } = 30;
        public int Steps { get; set; } = 100;
        public int DelayMs { get; set; } = 5;

        private const double USER_INTERRUPT_THRESHOLD = 4.0;
        private static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(60);

        private bool _isRunning;

        

        public MouseCircleMover()
        {
        
        }

        public void Run()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            try
            {
                Point center = MouseHelper.GetMousePosition();
                int step = 0;

                DateTime lastUserActivity = DateTime.Now;
                bool idleCallbackInvoked = false;

                while (true)
                {
                    double angle = step * (2 * Math.PI / Steps);

                    int expectedX = (int)(center.X + Radius * Math.Cos(angle));
                    int expectedY = (int)(center.Y + Radius * Math.Sin(angle));

                    // Move mouse
                    SetCursorPos(expectedX, expectedY);
                    Thread.Sleep(DelayMs);

                    Point actual = MouseHelper.GetMousePosition();

                    // USER MOVED MOUSE → STOP
                    if (Distance(actual, new Point(expectedX, expectedY)) > USER_INTERRUPT_THRESHOLD)
                    {
                        return;
                    }

                    // Check idle time
                    if (!idleCallbackInvoked &&
                        DateTime.Now - lastUserActivity >= IdleTimeout)
                    {
                        idleCallbackInvoked = true;
                        TeamsAutomation.ActivateTeams();

                        return;
                        
                    }

                    step = (step + 1) % Steps;
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public static class MouseHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetMousePosition()
        {
            GetCursorPos(out POINT p);
            return new Point(p.X, p.Y);
        }
    }



    internal static class IdleTimeDetector
    {
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static TimeSpan GetIdleTime()
        {
            var lii = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
            };

            GetLastInputInfo(ref lii);

            uint idleTicks = ((uint)Environment.TickCount) - lii.dwTime;
            return TimeSpan.FromMilliseconds(idleTicks);
        }
    }


   public static class TeamsAutomation
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        const uint KEYEVENTF_KEYUP = 0x0002;

        const byte VK_CONTROL = 0x11;
        const byte VK_E = 0x45;

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);


        

        public static void ActivateTeams()
        {
            IntPtr hWnd = FindTeamsWindow();

            // If no window, Teams is in tray or not started
            if (hWnd == IntPtr.Zero)
            {
                // Ask Teams to show itself
                Process.Start(new ProcessStartInfo
                {
                    FileName = "msteams:",
                    UseShellExecute = true
                });

                // Wait for window creation
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(300);
                    hWnd = FindTeamsWindow();
                    if (hWnd != IntPtr.Zero)
                        break;
                }
            }

            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("Teams window could not be activated.");
                return;
            }

            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);

            Thread.Sleep(500);

            // ✅ Now safely send shortcut
            SendAvailableCommand();
        }

        public static void SendAvailableCommand()
        {
            Thread.Sleep(300);

            // Open Teams command box (Ctrl + E)
            SendKeys.SendWait("^e");

            //Thread.Sleep(500);

            //// Type /available
            //SendKeys.SendWait("/available");

            //Thread.Sleep(500);

            //// Press Enter
            //SendKeys.SendWait("{ENTER}");

            Thread.Sleep(1500);
        }


    


        static IntPtr FindTeamsWindow()
        {
            IntPtr found = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint pid);

                try
                {
                    var proc = Process.GetProcessById((int)pid);
                    if (!proc.ProcessName.Equals("ms-teams", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch
                {
                    return true;
                }

                var title = new StringBuilder(256);
                GetWindowText(hWnd, title, title.Capacity);

                if (title.Length == 0)
                    return true;

                found = hWnd;
                return false; // stop enumeration
            }, IntPtr.Zero);

            return found;
        }

    }


}
