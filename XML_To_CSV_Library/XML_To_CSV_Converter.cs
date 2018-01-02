using System;
using System.Collections.Generic;
using System.Linq;

namespace XML_To_CSV_Library
{
    public static class XML_To_CSV_Converter
    {

        /// <summary>
        /// Extract XML data from xml file into single CSV file. If any exception returns 1 else returns 0.
        /// This does not parse XML with data in attributes for single file mode.
        /// </summary>
        /// <param name="sourceFilePath">Source XML file path.</param>
        /// <param name="destinationFilePath">Destination path to store CSV file.</param>
        /// <returns></returns>
        public static int ExtractXMLDataToSingleCSVFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                var CFSParser = new XML_To_CSV_Parser();
                CFSParser.ExtractXMLDataToDataTable(sourceFilePath, destinationFilePath);
            }
            catch
            {
                return 1;
            }
            return 0;
        }


        /// <summary>
        /// Extract XML data from xml file into multiple CSV files. If any exception returns 1 else returns 0.
        /// </summary>
        /// <param name="sourceFilePath">Source XML file path.</param>
        /// <param name="destinationFilePath">Destination path to store CSV files.</param>
        /// <returns></returns>
        public static int ExtractXMLDataToMultipleCSVFiles(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                var CFSParser = new XML_To_CSV_Parser();
                CFSParser.ExtractXMLDataToMultipleDataTable(sourceFilePath, destinationFilePath);
            }
            catch
            {
                return 1;
            }
            return 0;
        }

    }
}
