using System;
using System.Windows;

namespace Ftware.Apps.MetroShell.Base
{
    public abstract class MetroShellWidget
    {
        public abstract string Name { get; }

        public abstract FrameworkElement WidgetControl { get; }

        public abstract Uri IconPath { get; }

        public abstract int ColumnSpan { get; }

        public virtual int X { get; private set; }

        public virtual int Y { get; private set; }

        public virtual void Load() { }

        public virtual void Unload() { }

        public virtual void Load(string path) { }

        public virtual void Load(string id, string name, int seed) { }

        public virtual void Refresh() { }

        public virtual void HandleMessage(string message) { }
    }
}