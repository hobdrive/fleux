namespace Fleux.Core.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class Dispatcher : IDisposable
    {
        private readonly Queue<Action> actionsQueue = new Queue<Action>();
        private bool stopthread;
        private Thread thread;

        public void Dispatch(Action action)
        {
            this.actionsQueue.Enqueue(action);
            this.CheckThread();
        }

        public void Dispose()
        {
            this.actionsQueue.Clear();
            this.stopthread = true;
            if (this.thread != null)
            {
                this.thread.Join();
            }
        }

        private void CheckThread()
        {
            lock (this)
            {
                if (this.thread == null)
                {
                    this.thread = new Thread(this.DispatcherWorker)
                    {
#if !SILVERLIGHT
                        Priority = ThreadPriority.AboveNormal
#endif
                    };
                    this.thread.Start();
                }
            }
        }

        private void DispatcherWorker()
        {
            while (!this.stopthread && this.actionsQueue.Count > 0)
            {
                this.actionsQueue.Dequeue().Invoke();
            }
            lock (this)
            {
                this.thread = null;
            }
        }
    }
}
