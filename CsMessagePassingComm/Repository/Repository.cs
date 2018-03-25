//////////////////////////////////////////////////////////////////////////////
// Repository.cs - Demonstrate Build Server operations                     //
// ver 1.0                                                                  //
//                                                                          //
// Author: Rishit Reddy Muthyala, rmuthya@syr.edu                           //
// Application: CSE681 Project 4-Repository                                  //
// Environment: C# console                                                  //
//////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
* Repository is responsible for building request 
* it is also responsible for sending files to client and child builders
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
using Navigator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace MessagePassingComm
{
    class Repository
    {
        Comm repo { get; set; } = null;
        public string from { get; set; } = "http://localhost:8081/IMessagePassingComm";
        Thread rcvThread = null;
        List<String> filesList { get; set; } = new List<string>();
        List<String> filesXMLList { get; set; } = new List<string>();
        /*----------------------------------------------------<Constructor to intialize comm object and threadproc>--------------------------------------------*/
        private Repository()
        {
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting Repository server on 8081 port number", '=');
            repo = new Comm("http://localhost", 8081);
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();

        }
        /*----------------------------------------------------<This is the recieving thread>--------------------------------------------*/
        private void rcvThreadProc()
        {
            while(true)
            {
                CommMessage repoMsg = repo.getMessage();
                repoMsg.show();
                if (repoMsg.command == null)
                    continue;
                if(repoMsg.command== "RequestRepositoryFiles")
                {
                    CommMessage replyRepositoryFiles = new CommMessage(CommMessage.MessageType.reply);
                    replyRepositoryFiles.command = "getRepositoryFiles";
                    replyRepositoryFiles.from = "http://localhost:8081/IMessagePassingComm";
                    replyRepositoryFiles.to = "http://localhost:8080/IMessagePassingComm";
                    replyRepositoryFiles.author = "repository";
                    replyRepositoryFiles.arguments = getRepoFiles();
                    repo.postMessage(replyRepositoryFiles);
                }
                if (repoMsg.command == "RequestRepositoryXMLFiles")
                {
                    CommMessage replyRepositoryXMLFiles = new CommMessage(CommMessage.MessageType.reply);
                    replyRepositoryXMLFiles.command = "getRepositoryXMLFiles";
                    replyRepositoryXMLFiles.from = "http://localhost:8081/IMessagePassingComm";
                    replyRepositoryXMLFiles.to = "http://localhost:8080/IMessagePassingComm";
                    replyRepositoryXMLFiles.author = "repository";
                    replyRepositoryXMLFiles.arguments = getRepoXMLFiles();
                    repo.postMessage(replyRepositoryXMLFiles);
                }
                if(repoMsg.command== "sendBuildRequest")
                {
                    
                    sendBuildRequestToBuildQueue(repoMsg.arguments);
                }
                if(repoMsg.command== "BuildRequest")
                {
                   
                   string path= generateXmlFile(repoMsg.arguments);
                    CommMessage replMessage = new CommMessage(CommMessage.MessageType.reply);
                    replMessage.from = from;
                    replMessage.to = "http://localhost:8080/IMessagePassingComm";
                    replMessage.command = "RequestBuild";
                    replMessage.xmlString = path;
                    replMessage.author = "Repository";
                    repo.postMessage(replMessage);
                }
                if(repoMsg.command== "RequestParsedFiles")
                {
                    SendFilesToChild(repoMsg.arguments, repoMsg.fileName);
                    CommMessage replymsg = new CommMessage(CommMessage.MessageType.reply);
                    replymsg.from = repoMsg.to;
                    replymsg.to = repoMsg.from;
                    replymsg.author = "Repository";
                    replymsg.command = "SentFiles";
                    repo.postMessage(replymsg);
                }
            }
        }
        /*----------------------------------------------------<this is to send repo files to client>--------------------------------------------*/
        private void SendFilesToChild(List<string> files, string path)
        {
            foreach(string s in files)
            {
                bool tester = true;
                do
                {
                    tester = repo.postFile(s, "../../../RepositoryStorage/", path);
                } while (!tester);
            }
            
        }
        /*----------------------------------------------------<this is to send xml files to build>--------------------------------------------*/
        private string generateXmlFile(List<String> selectedFiles)
        {
            XDocument xml = new XDocument();
            String path = @"../../../RepositoryStorage/" + "BuildRequest" + ".xml";
            if (selectedFiles.Count > 0)
            {

                xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                XComment comment = new XComment("CreatedBuild request from selected files");
                xml.Add(comment);
                XElement testRequest = new XElement("testRequest");
                xml.Add(testRequest);
                XElement child1 = new XElement("author", "Rishit Reddy");
                XElement child2 = new XElement("dateTime", DateTime.Now.ToString());
                XElement child3 = new XElement("test");
                XElement grandchild1 = new XElement("testDriver", selectedFiles[0]);
                child3.Add(grandchild1);
                for (int i = 1; i < selectedFiles.Count(); i++)
                {
                    XElement grandchild2 = new XElement("tested", selectedFiles[i]);
                    child3.Add(grandchild2);
                }
                testRequest.Add(child1);
                testRequest.Add(child2);
                testRequest.Add(child3);
                int count = 1;
                while (File.Exists(path))
                {
                    path = @"../../../RepositoryStorage/" + "BuildRequest" + count.ToString() + ".xml";
                    count++;
                }
                xml.Save(path); 
            }
            return path;
        }
        /*----------------------------------------------------<this is to send build request to mother builder>--------------------------------------------*/
        private void sendBuildRequestToBuildQueue(List<String> selectedXmlFiles)
        {
                CommMessage buildrequestname = new CommMessage(CommMessage.MessageType.request);
                buildrequestname.to = "http://localhost:8083/IMessagePassingComm";
                buildrequestname.from = from;
                buildrequestname.arguments = selectedXmlFiles;             
                buildrequestname.author = "Repository";
                buildrequestname.command = "StoreBuildRequests";
                repo.postMessage(buildrequestname);
           
           
        }
        /*----------------------------------------------------<this is to get repo files>--------------------------------------------*/
        private List<String> getRepoFiles()
        {
            if(filesList!=null)
            {
                filesList.Clear();
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(@"../../../RepositoryStorage");
            foreach (FileInfo f in directoryInfo.GetFiles().Where(x => x.Extension != ".xml"))
            {
                filesList.Add(f.ToString());

            }
            return filesList;
        }
        /*----------------------------------------------------<this is to get repo xml files>--------------------------------------------*/
        private List<String> getRepoXMLFiles()
        {
            if (filesXMLList != null)
            {
                filesXMLList.Clear();
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(@"../../../RepositoryStorage");
            foreach (FileInfo f in directoryInfo.GetFiles("*.xml"))
            {
                filesXMLList.Add(f.ToString());

            }
            return filesXMLList;
        }

#if (TEST_REPO)
        static void Main(string[] args)
        {
            Repository repository = new Repository();
        }
#endif
    }
}
