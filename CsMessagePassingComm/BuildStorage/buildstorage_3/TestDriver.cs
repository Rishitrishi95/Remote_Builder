using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testDriver
{
    interface iTest
    {
        bool test();
        
    }
    public class TestDriver : iTest
    {
        bool testTested1()
        {
            int expectedResult=15;
            bool result = true;
            Tested1 td = new Tested1();
            int check = td.sum(5,10);
            Console.WriteLine("Given input numbers for the tested1 is 5,10");
            Console.WriteLine("Expected Output: {0}", expectedResult);
            Console.WriteLine("Actual output: {0}", check);
            if (check == expectedResult)
            {
                Console.WriteLine("TEST SUCCESSFULL");
                Console.Write("\n");
                return true;
            }
            else
            {
                Console.WriteLine("TEST UNSUCCESSFULL");
                Console.Write("\n");
                return false;
            }
        }
       
        public bool test()
        {
            bool result = testTested1();
            return result;
        }

        static void Main(string[] args)
        {
            Console.Write("Running the test Driver");
            TestDriver tD = new TestDriver();
            tD.test();
           
        }
    }

}
