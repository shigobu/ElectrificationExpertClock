using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ElectrificationExpertClock
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        AppWindow appWindow;
        int Width = 200;
        int Height = 190;
        private DispatcherTimer dispatcherTimer;


        public MainWindow()
        {
            this.InitializeComponent();

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            appWindow = AppWindow.GetFromWindowId(myWndId);

            appWindow.Resize(new SizeInt32(Width, Height));
            SubClassing();

            var op = OverlappedPresenter.Create();
            op.IsMaximizable = false;
            op.IsMinimizable = false;
            op.IsResizable = false;
            op.IsAlwaysOnTop = true;
            appWindow.SetPresenter(op);

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();

        }


        private void DispatcherTimer_Tick(object sender, object e)
        {
            DateTime now = DateTime.Now;
            time.Text = now.ToString();

            bool isSummer = false;

            if (7 <= now.Month && now.Month <= 9)
            {
                isSummer = true;
                season.Text = "夏季";
            }
            else
            {
                season.Text = "その他の季節";
            }

            if (10 <= now.Hour && now.Hour < 17)
            {
                timePeriod.Text = "昼間";
                timePeriodEmoji.Text = "☀";
                if (isSummer)
                {
                    price.Text = "43.93円";
                }
                else
                {
                    price.Text = "40.44円";
                }
            }
            else if ((7 <= now.Hour && now.Hour < 10) || (17 <= now.Hour && now.Hour < 23))
            {
                timePeriod.Text = "朝晩";
                timePeriodEmoji.Text = "🌅";
                price.Text = "35.87円";
            }
            else
            {
                timePeriod.Text = "夜間";
                timePeriodEmoji.Text = "🌙";
                price.Text = "28.85円";
            }

        }

        private NativeMethods.WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;

        private void SubClassing()
        {
            // ウインドウのハンドルを取ってくる
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            newWndProc = new NativeMethods.WinProc(NewWindowProc);
            oldWndProc = NativeMethods.SetWindowLong(hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
        }

        private IntPtr NewWindowProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case PInvoke.User32.WindowMessage.WM_GETMINMAXINFO:
                    // ウインドウのサイズが変わるたびにここを通る
                    var dpi = PInvoke.User32.GetDpiForWindow(hWnd);
                    float scalingFactor = (float)dpi / 96;

                    // ここで、最上の大きさをMINMAXINFOに入れて指定する
                    NativeMethods.MINMAXINFO minMaxInfo = Marshal.PtrToStructure<NativeMethods.MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.x = (int)(Width * scalingFactor);
                    minMaxInfo.ptMinTrackSize.y = (int)(Height * scalingFactor);
                    minMaxInfo.ptMaxTrackSize.x = (int)(Width * scalingFactor);
                    minMaxInfo.ptMaxTrackSize.y = (int)(Height * scalingFactor);
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;

            }
            // WM_GETMINMAXINFO以外の、自分で処理したくないMsgは、もとのWndProcに任せる
            return NativeMethods.CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }
    }

    public class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public PInvoke.POINT ptReserved;
            public PInvoke.POINT ptMaxSize;
            public PInvoke.POINT ptMaxPosition;
            public PInvoke.POINT ptMinTrackSize;
            public PInvoke.POINT ptMaxTrackSize;
        }

        public delegate IntPtr WinProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
    }
}
