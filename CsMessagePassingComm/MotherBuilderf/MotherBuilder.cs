//////////////////////////////////////////////////////////////////////////////
// Mother Builder.cs - Demonstrate Build Server operations                     //
// ver 1.0                                                                  //
//                                                                          //
// Author: Rishit reddy Muthyala, rmuthyal@syr.edu                          //
// Application: CSE681 Project 4-MotherBuilder.cs                       //
// Environment: C# console                                                  //
//////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
* Mother Builder is the one core part of build server.
* Mother builder is run on port number 8083 and is responsible for starting child builders on different ports handling multiple request and multiple ready messages from child builders.
* Mother builder sends the build request whenever a child builder is free. 
* It process class to create new process for each child builder assigning port numbers from 8084 and adding +1 for every child builder.


* 
*
* Required Files: 
* --------------- 
*  MessagePassingCommService, ImessagePassingCommService,Environment,TestUtilities.
*
*
*  
*  
* Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
*  
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWTools;
using Navigator;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MessagePassingComm
{
    class MotherBuilder
    {
        public string from { get; set; } = "http://localhost:8083/IMessagePassingComm";// from address
        public static Dictionary<string, int> ChildBuilderPorts = new Dictionary<string, int>();// dictionary to mainatain list of child builders
        private static BlockingQueue<String> build_requests = new BlockingQueue<String>();// queue for storing build request
        private static BlockingQueue<CommMessage> readyqueue = new BlockingQueue<CommMessage>();// queue for storing ready messages
        Comm motherbuilder { get; set; } = null;// comm object
        public static int numberofprocess = 0;//number of process to be created
        /*-----------------------------------------------<Constructor which initialized the comm object and starts threadProc and CheckProc>------------------------------------------------------------*/
        private MotherBuilder()
        {
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting MotherBuilder", '=');
            this.motherbuilder = new Comm("http://localhost", 8083);
            Thread rcvThread = new Thread(ThreadProc);
            rcvThread.Start();
            Thread checkThread = new Thread(checkProc);
            checkThread.Start();
        }
        /*-----------------------------------------------<This is to check whether there are build requests and ready messages from child builder>------------------------------------------------------------*/
        private void checkProc()
        {
            while (true)
            {
                if (readyqueue.size() != 0 && build_requests.size() != 0)
                {
                    CommMessage readyMessage = readyqueue.deQ();
                    String buildRequest = build_requests.deQ();
                    
                    CommMessage buildMessage = new CommMessage(CommMessage.MessageType.reply);
                    buildMessage.to = readyMessage.from;
                    buildMessage.from = this.from;
                    buildMessage.author = "MotherBuilder";
                    buildMessage.command = "BUILD";
                    buildMessage.xmlString = buildRequest;
                    motherbuilder.postMessage(buildMessage);
                    Console.WriteLine(" \n Requirement 3: The Communication Service shall support accessing build requests by Pool Processes from the mother Builder process, sending and receiving build requests, and sending and receiving files");
                    Console.WriteLine("============================================================================================================================================================================================================= ");
                    
                    bool result = true;
                    do
                    {
                        result = motherbuilder.postFile(buildRequest, "../../../RepositoryStorage/", readyMessage.fileName);
                    } while (!result);
                }
            }
        }
        /*-----------------------------------------------<This is reciever thread used for recieveing messages>------------------------------------------------------------*/
        private void ThreadProc()
        {
            
            while (true)
            {
                CommMessage recieve = motherbuilder.getMessage();
                recieve.show();
                if (recieve.command == "startchildbuilder")
                {
                    Console.WriteLine(recieve.arguments[0]);
                    CallChild(Convert.ToInt32(recieve.arguments[0]));
                }
                else if (recieve.command == "StoreBuildRequests")
                {
                    foreach (string f in recieve.arguments)
                    {
                        build_requests.enQ(f);
                    }
                }
                else if (recieve.command == "KillProcess")
                {
                    foreach (int childBuilderport in ChildBuilderPorts.Values)
                    {
                        Console.WriteLine(childBuilderport);
                        CommMessage killMessage = new CommMessage(CommMessage.MessageType.connect);
                        killMessage.author = "MotherBuilder";
                        killMessage.command = "Kill";
                        killMessage.from = this.from;
                        killMessage.to = "http://localhost:" + childBuilderport + "/IMessagePassingComm";
                        motherbuilder.postMessage(killMessage);
                    }

                }
                else if (recieve.command == "READY")
                {
                    Console.WriteLine(" \n Requirement 6:Pool Processes shall use message-passing communication to access messages from the mother Builder process.");
                    Console.WriteLine("=====================================================================================================================================================\n");

                    readyqueue.enQ(recieve);

                }
            }
        }
        /*-----------------------------------------------<This is to create process>------------------------------------------------------------*/
        private static bool createProcess(int i)
        {

            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = @"..\..\..\ChildBuilder\bin\Debug\ChildBuilder.exe";
            //string fileName = @"..\..\..\ChildBuilder\bin\Debug\ChildBuilder.exe";
            string childProcessNo = i.ToString();
            string childBuilderPort = (8083 + i).ToString();
            string motherBuilderPort = "8083";
            string[] args = { childProcessNo, motherBuilderPort, childBuilderPort };
            ChildBuilderPorts.Add("ChildBuilder" + i.ToString(), 8083 + i);
            string commandline = String.Join(" ", args);
                proc.StartInfo.Arguments = commandline;
                proc.Start();
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }

            return true;
        }
        /*-----------------------------------------------<This is to create process>------------------------------------------------------------*/
        private static void CallChild(int count)
        {
            List<string> buildrequests = new List<string>();
            List<string> readytorun = new List<string>();
            //BuildRequest buildreq = new BuildRequest();
            //buildrequests = buildreq.TestFunc();

            for (int i = 1; i <= count; ++i)
            {
                if (createProcess(i))
                {
                    Console.WriteLine(" - succeeded");
                }
                else
                {
                    Console.WriteLine(" - failed");
                }
            }
            foreach (KeyValuePair<string, int> elem in ChildBuilderPorts)
            {

                Console.WriteLine("Key = {0}, Value = {1}", elem.Key, elem.Value);
            }
            Console.Write("\n  Press key to exit");
            Console.Write("\n  ");
            return;
        }
#if (TEST_MB)
        static void Main(string[] args)
        {
            MotherBuilder s = new MotherBuilder();
            
            
        }
#endif
    }
}
