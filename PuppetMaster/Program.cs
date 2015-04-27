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

        public static const String WORKER = "WORKER";
        public static const String SUBMIT = "SUBMIT";
        public static const String WAIT = "WAIT";

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
                                createWorker(id, puppetMasterUrl, serviceUrl, entryUrl);
                            } else {
                                createWorker(id, puppetMasterUrl, serviceUrl);
                            }


                        } else {
                            // Input error, it has more than 4 arguments
                            Console.WriteLine("Wrong usage. Usage: WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>");
                        }

                        break;
                
                }

            }

        }

    }
}
