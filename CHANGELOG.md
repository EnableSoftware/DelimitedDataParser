# Change Log
	
All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased][unreleased]

## [6.1.0] - 2022-01-04

### Added

- Add the `|` character to the list of UnsafeLeadingCharacters in the `Exporter`.
- Add functionality to prevent the column value sanitization for specific columns.

## [6.0.0] - 2021-12-13

### Added

- Update _sanitizeStrings default to `true`.
- Sanitize strings will now also take effect when escape characters are used.

## [5.0.0] - 2021-11-23

### Added

- Add the option to trim column headers when reading from the input text, and make it the default.

## [4.2.1] - 2020-11-17

### Added

- Add support for progress reports in `Exporter`.

## [4.2.0] - 2019-03-19

### Added

- Add support for cancellation.

### Fixed

- Fix a bug in enumerating files where the first row of data contains less
  columns than the header row.

## [4.1.0] - 2018-03-28

### Changed

- Migrate `DelimitedDataParser` project file to .NET Core `.csproj` project system.

### Added

- Add multi-targeting support for `net45` and `netstandard2.0`.

## [4.0.3] - 2018-02-16

### Fixed

- Fix a bug in the handling of newline characters wherein any pair of newline characters
  (as opposed to just `\r\n`) would be treated as a single newline.

## [4.0.2] — 2017-08-30

### Fixed

- Fix a bug with the `HasData` property on the reader returned by
  `Parser.ParseReader()`. Previously this property incorrectly reported that
  there was data when a header row was present, even if there were no
  subsequent data rows.

## [4.0.1] — 2017-08-10

### Added

- Add support for specifying encoding of input data.

## [4.0.0] — 2016-12-21

### Added

- Add support for a streaming reading of delimited data files.
- Add support for a streaming export of delimited data files from a `DbDataReader`.

## [3.3.2] — 2016-08-03

### Fixed

- Remove check from `Exporter` preventing `TabSeparator` being used when
  `IncludeEscapeCharacters` disabled.

## [3.3.1] — 2016-05-04

### Added

- Add support for a blacklist of field values. Fields that start with a value
  from the blacklist should have a single quote prepended to them.

### Fixed

- Fix a bug with the `IncludeEscapeCharacters` setting. Previously this setting
  could only be disabled if using a tab character as a field separator.

## [3.2.0] — 2016-04-29

### Added

- Add XML documentation to NuGet package.
- Support for clearing column names specified via `ExtendedProperties` on a
  `DataColumn`.

## [3.1.0] — 2016-04-28

### Added

- Support for using `ExtendedProperties` on a `DataColumn`.

### Changed

- Unit test framework replaced with XUnit.net.

## [3.0.0] — 2015-10-02

### Changed

- The public API has been re-worked in order to be easier to use and to test.

  The dependencies on `TextReader` and `DataTable` have been changed from
  constructor parameters to method parameters on the `Parse` and `Export`
  methods. This makes mocking and dependency injection easier and also allows
  a single instance of the `Parser` and `Exporter` classes to be re-used for
  multiple operations.

### Removed

- Exporting to `string` has been removed.

## [2.6.0] — 2015-09-30

### Added

- Contributing guidelines.

### Changed
	
- Public members marked as `virtual` to support use with mocking frameworks.

## [2.5.0] — 2015-07-31
	
### Added

- Support exporting to `TextWriter`, for more efficient writing.
- This change log.
	
### Deprecated
	
- Exporting to `string` deprecated.
	
## 2.4.0 — 2015-07-12

- Initial public release.

[unreleased]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v6.1.0...HEAD
[6.1.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v6.0.0...v6.1.0
[6.0.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v5.0.0...v6.0.0
[5.0.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.2.1...v5.0.0
[4.2.1]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.2.0...v4.2.1
[4.2.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.1.0...v4.2.0
[4.1.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.0.3...v4.1.0
[4.0.3]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.0.2...v4.0.3
[4.0.2]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.0.1...v4.0.2
[4.0.1]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v4.0.0...v4.0.1
[4.0.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.3.2...v4.0.0
[3.3.2]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.3.1...v3.3.2
[3.3.1]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.2.0...v3.3.1
[3.2.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.1.0...v3.2.0
[3.1.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.0.0...v3.1.0
[3.0.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v2.6.0...v3.0.0
[2.6.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v2.5.0...v2.6.0
[2.5.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v2.4.0...v2.5.0
