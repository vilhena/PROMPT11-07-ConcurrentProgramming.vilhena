using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RWLock
{
    public class RWLock
    {
        private bool _writing = false;
        private int _readCount = 0;
        private int _writerWaitCount = 0;

        // Acquire read (shared) access
        public void EnterRead()
        {
            lock (this)
            {
                while (_writing || _writerWaitCount > 0)
                {
                    this.Wait();
                }
                _readCount++;
            }
        }

        // Acquire write (exclusive) access
        public void EnterWrite()
        {
            lock (this)
            {
                _writerWaitCount++;
                try
                {
                    while (_readCount > 0 || _writing)
                    {
                        this.Wait();
                    }
                }
                catch(ThreadInterruptedException)
                {
                    _writerWaitCount--;

                    if (_readCount == 0 && !_writing)
                        this.PulseAll();

                    throw;
                }
                _writerWaitCount--;
                _writing = true;
            }
        }

        // Release read (shared) access
        public void ExitRead()
        {
            this.SafeLock(() =>
                              {
                                  _readCount--;
                                  if (_readCount == 0)
                                  {
                                      this.PulseAll();
                                  }
                              });
        }

        // Release write (exclusive) access
        public void ExitWrite()
        {
            this.SafeLock(() =>
                              {
                                  _writing = false;
                                  Monitor.PulseAll(this);
                              });

        }
    }
}
