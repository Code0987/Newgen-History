using System;
using System.Collections.Generic;
using System.Windows.Interop;

namespace Ftware.Apps.MetroShell.Base.Messaging
{
    public static class MessagingHelper
    {
        #region Common

        public const string MetroShellKey = "Ftware.Apps.MetroShell.Message";

        public const string MetroShellWidgetKey = "Ftware.Apps.MetroShell.WidgetMessage";

        private static List<IntPtr> listners = new List<IntPtr>();
        private static readonly SerializerHelper serializerHelper = new SerializerHelper();
        public delegate void MessageHandler(MessageEventArgs e);
        public static event MessageHandler MessageReceived;

        #endregion Common

        #region Broadcasting

        public static void SendMessageToMetroShell(string key, string message)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message cannot be null");
            }

            SendMessage(MetroShellKey, key + ":" + message);
        }

        public static void SendMessageToWidget(string name, string message)
        {
            if (name == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message cannot be null");
            }

            SendMessage(MetroShellWidgetKey, name + ":" + message);
        }

        internal static void SendMessage(string key, object message)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message cannot be null");
            }

            SendMessage(key, serializerHelper.Serialize(message));
        }

        internal static void SendMessage(string key, string message)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message packet cannot be null");
            }

            using (var dataGram = new WinMsgData(key, message))
            {
                // Allocate the DataGram to a memory address contained in COPYDATASTRUCT
                Native.COPYDATASTRUCT dataStruct = dataGram.ToStruct();
                // Use a filter with the EnumWindows class to get a list of windows containing
                // a property name that matches the destination channel. These are the listening
                // applications.

                foreach (var hWnd in listners)
                {
                    IntPtr outPtr;
                    // For each listening window, send the message data. Return if hang or unresponsive within 1 sec.
                    Native.SendMessageTimeout(hWnd, Native.WM_COPYDATA, IntPtr.Zero, ref dataStruct,
                                              Native.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out outPtr);
                }
            }
        }

        #endregion Broadcasting

        #region Listening

        public static void AddListner(IntPtr hWnd)
        {
            try
            {
                HwndSource src = HwndSource.FromHwnd(hWnd);
                src.AddHook(new HwndSourceHook(WndProc));
                listners.Add(hWnd);
            }
            catch { }
        }

        public static void RemoveListner(IntPtr hWnd)
        {
            try
            {
                HwndSource src = HwndSource.FromHwnd(hWnd);
                src.RemoveHook(new HwndSourceHook(WndProc));
                listners.Remove(hWnd);
            }
            catch { }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != Native.WM_COPYDATA)
            {
                return IntPtr.Zero;
            }

            using (var dataGram = WinMsgData.FromPointer(lParam))
            {
                if (MessageReceived != null && dataGram.IsValid)
                {
                    MessageReceived.Invoke(new MessageEventArgs(dataGram));
                }
            }
            return IntPtr.Zero;
        }

        #endregion Listening
    }
}