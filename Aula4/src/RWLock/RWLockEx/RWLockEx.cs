using System;
using System.Threading;
using System.Collections.Generic;

namespace RWLock
{
    public class RWLockEx
    {
        private readonly Object _locker = new Object();

        private bool _writing = false;
        private int _numReaders = 0;

        private class Bool { public bool value = false; }

        private Bool _readersWaitObj = new Bool();
        private int _numWaitingReaders = 0;

        private readonly LinkedList<bool> _waitingWriters = new LinkedList<bool>();

        public void EnterRead()
        {
            lock (_locker)
            {
                if (!_writing && _waitingWriters.Count == 0)
                {
                    ++_numReaders;
                    return;
                }

                Bool _waitObj = _readersWaitObj;
                ++_numWaitingReaders;
                try
                {
                    do
                    {
                        Monitor.Wait(_locker);
                    } while (!_waitObj.value);
                }
                catch (ThreadInterruptedException)
                {
                    if (_waitObj.value)
                    {
                        Thread.CurrentThread.Interrupt();
                    }
                    else
                    {
                        --_numWaitingReaders;
                        throw;
                    }
                }
            }
        }

        public void ExitRead()
        {
            lock (_locker) // !!!
            {
                --_numReaders;

                if (_numReaders == 0) {
                    if (_waitingWriters.Count > 0) {
                        LinkedListNode<bool> _nextWriter = _waitingWriters.First;
                        _waitingWriters.RemoveFirst();

                        _nextWriter.Value = true;
                        _writing = true;
                        Monitor.PulseAll(_locker);
                    }
                }
            }
        }

        public void EnterWrite()
        {
            lock (_locker)
            {
                if (!_writing && _numReaders == 0)
                {
                    _writing = true;
                    return;
                }

                LinkedListNode<bool> _waitObj = _waitingWriters.AddLast(false);
                try
                {
                    do
                    {
                        Monitor.Wait(_locker);
                    } while (!_waitObj.Value);
                }
                catch (ThreadInterruptedException)
                {
                    if (_waitObj.Value)
                    {
                        Thread.CurrentThread.Interrupt();
                    }
                    else
                    {
                        _waitingWriters.Remove(_waitObj);
                        if (!_writing && _waitingWriters.Count == 0 && _numWaitingReaders > 0)
                        {
                            _readersWaitObj.value = true;
                            _readersWaitObj = new Bool();
                            _numReaders += _numWaitingReaders;
                            _numWaitingReaders = 0;
                            Monitor.PulseAll(_locker);
                        }
                    }
                }
            }
        }
        public void ExitWrite() { /* ... */ }
    }

    public class UseRWLockEx
    {
        static void Main(string[] args)
        {

        }
    }
}
