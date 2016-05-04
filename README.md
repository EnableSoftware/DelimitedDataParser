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
	myCsv = writer.ToString()
}
```
### Configuration properties

* `FieldSeparator` - the character used as field delimiter in the text file. Default: `,` (i.e., CSV).
* `SanitizeStrings` - specifies whether strings should be sanitized, prepending blacklisted characters at the start of the string with a single quote `'`. The default value is `false`.
* `IncludeEscapeCharacters` - specifies whether each value should be escaped by wrapping in quotation marks. Must be `true` if `FieldSeparator` is a tab character. Default: `true`.
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
