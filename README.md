# XML-To-CSV-Data-Extractor
Extracts XML data into CSV format.

Currently, it can extract the nested complexity of XML data in two ways. 
  1. XML data into single CSV file. 
  2. XML data into multiple CSV files.
  
But in first case it can only extract XML data that are not in XML attributes.

Like: " < XML > < location > ... < / location > < / XML > "

Not Like: " < XML > < location number="2" name="Fair" var="mf/02n.03"/ > < / location > < / XML >"
  
