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
        
        /// <summary>
        /// <see cref="Wait(int)"/> on the <see cref="UsefulSemaphore"/> with a amount of one.
        /// </summary>
        public void WaitOne()
        {
            Wait(1);
        }

        /// <summary>
        /// Waits on the <see cref="UsefulSemaphore"/> while it has less space than the amount <paramref name="i"/>. 
        /// </summary>
        /// <param name="i"></param>
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
        /// <see cref="Check(int)"/> the <see cref="UsefulSemaphore"/> with a value of one.
        /// </summary>
        public void CheckOne()
        {
            Check(1);
        }

        /// <summary>
        /// Waits until <see cref="UsefulSemaphore"/> is has space for the amount <see cref="i"/>, but does not actually subtract from the <see cref="UsefulSemaphore"/> when allowed.
        /// </summary>
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

        /// <summary>
        /// <see cref="TryRelease(int)"/> on the <see cref="UsefulSemaphore"/> with an amount of 1.
        /// </summary>
        /// <returns></returns>
        public bool TryReleaseOne()
        {
            return TryRelease(1);
        }

        /// <summary>
        /// Tries to instert amount <paramref name="i"/> into the <see cref="UsefulSemaphore"/> but only succeeds if there is space for <paramref name="i"/>.
        /// </summary>
        /// <param name="i"></param>
        /// <returns><see langword="true"/> if it successfully releases.</returns>
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
