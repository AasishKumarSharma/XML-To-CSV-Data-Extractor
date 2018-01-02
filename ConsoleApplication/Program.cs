using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML_To_CSV_Library;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter XML source file path: ");
            string source = Console.ReadLine();
            Console.Write("Please enter CSV destination folder path: ");
            string destination = Console.ReadLine();
            Console.Write("Extract data in single file (Y/N): ");
            string isSingle = Console.ReadLine();

            var result = (isSingle.ToLower().Equals("y")) ?
                            XML_To_CSV_Converter.ExtractXMLDataToSingleCSVFile(source, destination) :
                            XML_To_CSV_Converter.ExtractXMLDataToMultipleCSVFiles(source, destination);
            if (result == 0)
                Console.WriteLine("The file from {0} is converted and saved to {1}.", source, destination);
            else
                Console.WriteLine("Unable to extract XML data. Try again later !!!", source, destination);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
