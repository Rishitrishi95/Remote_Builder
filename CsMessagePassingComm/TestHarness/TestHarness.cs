//////////////////////////////////////////////////////////////////////////////
// TestHarness.cs - Demonstrate Build Server operations                     //
// ver 1.0                                                                  //
//                                                                          //
// Author: Rishit reddy Muthyala, rmuthyala@syr.edu                         //
// Application: CSE681 Project 4-TestHarness                                 //
// Environment: C# console                                                  //
//////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
* This is responsible for testing dll files
* this runs on 8090 port
* this uses reflection
* 

* Required Files: 
* --------------- 
*  MessagePassingCommService, ImessagePassingCommService,Environment,TestUtilities, XmlParser
*
* 
*  
* Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
*  
*/
using BuildServer;
using Navigator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessagePassingComm
{
    class TestHarness
    {
        Comm testHarness { get; set; } = null;
        public string from { get; set; } = "http://localhost:8082/IMessagePassingComm";
        Thread rcvThread = null;
        public string toAddress { get; set; } = null;
        public string testRequestName { get; set; } = null;
        public static string Author { get; set; } = null;
        public static string TestFolderPath { get; set; } = null;
        public static string logFile { get; set; } = null;
        public static string _DateTime { get; set; } = null;
        /*-----------------------------<Intializex the comm object and reciver thread>---------------------------*/
        private TestHarness()
        {
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting Repository server on 8081 port number", '=');
            testHarness = new Comm("http://localhost", 8082);
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }
        /*-----------------------------<recieving messages thread>---------------------------*/
        private void rcvThreadProc()
        {
            while (true)
            {
                CommMessage recieveMessage = testHarness.getMessage();
                recieveMessage.show();
                if (recieveMessage.command == "testRequest")
                {
                    toAddress = recieveMessage.from;
                    testRequestName = recieveMessage.xmlString;
                    TestFolderPath = "../../../TestStorage";

                    Directory.CreateDirectory(TestFolderPath);

                    ParseTestRequest(recieveMessage.fileName);
                }
            }
        }
        /*-----------------------------<this it parse test request>---------------------------*/
        private void ParseTestRequest(string path)
        {
            XmlParsing xml = new XmlParsing();
            xml.LoadXml(path);
            Author = xml.Parse("author");
            _DateTime = xml.Parse("dateTime");
            String testDriver = xml.Parse("testDriver");
            CommMessage requestFiles = new CommMessage(CommMessage.MessageType.request);
            requestFiles.from = from;
            requestFiles.to = toAddress;
            requestFiles.command = "SendDllFiles";
            requestFiles.xmlString = testDriver;
            requestFiles.fileName = TestFolderPath;
            requestFiles.author = "TestHarness";
            testHarness.postMessage(requestFiles);
            Thread.Sleep(10000);
            Console.WriteLine(" \n Requirement 9:The Test Harness shall attempt to load each test library it receives and execute it. It shall submit the results of testing to the Repository.");
            Console.WriteLine("===================================================================================================================================================================== ");
            Console.Write("\n \n");
            TestProcess(testDriver);

        }
        /*-----------------------------<this is for build process>---------------------------*/
        private void TestProcess(string file)
        {
            if (File.Exists(TestFolderPath + "/" + file))
            {
                string[] Libraries = Directory.GetFiles(TestFolderPath, "*.dll");
                foreach (string library in Libraries)
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(library));
                    Type[] types = assembly.GetExportedTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsClass && !type.IsAbstract)
                        {
                            object obj = Activator.CreateInstance(type);
                            MethodInfo[] methodInfo = type.GetMethods();
                            foreach (MethodInfo method in methodInfo)
                            {
                                if (method.DeclaringType != typeof(object))
                                {
                                    try
                                    {
                                        if (method.GetParameters().Length == 0)
                                        {
                                            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance;
                                            var rvalue = type.InvokeMember(method.Name, bindingFlags, null, obj, null);
                                            if (rvalue.Equals(true))
                                            {
                                                SendTrueMessage();
                                            }
                                            else SendFalseMessage();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Exception:{0}", ex);
                                    }
                                    finally
                                    {
                                        DirectoryInfo di = new DirectoryInfo(TestFolderPath);
                                        foreach (FileInfo f in di.GetFiles())
                                        {
                                            f.Delete();
                                        }
                                    }     }  }   } }   }  }
            sendReadyMessage();

        }
        /*-----------------------------<this is for sending success message to client>---------------------------*/
        private void SendTrueMessage()
        {
            CommMessage trueMessage = new CommMessage(CommMessage.MessageType.request);
            trueMessage.from = from;
            trueMessage.to = "http://localhost:8080/IMessagePassingComm";
            trueMessage.command = "UpdateTestreuslt";
            trueMessage.xmlString = "Test for " + testRequestName + " is SUCCESSFULL";
            trueMessage.author = "TestHarness";
            testHarness.postMessage(trueMessage);

        }
        /*-----------------------------<this is to send failure message to client>---------------------------*/
        private void SendFalseMessage()
        {
            CommMessage falseMessage = new CommMessage(CommMessage.MessageType.request);
            falseMessage.from = from;
            falseMessage.to = "http://localhost:8080/IMessagePassingComm";
            falseMessage.command = "UpdateTestreuslt";
            falseMessage.xmlString = "Test for " + testRequestName + "  Failed";
            falseMessage.author = "TestHarness";
            testHarness.postMessage(falseMessage);

        }
        /*-----------------------------<This is to send ready message to child>---------------------------*/
        private void sendReadyMessage()
        {
            CommMessage message = new CommMessage(CommMessage.MessageType.request);
            message.to = toAddress;
            message.from = from;
            message.command = "sendReadyMessage";
            message.author = "TestHarness";
            testHarness.postMessage(message);

        }
#if (TEST_TH)
        static void Main(string[] args)
        {
            TestHarness th = new TestHarness();
        }
#endif
    }
}
