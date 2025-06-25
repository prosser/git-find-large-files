# Homebrew Tap for git-find-large-files

This is the Homebrew tap for [git-find-large-files](https://github.com/prosser/git-find-large-files), a fast, cross-platform tool to find large files in the entire history of a Git repository (including deleted or moved files).

## How to Use

### 1. Tap this repository

```sh
brew tap prosser/git-find-large-files
```

### 2. Install the tool

```sh
brew install git-find-large-files
```

### 3. Upgrade to the latest version

```sh
brew upgrade git-find-large-files
```

## Usage

See the main project [README](https://github.com/prosser/git-find-large-files#usage) for full usage instructions.

## Updating the Formula

- Update the `url` and `sha256` fields in `git-find-large-files.rb` for each new release.
- The `sha256` can be generated with:
  ```sh
  shasum -a 256 git-find-large-files-macos-arm64-vX.Y.Z.tar.gz
  ```

## License

MIT
