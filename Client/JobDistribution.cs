using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PADIMapNoReduce;

namespace Client
{
    public class JobDistribution
    {

        private IList<Job> mAllJobs;
        private int mTotalJobs;
        private int mJobsDone;

        public int TotalJobs
        {
            get { return mTotalJobs; }
            set { mTotalJobs = value;  }
        }

        public int JobsDone
        {
            get { return mJobsDone; }
            set { mJobsDone = value; }
        }

        public JobDistribution() {
            mAllJobs = new List<Job>();
        }

        public IList<Job> GetJobs() {
            return mAllJobs;
        }

        public IList<Job> GetJobs(int workerId) {

            IList<Job> jobs = new List<Job>();

            foreach (Job job in mAllJobs){
                if (job.WorkerId == workerId) {
                    jobs.Add(job);
                }
            }

            return jobs;

        }

        public bool AddJob(int workerId, long beginIndex, long endIndex) {
            mAllJobs.Add(new Job(workerId, beginIndex, endIndex));
            return true;
        }

        public bool AddJob(int workerId, long beginIndex, long endIndex, String result)
        {
            mAllJobs.Add(new Job(workerId, beginIndex, endIndex, result));
            return true;
        }

        public bool UpdateJob(int workerId, String result)
        {
            foreach (Job job in mAllJobs) {
                if (job.WorkerId == workerId && !job.IsDone) {
                    job.Result = result;
                    job.IsDone = true;
                    JobsDone++;
                }
            }

            return true;
        }

        public class Job
        {

            // Fields
            private int mWorkerId;
            private long mBeginIndex;
            private long mEndIndex;
            private String mResult;
            private bool mIsDone;

            // Properties to get fields
            public int WorkerId { get { return mWorkerId; } }

            public long BeginIndex { get { return mBeginIndex; } }

            public long EndIndex { get { return mEndIndex; } }

            public String Result {
                get { return mResult; }
                set { mResult = value; }
            }

            public bool IsDone {
                get { return mIsDone; }
                set { mIsDone = value; }
            }

            /// <summary>
            /// Job constructor
            /// </summary>
            /// <param name="workerId">The Id of the Worker</param>
            /// <param name="beginIndex">The begin index</param>
            /// <param name="endIndex">The end index</param>
            public Job(int workerId, long beginIndex, long endIndex)
            {
                mWorkerId = workerId;
                mBeginIndex = beginIndex;
                mEndIndex = endIndex;
                mResult = "";
            }

            /// <summary>
            /// Job constructor
            /// </summary>
            /// <param name="workerId">The Id of the Worker</param>
            /// <param name="beginIndex">The begin index</param>
            /// <param name="endIndex">The end index</param>
            /// <param name="result">The result of the job</param>
            public Job(int workerId, long beginIndex, long endIndex, String result)
            {
                mWorkerId = workerId;
                mBeginIndex = beginIndex;
                mEndIndex = endIndex;
                mResult = result;
            }

        }

    }

}
