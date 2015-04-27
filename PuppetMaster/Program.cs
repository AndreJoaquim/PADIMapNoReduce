using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;

namespace PuppetMaster
{
    class Program
    {

        public const string WORKER = "WORKER";
        public const string SUBMIT = "SUBMIT";
        public const string WAIT = "WAIT";
        public const string STATUS = "STATUS";
        public const string SLOWW = "SLOWW";
        public const string FREEZEW = "FREEZEW";
        public const string UNFREEZEW = "UNFREEZEW";
        public const string FREEZEC = "FREEZEC";
        public const string UNFREEZEC = "UNFREEZEC";

        static void Main(string[] args)
        {

            IPuppetMaster puppetMaster = new PuppetMasterImplementation();

            while (true) {

                string input = Console.ReadLine();
                string[] split = input.Split(' ');

                string command = split[0];

                switch (command) { 
                
                    case WORKER:

                        /*
                         * WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>
                         */

                        if(split.Length <= 5){

                            string id = split[1];
                            string puppetMasterUrl = split[2];
                            string serviceUrl = split[3];

                            if (split.Length == 5) {
                                string entryUrl = split[4];
                                puppetMaster.CreateWorker(id, puppetMasterUrl, serviceUrl, entryUrl);
                            } else {
                                puppetMaster.CreateWorker(id, puppetMasterUrl, serviceUrl);
                            }

                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>");
                        }

                        break;

                    case SUBMIT:

                        /*
                         * SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> 
                         */

                        if (split.Length == 6)
                        {

                            string entryUrl = split[1];
                            string file = split[2];
                            string output = split[3];
                            string s = split[4];
                            string map = split[5];

                            puppetMaster.SubmitJob(entryUrl, file, output, s, map);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP>");
                        }

                        break;

                    case WAIT:

                        /*
                          * WAIT <SECS> 
                          */

                        if (split.Length == 6)
                        {

                            int secs = Int32.Parse(split[1]);
                            puppetMaster.Wait(secs);

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: WAIT <SECS>");
                        }

                        break;

                    case STATUS:

                        /*
                          * STATUS 
                          */

                        puppetMaster.Status();

                        break;

                    case SLOWW:

                        /*
                          * SLOWW <ID> <delay-in-seconds>
                          */

                        if (split.Length == 6)
                        {

                            string id = split[1];
                            

                        }
                        else
                        {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: WAIT <SECS>");
                        }


                        break;
                
                }

            }

        }

    }
}
