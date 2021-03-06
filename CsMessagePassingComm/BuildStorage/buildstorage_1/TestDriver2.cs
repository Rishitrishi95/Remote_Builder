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
    public class TestDriver2 : iTest
    {
        bool testTested3()
        {
            int expectedResult=15
            bool result = true;
            Tested2 td = new Tested2();
            int check = td.sum(5,10);
            Console.WriteLine("Given input numbers for the tested1 is 5,10");
            Console.WriteLine("Expected Output: {0}", expectedResult);
            Console.WriteLine("Actual Output: {0}", check);
            Console.Write("\n");
            if (check ==expectedResult)
            {
                Console.Write("Test successful");
                return true;
            }
            else
            {
                Console.WriteLine("Test Unsuccessful");
            }
            return false;
        }
        public bool test()
        {
            bool result = testTested3();
            return result;
        }
        static void Main(string[] args)
        {
            Console.Write("Running the test Driver");
            TestDriver2 tD = new TestDriver2();
            tD.test();
        }
    }

}
