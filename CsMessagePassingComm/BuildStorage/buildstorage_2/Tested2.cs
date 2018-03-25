using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testDriver
{
    public class Tested2

    {
        public int sum(int a, int b)
        {
            int sum=0;
            int[] arr= new int[2];
             arr[0]=a;
             arr[1]=b;

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    sum += arr[i];
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
                return 0;
            }
            return sum;
        }
    }
}