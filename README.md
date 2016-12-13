# DelimitedDataParser

C# library for parsing and exporting tabular data in delimited format (e.g. CSV).

 [![NuGet Downloads](https://buildstats.info/nuget/DelimitedDataParser)](https://www.nuget.org/packages/DelimitedDataParser/)

## Parser

The `Parser.Parse()` method takes a `TextReader` and returns `DataTable`.

```c#
var parser = new Parser();
using (DataTable myData = parser.Parse(myTextReader))
{
	// Make use of `myData` here…
}
```

For processing a large amount of delimited data, the `Parser.ParseReader()` method takes a `TextReader` and returns `System.Data.Common.DbDataReader`, which provides a fast, forward-only stream of rows. This allows for processing each row in turn, rather than reading a whole file into memory.

```c#
var parser = new Parser();

var reader =  parser.ParseReader(myTextReader))

while (reader.Read())
{
	// Each field for the current row can be retrieved using the column index:
	var field1 = reader[0];
	var field2 = reader[1];
	// etc…
}
```

### Configuration properties

* `FieldSeparator` - the character used as field delimiter in the text file. Default: `,` (i.e., CSV).
* `UseFirstRowAsColumnHeaders` - specifies whether the first row of the text file should be treated as a header row. Default: `true`.

## Exporter

The `Exporter.Export()` method takes a `DataTable` and writes to a `TextWriter`.

```c#
string myCsv;
using (var writer = new StringWriter())
{
	var exporter = new Exporter();
    exporter.Export(myData, writer);
    myCsv = writer.ToString();
}
```

For exporting a large amount of delimited data, the `Exporter.ExportReader()` method takes a `System.Data.Common.DbDataReader` instance and writes to a specified `TextWriter`, which provides a fast, streamed-based generation of row data. This allows for processing each row in turn, rather than needing to retain the whole data source in memory.

```c#
using (var fileWriter = File.CreateText("my.csv"))
{
    var exporter = new Exporter();
    exporter.ExportReader(myDataReader, fileWriter);
}
```

### Configuration properties

* `FieldSeparator` - the character used as field delimiter in the text file. Default: `,` (i.e., CSV).
* `SanitizeStrings` - specifies whether strings should be sanitized, prepending blacklisted characters at the start of the string with a single quote `'`. The default value is `false`.
* `IncludeEscapeCharacters` - specifies whether each value should be escaped by wrapping in quotation marks. Default: `true`.
* `OutputColumnHeaders` - specifies whether an initial row containing column names should be written to the output. Default: `true`.
* `UseExtendedPropertyForColumnName(string key)` - specifies a key that is used to search on the ExtendedProperties of a DataColumn. If it finds a value this will be used as the column header, if no match is found it will default to the column's ColumnName. This should be used if you are required to output a different column header to what is stored on the column's ColumnName.

## Columns as text

The `Parser` and `Exporter` support setting columns (identified by column name) as "text" wherein data is wrapped in quotes and preceded with an equals sign, as follows: `="1337"`

To set columns as text, call the `SetColumnsAsText` method on either `Parser` or `Exporter`.

```c#
parser.SetColumnsAsText(new[] { "Foo", "Bar" });
exporter.SetColumnsAsText(new[] { "Baz", "Qux" });
```

To clear any columns previously set as text, call the `ClearColumnsAsText` method on either `Parser` or `Exporter`.

```c#
parser.ClearColumnsAsText();
exporter.ClearColumnsAsText();
```
