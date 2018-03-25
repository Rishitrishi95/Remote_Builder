//////////////////////////////////////////////////////////////////////////////
// Testexecutive.cs - Demonstrate Build Server operations                     //
// ver 1.0                                                                  //
//                                                                          //
// Author: Rishit reddy Muthyala, rmuthyala@syr.edu                         //
// Application: CSE681 Project 4-TestHarness                                 //
// Environment: C# console                                                  //
//////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
*This is to demonstarate requirements
* 

* Required Files: 
* --------------- 
*  MessagePassingCommService, ImessagePassingCommService,Environment,TestUtilities
*
* 
*  
* Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
*  
*/
using MessagePassingComm;
using Navigator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestExecutive
{
    class Executive
    {
        Comm executive { get; set; } = null;
        public string from { get; set; } = "http://localhost:8090/IMessagePassingComm";
        /*---------------------------------<This is to initialize heading>------------------------------*/
        
        public Executive()
        {

            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting test Executive at port 8090", '=');
            
        }
        /*---------------------------------<This is to demonstrate requirement 7,13,>------------------------------*/
        private void DemonstrateReq7()
        {
            Console.WriteLine(" \n Requirement 7:Each Pool Process shall attempt to build each library, cited in a retrieved build request, logging warnings and errors.");
            Console.WriteLine("=====================================================================================================================================================\n");
            Console.WriteLine(" \n Requirement 13:The client shall be able to request the repository to send a build request in its storage to the Build Server for build processing");
            Console.WriteLine("=====================================================================================================================================================\n");
            Thread.Sleep(2000);
            List<string> selectedFiles = new List<string>();
            selectedFiles.Add("BuildRequest.xml");
            selectedFiles.Add("BuildRequest16.xml");
            selectedFiles.Add("BuildRequest5.xml");
            CommMessage sendBuildRequests = new CommMessage(CommMessage.MessageType.reply);
            sendBuildRequests.from = this.from;
            sendBuildRequests.to = "http://localhost:8081/IMessagePassingComm";
            sendBuildRequests.command = "sendBuildRequest";
            sendBuildRequests.author = "client";
            sendBuildRequests.arguments = selectedFiles;
            executive.postMessage(sendBuildRequests);
            Thread.Sleep(2000);
        }
        /*---------------------------------<This is to demonstrate requirement 6>------------------------------*/
        private void DemonstrateReq6()
        {
            Console.WriteLine(" \n Requirement 6:Pool Processes shall use message-passing communication to access messages from the mother Builder process.");
            Console.WriteLine("=====================================================================================================================================================\n");
            Console.WriteLine("\n we can see that communication between mother builder and child builder is happening through message passsing communication check the Mother builder Console where ypu can see that \n");
        }
        /*---------------------------------<This is to demonstrate requirement 5>------------------------------*/
        private void DemostrateReq5()
        {
            Console.WriteLine(" \n Requirement 5:Shall provide a Process Pool component that creates a specified number of processes on command.");
            Console.WriteLine("=====================================================================================================================================================\n");
            List<string> number = new List<string>();
            int i = 2;
            number.Add(i.ToString());
            CommMessage numberOfprocessMessage = new CommMessage(CommMessage.MessageType.request);
            numberOfprocessMessage.from = this.from;
            numberOfprocessMessage.to = "http://localhost:8083/IMessagePassingComm";
            numberOfprocessMessage.command = "startchildbuilder";
            numberOfprocessMessage.author = "client";
            numberOfprocessMessage.arguments = number;
            executive.postMessage(numberOfprocessMessage);
            Console.WriteLine("\n You can see that 2 process are created \n ");
            Thread.Sleep(2000);
        }
        /*---------------------------------<This is to demonstrate requirement 4,11,12>------------------------------*/
        private void DemonstrateReq4()
        {
            Console.WriteLine(" \n Requirement 4: Shall provide a Repository server that supports client browsing to find files to build, builds an XML build request string and sends that and the cited files to the Build Server.");
            Console.WriteLine("=====================================================================================================================================================\n");

            Console.WriteLine(" \n Requirement 11:The GUI client shall be a separate process, implemented with WPF and using message-passing communication. It shall provide mechanisms to get file lists from the Repository, and select files for packaging into a test library1, e.g., a test element specifying driver and tested files, added to a build request structure. It shall provide the capability of repeating that process to add other test libraries to the build request structure.");
            Console.WriteLine("========================================================================================================================================================================================================================================================================================================================================================================================================================================================================\n");
            CommMessage requestRepositoryFiles = new CommMessage(CommMessage.MessageType.request);
            requestRepositoryFiles.author = "Executive";
            requestRepositoryFiles.command = "RequestRepositoryFiles";
            requestRepositoryFiles.from = from;
            requestRepositoryFiles.to = "http://localhost:8081/IMessagePassingComm";
            requestRepositoryFiles.errorMsg = null;
            executive.postMessage(requestRepositoryFiles);
            Thread.Sleep(2000);
            Console.WriteLine("\n You can see that Repository folder gets populated\n ");
            CommMessage requestRepositoryXMLFiles = new CommMessage(CommMessage.MessageType.request);
            requestRepositoryXMLFiles.author = "Executive";
            requestRepositoryXMLFiles.command = "RequestRepositoryXMLFiles";
            requestRepositoryXMLFiles.from = from;
            requestRepositoryXMLFiles.to = "http://localhost:8081/IMessagePassingComm";
            requestRepositoryXMLFiles.errorMsg = null;
            executive.postMessage(requestRepositoryXMLFiles);
            Console.WriteLine("\n You can see that Repository xml folder gets populated \n");

            Console.WriteLine(" \n Requirement 12:The client shall send build request structures to the repository for storage and transmission to the Build Server.");
            Console.WriteLine("=====================================================================================================================================================\n");
            Thread.Sleep(2000);
            List<string> selectedFiles = new List<string>();
            selectedFiles.Add("TestDriver.cs");
            selectedFiles.Add("Tested1.cs");
            CommMessage buildRequestMessage = new CommMessage(CommMessage.MessageType.request);
            buildRequestMessage.from = from;
            buildRequestMessage.to = "http://localhost:8081/IMessagePassingComm";
            buildRequestMessage.errorMsg = null;
            buildRequestMessage.author = "Client";
            buildRequestMessage.command = "BuildRequest";
            buildRequestMessage.arguments = selectedFiles;
            executive.postMessage(buildRequestMessage);
            Console.WriteLine("\n You can see that build request is build and path of the file on gui gets populated \n");
            Thread.Sleep(2000);

        }
#if (TEST_EXEC)
        static void Main(string[] args)
        {
            Executive exec = new Executive();
            Console.WriteLine(" \n Requirement 1: This Project is prepared using C#, the .Net Frameowrk, and Visual Studio 2017. ");
            Console.WriteLine("============================================================================================== ");
            Console.Write("\n \n");
            Console.WriteLine("\n Requirement 2: Shall include a Message-Passing Communication Service built with WCF. It is expected that you will build on your Project #3 Comm Prototype.");
            Console.WriteLine("=====================================================================================================================================================");
            Console.Write("\n \n");
            Console.WriteLine("\n Requirement 10: Shall include a Graphical User Interface, built using WPF.");
            Console.WriteLine("=====================================================================================================================================================");
            exec.executive = new Comm("http://localhost", 8090);
            Thread.Sleep(5000);
            exec.DemonstrateReq4();
            Thread.Sleep(2000);
            exec.DemostrateReq5();
            Thread.Sleep(2000);
            exec.DemonstrateReq6();
            Thread.Sleep(2000);
            exec.DemonstrateReq7();


        }
#endif
    }
}
