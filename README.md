# UniqueNumberCounter

## Overview
This project consists of two subprojects:
1. **Console App** - Reads a binary file and counts unique numbers.
2. **xUnit Tests** - Validates the correctness of the BinaryFileProcessor.

## Prerequisites
Make sure you have the following installed:
- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

Verify installation:
```sh
dotnet --version
```

## Solution Structure

```sh
UniqueNumberCounter/
│── UniqueNumberCounterApp/                 # Console application
│   ├── Program.cs                          # Main entry point for the application
│   ├── BinaryFileProcessor.cs              # Basic processing of binary files
│   ├── LowLevelBinaryFileProcessor.cs      # Low level processing of binary files
│   ├── ParallelBinaryFileProcessor.cs      # Parallel processing of binary files
│   ├── UniqueNumberCounter.csproj          # Project file
│
│── UniqueNumberCounterTests/               # xUnit test project
│   ├── BinaryFileProcessorTests.cs         # Unit tests for basic processing of binary files
│   ├── LowLevelBinaryFileProcessorTests.cs # Unit tests for low level processing of binary files
│   ├── ParallelBinaryFileProcessorTests.cs # Unit tests for parallel processing of binary files
│   ├── UniqueNumberCounter.Tests.csproj    # Test project file
│
│── global.json                             # Configuration containing SDK version
│── README.md                               # Project documentation
│── UniqueNumberCounter.sln                 # Solution file
```

## Setup

### Clone or download the repository
```sh
git clone https://github.com/Ivo-Sap/UniqueNumberCounter.git
cd UniqueNumberCounter
```

### Open the project in Visual Studio Code
```sh
code .
```

### Restore dependencies
```sh
dotnet restore
```
## Running the Console App

### Restore dependencies
```sh
dotnet restore
```

### The console app requires a binary file as input. Run the program using:
```sh
dotnet run --project UniqueNumberCounterApp <binary_file>
```

### Example:
```sh
dotnet run --project UniqueNumberCounterApp data.bin
```

### Running All Tests
``` sh
dotnet test
```

### Running Specific Tests
``` sh
dotnet test --filter "<namespace>.<class>.<test>"
```

### Example:
``` sh
dotnet test --filter "UniqueNumberCounter.BinaryFileProcessorTests.ProcessBinaryFile_VariousDatasetSizes"
```
