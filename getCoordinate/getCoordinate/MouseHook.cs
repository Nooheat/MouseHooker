﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace getCoordinate
{
    public class MouseHook
    {
        // Mouse Click Event Handler
        public static event EventHandler<MouseClickedEventArgs> MouseClicked;


        // Start hooking
        public void Start() 
        {
            _hookID = SetHook(_proc);
        }

        // Stop hooking
        public void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                  GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        

        // use this  when your mouse move, wheel, down, up, drag, even click (all your mouse event)
        private static IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            // when your mouse left button down
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                // The EventHandler "MouseCliked" is invoked , MouseHook_MouseClicked(object sender, MouseClickedEventArg e) in "MainWindow.xaml"
                MouseClicked?.Invoke(null, new MouseClickedEventArgs { Hook = hookStruct /*Property*/});
                
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }


       

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
          IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }


    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
        
        public override string ToString()
        {
            return $"({x},{y})";
        }

        public override bool Equals(object obj)
        {

            if (x == ((POINT)obj).x && y == ((POINT)obj).y) return true;
            return false;
        }

        public static bool IsAddable(POINT pt, List<POINT> pointList)
        {
            foreach (POINT p in pointList)
            {
                if (p.Equals(pt)) return false;
            }
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public class MouseClickedEventArgs : EventArgs
    {
        public MSLLHOOKSTRUCT Hook { get; set; }
    }
}
