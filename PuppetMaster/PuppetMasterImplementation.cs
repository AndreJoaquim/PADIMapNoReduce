using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;

namespace PuppetMaster
{
    class PuppetMasterImplementation : MarshalByRefObject, IPuppetMaster
    {

        public bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl)
        {
            return true;
        }

        public bool CreateWorker(string id, string puppetMasterUrl, string serviceUrl, string entryUrl)
        {
            return true;
        }

        public bool SubmitJob(string entryUrl, string file, string output, string s, string map) { return true; }

        public bool Wait(int seconds) { return true; }

        public bool Status() { return true; }

        public bool SlowW(string id, int delayInSeconds) { return true; }

        public bool FrezeeW(string id) { return true; }

        public bool UnfreezeW(string id) { return true; }

        public bool FreezeC(string id) { return true; }

        public bool UnfreezeC(string id) { return true; }

    }
}
