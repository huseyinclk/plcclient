using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Util
{
    public class AssemblyHelper
    {
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_COMMAND = 0x0112;      // Code for Windows command
        public const int WM_CLOSE = 0xF060;		 // Command code for close window

        public static void PostMessage(IntPtr hWndNotepad)
        {
            try
            {
                //IntPtr hWndNotepad = Process.GetProcessesByName("notepad")[0].MainWindowHandle;
                if (hWndNotepad != null)
                {
                    // Close Window
                    PostMessage(hWndNotepad, WM_COMMAND, WM_CLOSE, 0);
                }
                Console.WriteLine("Command sent.  Press enter to close.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Notepad not running.  Press enter to close.");
                Console.Read();
            }
        }
    }
}
