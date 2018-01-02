using System;
using System.Collections.Generic;
using System.Linq;

namespace XML_To_CSV_Library
{
    public class ColumnDetails
    {
        public int Order { get; set; }
        public string ParentHeaderName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnId { get; set; }
        public bool IsStarting { get; set; }
    }
}
