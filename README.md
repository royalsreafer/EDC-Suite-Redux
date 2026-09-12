# EDC Suite Redux

**Warning:** This project is currently a placeholder and is not ready for production use. The final goal is to replace the VAG EDC Suite

**Warning:** The project is also very much an AI upgrade of the original project. Mostly as an experiment, but once the project matures this will change.


EDC Suite Redux is a .NET 8 library for inspecting and editing automotive ECU binary data. It provides reusable parsing, map, axis, checksum, and XDF-generation components for supported EDC and MSA file formats.

## Features

- Binary document access with bounds checking
- ECU file parsers for supported EDC15, EDC16, EDC17, and MSA variants
- Symbol, map, axis, and code-block helpers
- Checksum calculation and validation components
- VIN and part-number utilities
- XDF metadata generation
- Transaction and project logging helpers

## Repository Layout

- `EDCSuiteRedux.Core/` - Core library and parser implementations
- `EDCSuiteRedux.Core.Tests/` - xUnit tests
- `EDCSuiteRedux.Viewer/` - WPF binary viewer using WPFHexaEditor
- `EDCSuiteRedux.sln` - Visual Studio solution
- `binaries/` - Binary ecu dumps, can be used as example. Will be used in unit tests.

## Requirements

- .NET 8 SDK

## Build

```powershell
dotnet restore .\EDCSuiteRedux.sln
dotnet build .\EDCSuiteRedux.sln
```

## Test

```powershell
dotnet test .\EDCSuiteRedux.sln
```

## Binary Viewer

The Windows viewer opens a `.bin` file, detects the matching parser, displays parser results, and shows the binary in a read-only hex/ASCII grid.
It also compares the loaded file with another binary byte-for-byte and can create a new merged file by concatenating the loaded file with a second binary. The original files are never overwritten.

```powershell
dotnet run --project .\EDCSuiteRedux.Viewer\EDCSuiteRedux.Viewer.csproj
```

Use **Open binaries folder** to start in the repository's `binaries` directory.

The viewer uses the `WPFHexaEditor` NuGet package. Version 3.4.5 is licensed under AGPL-3.0; review that license before distributing the viewer.

## Usage Notes

This library works with ECU binary data and can make consequential changes to calibration files. Always keep an untouched backup, verify that a file matches the expected ECU hardware and software, and validate checksums before using a modified file. Testing on appropriate bench equipment and following applicable laws and safety procedures is the responsibility of the user.

The public API and parser coverage are still evolving. Consult the source and tests for the currently supported formats and behavior.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
