using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Inspired by https://stackoverflow.com/questions/7330834/how-to-check-the-state-of-a-semaphore

namespace Bachelor_Project.Utility
{
    public class UsefulSemaphore
    {
        private int count = 0;
        private int limit = 0;
        private object locker = new object();

        public UsefulSemaphore(int initialCount, int maximumCount)
        {
            count = initialCount;
            limit = maximumCount;
        }

        public UsefulSemaphore(int maximumCount) 
        {
            count = 0;
            limit = maximumCount;
        }

        public void WaitOne()
        {
            Wait(1);
        }

        public void Wait(int i)
        {
            lock (locker)
            {
                while (count-i < 0)
                {
                    Monitor.Wait(locker);
                }
                count-= i;
            }
        }
        /// <summary>
        /// Waits until semaphore is free, but does not actually subtract from the semaphore
        /// </summary>
        public void CheckOne()
        {
            Check(1);
        }

        public void Check(int i)
        {
            lock (locker)
            {
                while (count-i < 0)
                {
                    Monitor.Wait(locker);
                }
            }
        }

        public bool TryReleaseOne()
        {
            return TryRelease(1);
        }

        public bool TryRelease(int i)
        {
            lock (locker)
            {
                if (count+i <= limit)
                {
                    count+=i;
                    Monitor.PulseAll(locker);
                    return true;
                }
                return false;
            }
        }
    }
}
