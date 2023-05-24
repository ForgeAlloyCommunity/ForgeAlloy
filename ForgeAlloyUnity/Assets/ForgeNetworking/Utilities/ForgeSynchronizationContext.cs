
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Forge.Utilities
{

    public sealed class ForgeSynchronizationContext : SynchronizationContext
    {
        private struct SynchActivity
        {
            public SendOrPostCallback CallbackDelegate;
            public object State;
        }

        private readonly ConcurrentQueue<SynchActivity> queue =
            new ConcurrentQueue<SynchActivity>();

        private readonly int mainThreadId = Thread.CurrentThread.ManagedThreadId;

		private int sleepInterval = 10;
		public CancellationTokenSource CancellationSource { get; set; }

		private SynchronizationContext saveContext;

		public ForgeSynchronizationContext()
        {
			saveContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(this);
		}

        public override SynchronizationContext CreateCopy()
        {
            return new ForgeSynchronizationContext();
        }

        public override void Send(SendOrPostCallback callbackDelegate, object state)
        {
            this.Post(callbackDelegate, state);
        }

        public override void Post(SendOrPostCallback callbackDelegate, object state = null)
        {
            queue.Enqueue(new SynchActivity { CallbackDelegate = callbackDelegate, State = state });
        }

		public void ShutDown()
		{
			CancellationSource?.Cancel();
		}

		public void Run()
        {
            this.Run(null, null);
        }

		public void Run(SendOrPostCallback mainThreadWork, object mainThreadState, CancellationTokenSource cancellationSource = null)
		{
			if (cancellationSource == null)
				CancellationSource = new CancellationTokenSource();
			else
				CancellationSource = cancellationSource;



			try
			{
				CancellationSource.Token.ThrowIfCancellationRequested();

				var currentThreadId = Thread.CurrentThread.ManagedThreadId;
				if (currentThreadId != mainThreadId)
				{
					throw new InvalidOperationException();
				}

				while (true)
				{
					if (mainThreadWork != null)
						mainThreadWork(mainThreadState);


					while (queue.Count > 0)
					{
						if (queue.TryDequeue(out var callbackDelegate))
						{
							callbackDelegate.CallbackDelegate(callbackDelegate.State);
						}
					}

					Thread.Sleep(sleepInterval);
				}
			}
			catch (OperationCanceledException) { }

		}

		~ForgeSynchronizationContext()
		{
			if (saveContext != null) SynchronizationContext.SetSynchronizationContext(saveContext);
		}
	}
}
