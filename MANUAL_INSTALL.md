# Manual Installation

## Linux
1. Download the latest Linux binary from the [Releases](https://github.com/prosser/git-find-large-files/releases) page:
   ```sh
   wget https://github.com/prosser/git-find-large-files/releases/download/v0.0.3/git-find-large-files-linux-x64-v0.0.3.tar.gz
   wget https://github.com/prosser/git-find-large-files/releases/download/v0.0.3/git-find-large-files-linux-x64-v0.0.3.tar.gz.sha256
   sha256sum -c git-find-large-files-linux-x64-v0.0.3.tar.gz.sha256
   tar -xzf git-find-large-files-linux-x64-v0.0.3.tar.gz
   sudo mv git-find-large-files /usr/local/bin/
   ```
2. Run with:
   ```sh
   git find-large-files --help
   ```

## Windows
1. Download the latest Windows `.zip` and `.zip.sha256` from the [Releases](https://github.com/prosser/git-find-large-files/releases) page.
2. Verify the checksum:
   ```powershell
   Get-FileHash git-find-large-files-win-x64-v0.0.3.zip -Algorithm SHA256
   # Compare the output to the contents of git-find-large-files-win-x64-v0.0.3.zip.sha256
   ```
3. Extract the zip file to a folder in your `PATH` environment variable.
4. Run in Command Prompt or PowerShell:
   ```powershell
   git find-large-files.exe --help
   ```

## macOS (manual)
If you prefer not to use Homebrew:
1. Download the latest macOS binary and `.sha256` from the [Releases](https://github.com/prosser/git-find-large-files/releases) page.
2. Verify the checksum:
   ```sh
   shasum -a 256 -c git-find-large-files-macos-arm64-v0.0.3.tar.gz.sha256
   ```
3. Extract and move to a directory in your `PATH`:
   ```sh
   tar -xzf git-find-large-files-macos-arm64-v0.0.3.tar.gz
   sudo mv git-find-large-files /usr/local/bin/
   ```
