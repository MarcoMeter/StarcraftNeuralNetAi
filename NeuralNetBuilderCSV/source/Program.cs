using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetBuilderCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TEST");
            using (StreamWriter file = new StreamWriter("my.txt", true))
            {
                file.WriteLine(System.DateTime.Now.TimeOfDay.ToString("") + "  baaaaam!  ");
            }
            Console.ReadLine();
        }
    }
}
