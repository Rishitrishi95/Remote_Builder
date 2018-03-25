using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testDriver
{
    public class Tested1
    {
        public int sum(int a, int b)
             {
            int sum = 0;
            try
            {
                
                sum += a + b;
                return sum;

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return ex;
            }
            

        }
    }
}