# DelimitedDataParser

C# library for parsing and exporting tabular data in delimited format (e.g. CSV).

## Parser

The `Parser` object takes a `TextWriter` and returns `DataTable`.
```c#
DataTable myData;
using (var parser = new Parser(myTextReader))
{
    myData = parser.Parse();
}
```
### Configuration properties

* `FieldSeparator` - the character used as field delimiter in the text file. Default: `,` (i.e., CSV).
* `UseFirstRowAsColumnHeaders` - indicates whether the first row of the text file should be treated as a header row. Default: `true`.

## Exporter

The `Exporter` object takes a `DataTable` and writes to a `TextWriter`.
```c#
string myCsv;
using (var exporter = new Exporter(myData))
using (var writer = new StringWriter())
{
    exporter.Export(writer);
	myCsv = writer.ToString()
}
```
### Configuration properties

* `FieldSeparator` - the character used as field delimiter in the text file. Default: `,` (i.e., CSV).
* `IncludeEscapeCharacters` - specifies whether each value should be escaped by wrapping in quotation marks. Must be `true` if `FieldSeparator` is a tab character. Default: `true`.
* `OutputColumnHeaders` - specifies whether an initial row containing column names should be written to the output. Default: `true`.

## Columns as text

The `Parser` and `Exporter` support setting columns (identified by column name) as "text" wherein data is wrapped in quotes and preceded with an equals sign, as follows: `="1337"`

To set columns as text, call the `SetColumnsAsText` method on either `Parser` or `Exporter`.

```c#
parser.SetColumnsAsText(new[] { "Foo", "Bar" });
exporter.SetColumnsAsText(new[] { "Baz", "Qux" });
```
