# Filep - File Encryption and Decryption CLI

**WARNING: ENCRYPTED FILES WILL BE LOST IF YOU FORGET YOUR PASSWORD.**

A simple Command Line Interface (CLI) program designed for parallel encryption and decryption of files using a password.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
  - [Options](#options)
  - [Examples](#examples)

## Features

- Parallel processing for efficient encryption and decryption of files.
- Ability to encrypt, decrypt, or perform both actions on a specified directory.
- Supports keeping older files after decryption/encryption.
- Support for limiting the amount of files.
- Support for additionally encrypting file names as well.
- Argon2id key derivation.
- Sequential encryption/decryption for better performance and lower memory usage.

## Getting Started

### Prerequisites

Ensure that you have the following prerequisite installed:

- [ .NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### Installation

1. Download the latest executable from the [Releases](https://github.com/omegand/File-Protector/releases) tab.
2. Run it using the command line. See [Usage](#usage).
3. Optionally, set it in PATH to use it anywhere using cmd.
4. Optionally, enable it in Windows context menu. See [Examples](#examples).

## Usage

### Options
- ***togglecontext***: Toggles the encrypt and decrypt functions in Windows directory context menu.
- **-d, --directory**: Required. Directory to be processed.
- **-p, --password**: Password to be used. If empty, the executable will ask to input one.
- **--en**: Will only encrypt.
- **--de**: Will only decrypt.
- **-s**: After decryption/encryption, will not delete previous older files.
- **-l**: Limit the number of files processed. Negative numbers will return from the end.
- **--name**: Will ignore file name encryption.
- **--help**: Display a help screen.

### Examples

- <b>Encrypt</b> files and <b>Decrypt</b> (encrypted) files in the "exampleDirectory" directory with the password "secretpass":

    ```bash
     filep -d exampleDirectory -p secretpass
    ```
- <b>Decrypt only</b>, using <b>safe mode</b> (keep old files).

    ```bash
    filep -d exampleDirectory --de -s
    ```
- <b>Encrypt only</b>.

    ```bash
    filep -d exampleDirectory --en
    ```
- Process the <b>first</b> 20 files

    ```bash
    filep -d exampleDirectory -p secretpass -l 20
    ```
- Process the <b>last</b> 20 files

    ```bash
    filep -d exampleDirectory -p secretpass -l -20
    ```
- Toggle the encrypt and decrypt functions in Windows directory context menu.

    ```bash
    filep togglecontext
    ```
    *note:* If you move the executable, you will need to remove and re-add the context menu.