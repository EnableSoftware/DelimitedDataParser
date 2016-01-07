# DelimitedDataParser

C# library for parsing and exporting tabular data in delimited format (e.g. CSV).

[![NuGet Version](https://img.shields.io/nuget/v/DelimitedDataParser.svg)](https://www.nuget.org/packages/DelimitedDataParser/) [![NuGet Downloads](https://img.shields.io/nuget/dt/DelimitedDataParser.svg)](https://www.nuget.org/packages/DelimitedDataParser/)

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
* `IncludeEscapeCharacters` - specifies whether each value should be escaped by wrapping in quotation marks. Must be `true` if `FieldSeparator` is a tab character. Default: `true`.
* `OutputColumnHeaders` - specifies whether an initial row containing column names should be written to the output. Default: `true`.

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