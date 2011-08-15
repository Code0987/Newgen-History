using System;
using System.Windows;

namespace Newgen.Base
{
    public abstract class NewgenWidget
    {
        public abstract string Name { get; }

        public abstract FrameworkElement WidgetControl { get; }

        public abstract Uri IconPath { get; }

        public abstract int ColumnSpan { get; }

        public virtual void Load() { }

        public virtual void Unload() { }

        public virtual void Load(string path) { }

        public virtual void Load(string id, string name, int seed) { }

        public virtual void Refresh() { }

        public virtual int X { get; private set; }

        public virtual int Y { get; private set; }
    }
}