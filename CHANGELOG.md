# Change Log
	
All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased][unreleased]

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

[unreleased]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.3.2...HEAD
[3.3.2]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.3.1...v3.3.2
[3.3.1]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.2.0...v3.3.1
[3.2.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.1.0...v3.2.0
[3.1.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v3.0.0...v3.1.0
[3.0.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v2.6.0...v3.0.0
[2.6.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v2.5.0...v2.6.0
[2.5.0]: https://github.com/EnableSoftware/DelimitedDataParser/compare/v2.4.0...v2.5.0
