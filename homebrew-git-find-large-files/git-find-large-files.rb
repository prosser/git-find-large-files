class GitFindLargeFiles < Formula
  desc "Find large files in the entire history of a Git repository, including deleted or moved files. Fast, cross-platform, and outputs JSON."
  homepage "https://github.com/prosser/git-find-large-files"
  version "0.0.3"
  license "MIT"

  on_macos do
    url "https://github.com/prosser/git-find-large-files/releases/download/v#{version}/git-find-large-files-osx-arm64-v#{version}.tar.gz"
    sha256 "PUT_MACOS_SHA256_HASH_HERE"
  end

  on_linux do
    url "https://github.com/prosser/git-find-large-files/releases/download/v#{version}/git-find-large-files-linux-x64-v#{version}.tar.gz"
    sha256 "PUT_LINUX_SHA256_HASH_HERE"
  end

  def install
    bin.install "git-find-large-files"
  end

  test do
    system "#{bin}/git-find-large-files", "--version"
  end
end
