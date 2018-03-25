//////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client Gui operations                               //
// ver 1.0                                                                  //
//                                                                          //
// Author: Rishit Reddy Muthyala, rmuthyal@syr.edu                          //
// Application: CSE681 Project 4-Client                                     //
// Environment:WPF Console                                                  //
//////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
* This package does the operations that needed to be performed by client gui
* It communicates with repository, mother builder and test harness
* It runs on port 8080
* 
* Public Interface 
* ---------------- 
* MainWindow()- It initializes the components when GUI is loaded. 
*  
* Required Files: 
* --------------- 
* MessagePassingCommService, ImessagePassingCommService,Environment,TestUtilities.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using MessagePassingComm;
using System.Threading;
using System.Xml.Linq;
using System.IO;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string from { get; set; } = "http://localhost:8080/IMessagePassingComm";

        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Comm client { get; set; } = null;
        Thread rcvThread = null;
        List<String> repoFilesList { get; set; } = new List<String>();
        /*--------------------------------<It loads the initial components of the GUI window and creates comm object and intializes dispatcher messages>-------------------------------------------*/
        public MainWindow()
        {
            InitializeComponent();
            client = new Comm("http://localhost",8080);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }
        /*--------------------------------<This is for initalizing dispatcher messages>-------------------------------------------*/
        private void initializeMessageDispatcher()
        {
            messageDispatcher["getRepositoryFiles"] = (CommMessage msg) =>
              {
                  RepsoitoryListBox.Items.Clear();
                  foreach(string f in msg.arguments)
                  {
                      //repoFilesList.Add(f);
                      RepsoitoryListBox.Items.Add(f);
                  }
              };
            messageDispatcher["getRepositoryXMLFiles"] = (CommMessage msg) =>
            {
              RepositoryXmlListBox.Items.Clear();
                    foreach (string f in msg.arguments)
                {
                    //repoFilesList.Add(f);
                    
                    RepositoryXmlListBox.Items.Add(f);
                }
            };
            messageDispatcher["UpdateTestreuslt"] = (CommMessage msg) =>
            {
                TestResults.Text = msg.xmlString;
            };
            messageDispatcher["RequestBuild"] = (CommMessage msg) =>
            {
                PathAddress.Text = msg.xmlString;
            };

        }
        /*--------------------------------<This method runs to recieve messages from other servers>-------------------------------------------*/
        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = client.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution
                if((msg.command == "RequestBuild") ||(msg.command== "getRepositoryXMLFiles" )|| (msg.command== "getRepositoryFiles")||(msg.command== "UpdateTestreuslt"))
                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }

        /*--------------------------------<On click of repository files it sends message to repository server to send list of files>-------------------------------------------*/
        private void RepoFiles(object sender, RoutedEventArgs e)
        {
            CommMessage requestRepositoryFiles = new CommMessage(CommMessage.MessageType.request);
            requestRepositoryFiles.author = "client";
            requestRepositoryFiles.command = "RequestRepositoryFiles";
            requestRepositoryFiles.from = this.from;
            requestRepositoryFiles.to= "http://localhost:8081/IMessagePassingComm";
            requestRepositoryFiles.errorMsg = null;
            client.postMessage(requestRepositoryFiles);

        }
        /*--------------------------------<on click sends message requesting xml files>-------------------------------------------*/
        private void XmlFiles(object sender, RoutedEventArgs e)
        {
            CommMessage requestRepositoryXMLFiles = new CommMessage(CommMessage.MessageType.request);
            requestRepositoryXMLFiles.author = "client";
            requestRepositoryXMLFiles.command = "RequestRepositoryXMLFiles";
            requestRepositoryXMLFiles.from = this.from;
            requestRepositoryXMLFiles.to = "http://localhost:8081/IMessagePassingComm";
            requestRepositoryXMLFiles.errorMsg = null;
            client.postMessage(requestRepositoryXMLFiles);
        }
        /*--------------------------------<on click sends the selected files to make a build request in repsoitory>-------------------------------------------*/
        private void BuildRequest(object sender, RoutedEventArgs e)
        {
            
                CommMessage buildRequestMessage = new CommMessage(CommMessage.MessageType.request);
                buildRequestMessage.from = this.from;
                buildRequestMessage.to = "http://localhost:8081/IMessagePassingComm";
                buildRequestMessage.errorMsg = null;
                buildRequestMessage.author = "Client";
                buildRequestMessage.command = "BuildRequest";
                buildRequestMessage.arguments = getSelectedFiles();
                
                client.postMessage(buildRequestMessage);
            RepsoitoryListBox.SelectedIndex = -1;
            
        }
        /*--------------------------------<It gets the selcted files in reposotory list box>-------------------------------------------*/
        private List<string> getSelectedFiles()
        {
            List<string> selectedFiles = new List<string>();
            foreach (string item in RepsoitoryListBox.SelectedItems)
            {
                selectedFiles.Add(item);
                //selectedFiles.Add(item);
            }
            return selectedFiles;

        }

        /*--------------------------------<It sends message to reposoitory to send the selcted build requests to mother builder>-------------------------------------------*/
        private void SendBuildRequest(object sender, RoutedEventArgs e)
        {
                CommMessage sendBuildRequests = new CommMessage(CommMessage.MessageType.reply);
                sendBuildRequests.from = this.from;
                sendBuildRequests.to = "http://localhost:8081/IMessagePassingComm";
                sendBuildRequests.command = "sendBuildRequest";
                sendBuildRequests.author = "client";
                sendBuildRequests.arguments = getSelectedXmlFiles();
                client.postMessage(sendBuildRequests);
            RepositoryXmlListBox.SelectedIndex = -1;
            
        }
        /*--------------------------------<it gets the selected build requests>-------------------------------------------*/
        private List<String> getSelectedXmlFiles()
        {
            List<String> selectedXMLFiles = new List<String>();
            foreach (string item in RepositoryXmlListBox.SelectedItems)
            {

                selectedXMLFiles.Add(item);
            }
            return selectedXMLFiles;
        }

        /*--------------------------------<on click of start process sends message to mother builder to startnumber of process>-------------------------------------------*/
        private void NumberOfProcess(object sender, RoutedEventArgs e)
        {
            if (numberofprocess.Text != null)
            {


                List<string> number = new List<string>();
                number.Add(numberofprocess.Text);
                CommMessage numberOfprocessMessage = new CommMessage(CommMessage.MessageType.request);
                numberOfprocessMessage.from = this.from;
                numberOfprocessMessage.to = "http://localhost:8083/IMessagePassingComm";
                numberOfprocessMessage.command = "startchildbuilder";
                numberOfprocessMessage.author = "client";
                numberOfprocessMessage.arguments = number;
                client.postMessage(numberOfprocessMessage);

            }
        }
 /*--------------------------------<On kill button kills the child process by sending message>-------------------------------------------*/
        private void killProcessButtonClick(object sender, RoutedEventArgs e)
        {
            CommMessage numberOfprocessMessage = new CommMessage(CommMessage.MessageType.request);
            numberOfprocessMessage.from = "http://localhost:8080/IMessagePassingComm";
            numberOfprocessMessage.to = "http://localhost:8083/IMessagePassingComm";
            numberOfprocessMessage.command = "KillProcess";
            numberOfprocessMessage.author = "client";
           // numberOfprocessMessage.arguments = number;
            client.postMessage(numberOfprocessMessage);
        }

        
    }
}
