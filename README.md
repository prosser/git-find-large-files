# Git Tool: Find Large Files

[![Build Status](https://github.com/prosser/git-find-large-files/actions/workflows/release.yml/badge.svg)](https://github.com/prosser/git-find-large-files/actions/workflows/release.yml)

A fast, cross-platform command-line tool to find large files in the entire history of a Git repository, including deleted or moved files.

## Features

- Scans all commits and branches for large files (including deleted/moved)
- Outputs results as JSON (to stdout or a file)
- Supports Windows, Linux, and macOS (AOT/self-contained builds)
- Efficient: uses batch Git commands for speed

## Installation

- **Homebrew (macOS & Linux):**
  ```sh
  brew tap prosser/git-find-large-files
  brew install git-find-large-files
  ```
- **Manual install:** See [MANUAL_INSTALL.md](MANUAL_INSTALL.md) for step-by-step instructions for Windows, Linux, and macOS.

## Usage

```sh
git find-large-files [OPTIONS]
```

### Options

- `--size-threshold <size>`  
  Set the size threshold for large files in MiB (default: 10 MiB).
- `--help`  
  Show help message.
- `-o, --output <file>`  
  Output the results to a file (default: stdout).

### Examples

Find all files larger than 50 MiB in the repo history, and output the results to `large-files.json`.
```sh
git find-large-files --size-threshold 50 -o large-files.json
```

Find all files larger than 20 MiB in the repo history, and output the results to `big.json`.
```sh
git find-large-files 20 >big.json
```


## Building

To build, ensure you have the latest .NET 8.0 SDK installed, and run:

- **Windows:**
  ```sh
  dotnet publish -c Release -r win-x64 --self-contained
  ```
- **Linux:**
  ```sh
  dotnet publish -c Release -r linux-x64 --self-contained
  ```
- **macOS (Apple Silicon):**
  ```sh
  dotnet publish -c Release -r osx-arm64 --self-contained
  ```

## License

See [LICENSE.txt](LICENSE.txt).