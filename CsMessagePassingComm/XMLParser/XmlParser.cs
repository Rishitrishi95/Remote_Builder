//////////////////////////////////////////////////////////////////////
// XmlParser.cs - Parses xml file and sends them to test executive. //
//------------------------------------------------------------------//
// Rishit Reddy Muthyala(c) copyright 2017                          //
// All rights granted provided this copyright notice is retained    //
//------------------------------------------------------------------//
// Language:    C# , Visual Studio 2017                             //
// Platform:    Dell, Windows 10                                    //
// Application: Project #4, BUILD SERVER                            //
//CSE681 - Software Modelling and Analysis                          //
// Author:      Rishit Reddy Muthyala, Syracuse University,		    //
//           rmuthyal@syr.edu                                       //
// Maintainance History: Created on 3rd october 2017                //
// Ver 1.0                      									//
/////////////////////////////////////////////////////////////////////
/*
Module Operations:
==================
This package takes xml file as input and parses them to get required components and uses them in build process.
Build Process:
==============
Required files
System.Xml, System.Xml.Linq

Maintenance History:
====================
ver 1.0

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BuildServer
{
    public class XmlParsing
    {
        public string Author { get; set; } = "";
        public string DateTime { get; set; } = "";
        public string ToolChain { get; set; } = "";
        public string TestDriver { get; set; } = "";
        public List<string> TestedFiles { get; set; } = new List<string>();
        public XDocument Doc { get; set; } = new XDocument();
        /*----------------------------<Load xml file from the test reuest sent>-------------------*/
        public bool LoadXml(string path)
        {
            try
            {
                Doc = XDocument.Load(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }
        public string Parse(string propertyName)
        {

            string parseStr = Doc.Descendants(propertyName).First().Value;
            if (parseStr.Length > 0)
            {
                switch (propertyName)
                {
                    case "author":
                        Author = parseStr;
                        break;
                    case "dateTime":
                        DateTime = parseStr;
                        break;
                    case "testDriver":
                        TestDriver = parseStr;
                        break;
                    case "language":
                        ToolChain = parseStr;
                        break;
                    default:
                        break;
                }
                return parseStr;
            }
            return "";
        }
        /*----< parse document for property list >---------------------*/
        /*
        * - now, there is only one property list for tested files
        */
        public List<string> ParseList(string propertyName)
        {
            List<string> values = new List<string>();

            IEnumerable<XElement> parseElems = Doc.Descendants(propertyName);

            if (parseElems.Count() > 0)
            {
                switch (propertyName)
                {
                    case "tested":
                        foreach (XElement elem in parseElems)
                        {
                            values.Add(elem.Value);
                        }
                        TestedFiles = values;
                        break;
                    default:
                        break;
                }
            }
            return values;
        }
    }
    class XMLParserExec
    {
#if (TEST_XMLPARSER)
        static void Main(string[] args)
        {
            XmlParsing x = new XmlParsing();
            x.LoadXml(@"..\..\..\BuildStorage\TestRequest.xml");
            x.Parse("author");
            x.Parse("dateTime");
            x.Parse("language");
            x.Parse("testDriver");
            x.ParseList("tested");
            Console.WriteLine("Name of Author " + x.Author);
            Console.WriteLine(" date and time " + x.DateTime);
            Console.WriteLine("Tool chain to be used: " + x.ToolChain);
            Console.WriteLine("Test Driver: " + x.TestDriver);
            foreach(string test in x.TestedFiles)
            {
                Console.WriteLine("Tested: " + test);
            }
            Console.ReadKey();
        }
    }
#endif
    }
