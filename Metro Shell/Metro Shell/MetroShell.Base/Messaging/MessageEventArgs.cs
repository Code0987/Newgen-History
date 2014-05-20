using System;

namespace Ftware.Apps.MetroShell.Base.Messaging
{
    public sealed class MessageEventArgs : EventArgs
    {
        private readonly MessageData data;

        internal MessageEventArgs(MessageData data)
        {
            this.data = data;
        }

        public MessageData Data
        {
            get { return data; }
        }
    }
}