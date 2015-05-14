using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace PADIMapNoReduce {

    [Serializable]
    public class WorkStatus : ICloneable {

        public enum Status { GettingInput, FetchingInput, Computing, SendingOutput, Idle };

        /// <summary>
        /// Worker responsible of this split
        /// </summary>
        private int workerId = -1;

        /// <summary>
        /// Status of the current worker in its split
        /// </summary>
        private Status myStatus = Status.Idle;

        /// <summary>
        /// Number of lines computed
        /// </summary>
        private int numberLinesComputed = -1;

        /// <summary>
        /// Total number of lines to compute
        /// </summary>
        private int totalNumberOfLines = 0;

        /// <summary>
        /// Last time the split sent new computation
        /// </summary>
        private DateTime lastModification;

        /// <summary>
        /// The begin index of the split working on
        /// </summary>
        private long beginIndexOfSplit = 0;

        /// <summary>
        /// The end index of the split working on
        /// </summary>
        private long endIndexOfSplit = 0;

        /// <summary>
        /// Empty Constructor
        /// </summary>
        /// 
        public WorkStatus() {
            lastModification = DateTime.Now;
            myStatus = Status.Idle;
        }

        /// <summary>
        /// Creates an instance with a specific begin and end index for the split
        /// </summary>
        /// <param name="beginIndex">Begin of the input Split</param>
        /// <param name="endIndex">End of the input Split</param>
        /// 
        public WorkStatus(long beginIndex, long endIndex) : base() {

            beginIndexOfSplit = beginIndex;
            endIndexOfSplit = endIndex;

        }

        /// <summary>
        /// Changes the status of the worker
        /// </summary>
        /// <param name="newStatus">New Status</param>
        /// 
        public void changeStatus(Status newStatus) { myStatus = newStatus; }

        /// <summary>
        /// Gets the status of the worker
        /// </summary>
        /// <returns>The worker status</returns>
        /// 
        public Status getStatus() { return myStatus; }

        /// <summary>
        /// Set the begin and end indexes of the split
        /// </summary>
        /// <param name="beginIndex">The split being</param>
        /// <param name="endIndex">The split end</param>
        public void setSplitIndexes(long beginIndex, long endIndex) {

            beginIndexOfSplit = beginIndex;
            endIndexOfSplit = endIndex;
        }

        /// <summary>
        /// Get begin index of the split
        /// </summary>
        /// <returns>The begin index of the split</returns>
        public long getBeginIndex() {

            return beginIndexOfSplit;
        }

        /// <summary>
        /// Get end index of the split
        /// </summary>
        /// <returns>The end index of the split</returns>
        public long getEndIndex() {

            return endIndexOfSplit;
        }

        /// <summary>
        /// Sets the total number of lines of the split
        /// </summary>
        /// <param name="totalNumberOfLines">number of lines to compute</param>
        public void setTotalNumberOfLines(int totalNumberOfLines) {

            this.totalNumberOfLines = totalNumberOfLines;
        }

        /// <summary>
        /// Sets the number of lines computed
        /// </summary>
        /// <param name="totalNumberOfLines">number of lines computed</param>
        public void setNumberLinesComputed(int numberOfLinesComputed) {

            this.numberLinesComputed = numberOfLinesComputed;
        }

        /// <summary>
        /// Gets the number of lines computed
        /// </summary>
        /// <returns>the number of lines computed</returns>
        public int getNumberLinesComputed() {

            return this.numberLinesComputed;
            
        }

        /// <summary>
        /// Gets the total number of lines to compute
        /// </summary>
        /// <returns>the number of lines to compute</returns>
        public int getTotalNumberLines() {

            return this.totalNumberOfLines;

        }

        /// <summary>
        /// Get the last modification time
        /// </summary>
        /// <returns>the time of the last modification</returns>
        public DateTime getLastModification() {

            return this.lastModification;

        }

        /// <summary>
        /// Set the last modification time
        /// </summary>
        /// <param name="lastModification">the last modification time</param>
        public void setLastModification(DateTime lastModification) {

            this.lastModification = lastModification;

        }

        /// <summary>
        /// Sets the worker responsible for this split
        /// </summary>
        /// <param name="workerId">worker responsible for this split</param>
        public void setWorkerId(int workerId) {

            this.workerId = workerId;
        }

        /// <summary>
        /// Sets the worker responsible for this split
        /// </summary>
        /// <returns> returns the worker responsible for this split</returns>
        public int getWorkerId() {

            return this.workerId;

        }

        /// <summary>
        /// Is this work split assigned to any worker
        /// </summary>
        /// <returns>returns true if the worker is already assigned, false otherwise</returns>
        public bool isWorkerAssigned() {

            return workerId != -1;

        }

        /// <summary>
        /// Is all the work computed
        /// </summary>
        /// <returns>returns true if all the lines were computed, false otherwise</returns>
        public bool isWorkCompleted() {

            return totalNumberOfLines == numberLinesComputed;
        }

        public void removeWorker() {

            this.workerId = -1;
        }

        /// <summary>
        /// Deep clone of this object
        /// </summary>
        /// <returns>new copied object</returns>
        public object Clone() {

            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;

        }
    
    }
}
