namespace Fleux.Core.Threading
{
    using System;
    using System.Threading;

    public class RealTimeDispatcher : IDisposable
    {
        private readonly AutoResetEvent dispatchEvent = new AutoResetEvent(false);
        private Action pendingAction;
        private Action lastInvokedAction;
        private bool stopthread;
        private Thread thread;

        public void Dispatch(Action action)
        {
            action();
            this.pendingAction = action;
            this.CheckThread();
        }

        public void Dispose()
        {
            this.stopthread = true;
            if (this.thread != null)
            {
                this.dispatchEvent.Set();
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
                else
                {
                    this.dispatchEvent.Set();
                }
            }
        }

        private void DispatcherWorker()
        {
            while (!this.stopthread && this.pendingAction != this.lastInvokedAction)
            {
                this.pendingAction.Invoke();
                this.lastInvokedAction = this.pendingAction;
#if !SILVERLIGHT
                this.dispatchEvent.WaitOne(1000, false);
#endif
#if SILVERLIGHT
                this.dispatchEvent.WaitOne(1000);
#endif
            }

            lock (this)
            {
                this.thread = null;
            }
        }
    }
}
