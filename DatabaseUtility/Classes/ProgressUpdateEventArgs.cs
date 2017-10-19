using System;

namespace LawstreamUpdate.Classes
{
    public class ProgressUpdateEventArgs : EventArgs
    {
        private int total;
        private int current;

        // Constructor.
        public ProgressUpdateEventArgs(int current, int total)
        {
            this.current = current;
            this.total = total;
        }

        // Properties.
        public int Current
        {
            get { return current; }
        }

        public int Total
        {
            get { return total; }
        }
    }
}
