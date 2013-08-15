﻿using System;
using System.Threading;
using ServiceStack.Logging;

namespace HangFire
{
    internal class JobDispatcher
    {
        private readonly JobDispatcherPool _pool;
        private readonly JobProcessor _processor = new JobProcessor();
        private readonly Thread _thread;
        private readonly ManualResetEventSlim _jobIsReady 
            = new ManualResetEventSlim(false);

        private readonly ILog _logger = LogManager.GetLogger(typeof(JobDispatcher));

        private volatile string _currentJob;

        public JobDispatcher(JobDispatcherPool pool, string name)
        {
            _pool = pool;
            
            _thread = new Thread(DoWork)
                {
                    Name = name,
                    IsBackground = true
                };
            _thread.Start();
        }

        public void Process(string serializedJob)
        {
            _currentJob = serializedJob;
            _jobIsReady.Set();
        }

        private void DoWork()
        {
            while (true)
            {
                _pool.NotifyReady(this);
                _jobIsReady.Wait();

                try
                {
                    _processor.ProcessJob(_currentJob);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        "Failed to process the job: unexpected exception caught. Job JSON:"
                        + Environment.NewLine
                        + _currentJob,
                        ex);
                    _pool.NotifyFailed(_currentJob, ex);

                }
                finally
                {
                    _jobIsReady.Reset();
                }
            }
        }
    }
}