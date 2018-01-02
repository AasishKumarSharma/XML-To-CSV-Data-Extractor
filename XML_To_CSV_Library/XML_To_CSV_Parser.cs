using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Data;
using System.Text;

namespace XML_To_CSV_Library
{
    /// <summary>
    /// 
    /// </summary>
    public class XML_To_CSV_Parser
    {
        public XML_To_CSV_Parser()
        {
            this.dataTable = new DataTable();
            this.tableColumnList = new Dictionary<string, ColumnDetails>();
            this.tableColumnWiseDataList = new Dictionary<int, ColumnData>();
            this.XMLDepthDictonary = new Dictionary<int, string>();
        }


        public DataTable dataTable { get; set; }
        public Dictionary<string, ColumnDetails> tableColumnList { get; set; }
        public Dictionary<int, ColumnData> tableColumnWiseDataList { get; set; }
        public Dictionary<int, string> XMLDepthDictonary { get; set; }


        /// <summary>
        /// Extract XML Data with nested complexity into CSV format.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFilePath"></param>
        public void ExtractXMLDataToDataTable(string sourceFilePath, string destinationFilePath)
        {

            #region Extract XML Information

            using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = XmlReader.Create(stream))
            {
                var colId = string.Empty;
                var parentHeaderName = string.Empty;
                var previousHeaderName = string.Empty;
                XMLDepthDictonary.Clear();

                // Read each of the tags
                // MoveToContent skips any whitespace and comments that may reside in the XML
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        /*
                        None = 0,
                        Element = 1,
                        Attribute = 2,
                        Text = 3,
                        CDATA = 4,
                        EntityReference = 5,
                        Entity = 6,
                        ProcessingInstruction = 7,
                        Comment = 8,
                        Document = 9,
                        DocumentType = 10,
                        DocumentFragment = 11,
                        Notation = 12,
                        Whitespace = 13,
                        SignificantWhitespace = 14,
                        EndElement = 15,
                        EndEntity = 16,
                        XmlDeclaration = 17
                        */

                        case XmlNodeType.None:

                            reader.MoveToContent();
                            // Only detect start elements.
                            if (reader.IsStartElement())
                            {
                                colId = GetOrAddColumnId(parentHeaderName, EscapeCharacters(reader.LocalName), true);
                                parentHeaderName = EscapeCharacters(reader.LocalName);
                            }

                            reader.ReadStartElement();

                            break;

                        case XmlNodeType.Whitespace:
                            //reader.Skip();
                            break;

                        case XmlNodeType.Element:
                            if (string.IsNullOrEmpty(EscapeCharacters(reader.LocalName)))
                            {
                                //reader.MoveToContent();
                            }
                            else
                            {
                                if (reader.IsStartElement())
                                {
                                    parentHeaderName = GetDepthWiseHeaderName(reader.Depth, string.IsNullOrEmpty(previousHeaderName) ? EscapeCharacters(reader.LocalName) : previousHeaderName);
                                    colId = GetOrAddColumnId(parentHeaderName, EscapeCharacters(reader.LocalName), true);//reader.LocalName
                                    previousHeaderName = EscapeCharacters(reader.LocalName);
                                }
                            }
                            reader.MoveToContent();
                            break;

                        case XmlNodeType.EndElement:
                            RemoveParentHeader(EscapeCharacters(reader.LocalName));
                            reader.ReadEndElement();
                            break;

                        case XmlNodeType.Text:
                            AddColumnWiseDataByColId(colId, reader.ReadContentAsString());
                            break;

                        //case XmlNodeType.None:
                        //break;

                        default:
                            reader.Skip();
                            break;
                    }
                }

            }

            #endregion

            #region Generate DataTable

            var colIndex = 1;
            foreach (var colDetails in tableColumnList)
            {
                AddTableColumn(string.Format("{0}_{1}", colIndex, colDetails.Value.ColumnName), "string");
                colIndex++;
            }

            var count = 0;
            var dataArray = tableColumnWiseDataList.ToArray();
            var totalDataCount = (tableColumnWiseDataList.Count);

            while (count != totalDataCount)
            {

                var row = this.dataTable.NewRow();
                colIndex = 1;

                foreach (var item in tableColumnList)
                {

                    if ((count < totalDataCount) && (item.Value.ColumnId == dataArray[count].Value.ColumnId && dataArray[count].Key == (count + 1)))
                    {
                        row[string.Format("{0}_{1}", colIndex, item.Value.ColumnName)] = dataArray[count].Value.Data;
                        count++;
                    }

                    colIndex++;
                }

                this.dataTable.Rows.Add(row);

            }

            #endregion

            #region Export DataTable To CSV File.

            string directoryPath = string.IsNullOrEmpty(destinationFilePath) ? GetDestinationFilePath(sourceFilePath) : destinationFilePath;
            var tableName = Path.GetFileName(sourceFilePath).Replace(Path.GetExtension(sourceFilePath), ""); ;
            WriteDataTableToCSV(string.Format("{0}\\{1}.csv", directoryPath, EscapeCharacters(tableName)), this.dataTable);

            #endregion

        }


        public void ExtractXMLDataToMultipleDataTable(string sourceFilePath, string destinationFilePath)
        {
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(sourceFilePath);

            string directoryPath = string.IsNullOrEmpty(destinationFilePath) ? GetDestinationFilePath(sourceFilePath) : destinationFilePath;

            foreach (DataTable dt in dataSet.Tables)
            {
                WriteDataTableToCSV(string.Format("{0}\\{1}.csv", directoryPath, EscapeCharacters(dt.TableName)), dt);
            }
        }


        private string GetDestinationFilePath(string sourceFilePath)
        {
            string path = sourceFilePath.Replace(Path.GetExtension(Path.GetFileName(sourceFilePath)), "");
            string directoryPath = string.Format("{0}\\Output", path); // your code goes here
            bool exists = System.IO.Directory.Exists(directoryPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }


        private string GetDepthWiseHeaderName(int depth, string headerName)
        {
            var _depth = depth == 0 ? 0 : (depth - 1);
            var xmlDepthInfo = this.XMLDepthDictonary.FirstOrDefault(x => x.Key == _depth);
            if (string.IsNullOrEmpty(xmlDepthInfo.Value))
            {
                this.XMLDepthDictonary.Add(_depth, headerName);
                xmlDepthInfo = this.XMLDepthDictonary.FirstOrDefault(x => x.Key == _depth);
            }

            var _headerName = string.Empty;
            foreach (var item in this.XMLDepthDictonary)
            {
                _headerName = string.Format("{0}", string.IsNullOrEmpty(_headerName) ? item.Value : string.Format("{0}_{1}", _headerName, item.Value));
                if (item.Key == _depth) { break; }
            }

            return _headerName; //xmlDepthInfo.Value;
        }


        private void RemoveParentHeader(string headerName)
        {
            var item = this.XMLDepthDictonary.FirstOrDefault(x => x.Value == headerName);
            if ((!string.IsNullOrEmpty(headerName)) && (!string.IsNullOrEmpty(item.Value)))
            {
                this.XMLDepthDictonary.Remove(item.Key);
            }
        }


        private void AddTableColumn(string colName, string colType)
        {
            if (!HasColumn(colName))
            {
                this.dataTable.Columns.Add(colName, colType == "decimal" ? typeof(decimal) :
                                            colType == "int" ? typeof(int) :
                                            typeof(string));
            }
        }


        private void AddData(ref DataRow dataRow, string colName, string value)
        {
            if (!HasColumn(colName))
            {
                dataRow[colName] = value;
            }
        }


        private bool HasColumn(string colName)
        {
            return this.dataTable.Columns.Contains(colName);
        }


        private string GetOrAddColumnId(string parentHeaderName, string colName, bool isStarting = false)
        {
            var item = this.tableColumnList.FirstOrDefault(x => x.Value.ColumnName == colName && x.Value.ParentHeaderName == parentHeaderName);
            if (!string.IsNullOrEmpty(item.Key))
            {
                return item.Value.ColumnId;
            }
            else
            {
                var count = this.tableColumnList.Count + 1;
                var _colId = Guid.NewGuid().ToString("N");
                var columnDetails = new ColumnDetails { ParentHeaderName = parentHeaderName, ColumnName = colName, ColumnId = _colId, IsStarting = isStarting, Order = count };
                this.tableColumnList.Add(_colId, columnDetails);
                return columnDetails.ColumnId;
            }
        }


        private bool AddColumnWiseDataByColId(string colId, string data)
        {
            try
            {
                var count = this.tableColumnWiseDataList.Count + 1;
                this.tableColumnWiseDataList.Add(count, new ColumnData { ColumnId = colId, Data = data });
                return true;
            }
            catch (Exception ex)
            {
                return false;
                //throw ex;
            }
        }


        private string EscapeCharacters(string str)
        {
            return str.Trim().Replace('-', '_');
        }


        public void WriteDataTableToCSV(string destinationCSVFile, DataTable dt)
        {
            // Open the output CSV file
            using (var stream = new FileStream(destinationCSVFile, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                // Loop through each of the items we read from the source CSV file.
                foreach (DataColumn column in dt.Columns)
                {
                    if (column != null)
                    {
                        writer.Write(CSVEscape(column.ColumnName));
                        //writer.Write(';');
                        writer.Write(',');
                    }
                }
                writer.WriteLine();

                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        writer.Write(CSVEscape(row[column.ColumnName].ToString()));
                        //writer.Write(';');
                        writer.Write(',');
                    }
                    writer.WriteLine();
                }

            }
        }


        // This methods escapes a string using quotes
        private string CSVEscape(string str)
        {
            // Surround content in quotes and replace any quotes inside the string
            // with two double quotes
            if (string.IsNullOrEmpty(str))
                return string.Format("\"\"", "");
            else
                return string.Format("\"{0}\"", str.Replace("\"", "\"\""));
        }

    }
}
