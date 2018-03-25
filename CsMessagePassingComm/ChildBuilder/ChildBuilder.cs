//////////////////////////////////////////////////////////////////////////////
// ChildBUilder.cs - Demonstrate Build Server operations                     //
// ver 1.0                                                                  //
//                                                                          //
// Author: Rishit Reddy Muthyala, rmuthyal@syr.edu                          //
// Application: CSE681 Project 4-ChildBuilder                               //
// Environment: C# console                                                  //
//////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
* Child builder is responsible for actual build process. 
* It uses csc command to build the files and builds log files. 
* Then creates a test request xml file and sends it to test harness.
* It gets killed when receive a kill command from the client.
 
*  
* Required Files: 
* --------------- 
* MessagePassingCommService, ImessagePassingCommService,Environment,TestUtilities, XmlParser
*
* 
* Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
*  
*/

using Navigator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuildServer;
using System.Xml.Linq;

namespace MessagePassingComm
{
    class ChildBuilder
    {
        public static int childBuilderPort { get; set; } = 0;// for childer builder port
        public static int motherBuilderPort { get; set; } = 0;// for mother builder port
        public static Comm childbuilder { get; set; } = null;// for child builder comm
        public static string BuildFolderPath { get; set; } = null;// for build folder path
        public static string Author { get; set; } = null;// for author
        public static string logFile { get; set; } = null;// for logfile
        public static string _DateTime { get; set; } = null;// for datetime
        public static string BuildrequestName { get; set; } = null;// for build request name
        public static Thread rcvThread { get; set; } = null;// for recive thread
        public static List<string> tested { get; set; } = new List<string>();// for tested files

    /*-------------------------------------------<For recieving messages it always runs till killed>------------------------------------------*/   
       private  static void ThreadProc()
        {
           
            bool result = true;
            while (result)
            { 
                CommMessage commMessage = childbuilder.getMessage();
                commMessage.show();
                if (commMessage.command == "BUILD")
                {
                    BuildrequestName = commMessage.xmlString;
                    startBuildProcess(commMessage.xmlString);
                }
                if(commMessage.command=="Kill")
                {
                    result = false;
                }
                if(commMessage.command== "SentFiles")
                {
                    BuildProcess();
                }
                if(commMessage.command== "SendDllFiles")
                {
                    bool check= true;
                    do
                    {
                        check=childbuilder.postFile(commMessage.xmlString, BuildFolderPath + "/", commMessage.fileName + "/");
                    } while (!check);
                }
                if(commMessage.command== "sendReadyMessage")
                {
                    ReadyMessage();
                }
            }
        }
        /*-------------------------------------------<it is to start build process here parsing happens>------------------------------------------*/
        private static void startBuildProcess(string buildRequest)
        {
            
            
            XmlParsing xml = new XmlParsing();
            xml.LoadXml("../../../RepositoryStorage/" + buildRequest);
             Author = xml.Parse("author");
            _DateTime = xml.Parse("dateTime");
            tested = xml.ParseList("tested");
                String testDriver = xml.Parse("testDriver");      
                tested.Add(testDriver);
           

            CommMessage requestFiles = new CommMessage(CommMessage.MessageType.request);
            requestFiles.author = "ChildBuilder";
            requestFiles.command = "RequestParsedFiles";
            requestFiles.fileName = BuildFolderPath + "/";
            requestFiles.from = "http://localhost:" + childBuilderPort.ToString() + "/IMessagePassingComm";
            requestFiles.to = "http://localhost:8081/IMessagePassingComm";
            requestFiles.arguments = tested;
            childbuilder.postMessage(requestFiles);




        }
        /*-------------------------------------------<Here the build of files happen>------------------------------------------*/
        private static void BuildProcess()
        {
            try
            {
                List<string> wholePath = new List<string>();
                foreach (string f in tested)
                {
                    string fullPath = Path.GetFullPath(BuildFolderPath+"//" + f);
                    wholePath.Add(fullPath);
                }
                string parameters = "/Ccsc.exe /target:library /out:";
                int i = BuildrequestName.IndexOf('.');
                string dllname = BuildrequestName.Substring(0, i) + ".dll";
                parameters +=dllname ;
                foreach (string fin in wholePath)
                {
                    parameters += " " + fin;
                }
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = parameters;
                p.StartInfo.WorkingDirectory = BuildFolderPath;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
                string errors = p.StandardError.ReadToEnd();
                string output = p.StandardOutput.ReadToEnd();
                Console.WriteLine(errors);
                Console.WriteLine(output);
               
                if(File.Exists(BuildFolderPath+"/"+dllname))
                {
                    Console.WriteLine("\nSUCCESS\n");
                    Console.WriteLine(" \n Requirement 8: If the build succeeds, shall send a test request and libraries to the Test Harness for execution, and shall send the build log to the repository");
                    Console.WriteLine("===================================================================================================================================================================== ");
                    Console.Write("\n \n");
                    
                    BuildTestRequest();
                    
                }
                else
                {
                    Console.WriteLine("\nFAIL\n");
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
            }
            ReadyMessage();
        }
        /*-------------------------------------------< It is to build test request after success of build>------------------------------------------*/
        private static void BuildTestRequest()
        {
            XDocument xml = new XDocument();

            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XComment comment = new XComment("Created Test request from selected files");
            xml.Add(comment);
            XElement testRequest = new XElement("testRequest");
            xml.Add(testRequest);
            XElement child1 = new XElement("author", "Rishit Reddy");
            XElement child2 = new XElement("dateTime", DateTime.Now.ToString());
            XElement child3 = new XElement("test");
            int i = BuildrequestName.IndexOf('.');
            XElement grandchild1 = new XElement("testDriver", BuildrequestName.Substring(0,i)+".dll");
            child3.Add(grandchild1);
            testRequest.Add(child1);
            testRequest.Add(child2);
            testRequest.Add(child3);
            String path = BuildFolderPath + "/testRequest_" + BuildrequestName.Substring(0, i) + ".xml";
            int count = 1;
            while (File.Exists(path))
            {
                path = BuildFolderPath + "/testRequest_" + BuildrequestName.Substring(0, i) + count.ToString() + ".xml";
                count++;
            }
            xml.Save(path);
            CommMessage send = new CommMessage(CommMessage.MessageType.request);
            send.author = "ChildBuilder";
            send.from= "http://localhost:" + childBuilderPort.ToString() + "/IMessagePassingComm";
            send.to = "http://localhost:8082/IMessagePassingComm";
            send.fileName = path;
            send.xmlString = BuildrequestName;
            send.command = "testRequest";
            childbuilder.postMessage(send);
        }
        /*-------------------------------------------<ready message sent to mother builder whenever child starts and whenevr it is ready>------------------------------------------*/
        public static void ReadyMessage()
        {
            CommMessage send = new CommMessage(CommMessage.MessageType.request);
            send.from = "http://localhost:" + childBuilderPort.ToString() + "/IMessagePassingComm";
            send.to = "http://localhost:8083/IMessagePassingComm";
            send.author = "Rishit Reddy";
            send.command = "READY";
            send.fileName = BuildFolderPath + "/";
            
            childbuilder.postMessage(send);
        }

        static void Main(string[] args)
        {


            ChildBuilder childBuilder = new ChildBuilder();
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting ChildBuilder", '=');

            int j = 0;
            string[] commandlineArgs = new string[3];
            foreach (string command in args)
            {

                commandlineArgs[j] = command;
                j++;
            }
            Console.Title = "ChildProc";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            Console.Write("\n  Child Builder Initialized");
            Console.Write("\n ====================");
            motherBuilderPort = Convert.ToInt32(commandlineArgs[1]);
            childBuilderPort = Convert.ToInt32(commandlineArgs[2]);
            Console.Write("\n  Hello from childbuilder #{0}\n\n", commandlineArgs[0]);
            BuildFolderPath = "../../../BuildStorage/" + "buildstorage_" + commandlineArgs[0].ToString();
            
                Directory.CreateDirectory(BuildFolderPath);
            
            Console.WriteLine(childBuilderPort);
            childbuilder = new Comm("http://localhost", childBuilderPort);

            ReadyMessage();
            rcvThread = new Thread(ThreadProc);
            rcvThread.Start();

        }
    }
}
