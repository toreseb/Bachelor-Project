using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Inspired by https://stackoverflow.com/questions/7330834/how-to-check-the-state-of-a-semaphore

namespace Bachelor_Project.Utility
{
    public class UsefullSemaphore
    {
        private int count = 0;
        private int limit = 0;
        private object locker = new object();

        public UsefullSemaphore(int initialCount, int maximumCount)
        {
            count = initialCount;
            limit = maximumCount;
        }

        public UsefullSemaphore(int maximumCount) 
        {
            count = 0;
            limit = maximumCount;
        }

        public void WaitOne()
        {
            lock (locker)
            {
                while (count == 0)
                {
                    Monitor.Wait(locker);
                }
                count--;
            }
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
        public void Check()
        {
            lock (locker)
            {
                while (count == 0)
                {
                    Monitor.Wait(locker);
                }
            }
        }

        public bool TryReleaseOne()
        {
            lock (locker)
            {
                if (count < limit)
                {
                    count++;
                    Monitor.PulseAll(locker);
                    return true;
                }
                return false;
            }
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
