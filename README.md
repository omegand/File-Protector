# Filep

A simple Command Line Interface (CLI) program that enables parallel encryption and decryption of files using a password.
This tool is designed to provide a straightforward and secure way to process directories with file encryption and decryption functionalities.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
  - [Actions](#actions)
  - [Options](#options)
- [Examples](#examples)

## Features

- Parallel processing for efficient encryption and decryption of files.
- Option to reset and remove the current password from the system.
- Ability to encrypt, decrypt, or perform both actions on a specified directory.
- Supports keeping the older files after decryption/encryption.

## Getting Started

### Prerequisites

Ensure that you have the following prerequisites installed:

- [ .NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

### Installation

Download the executable from Releases tab and run it through the command line.

## Usage

### Actions

- **process**: (Default) Process a directory.
- **reset**: Removes the current password from the system. Encrypted files will not be decrypted; be careful when using this.

### Options

- **-d, --directory**: Required. Directory to be processed.
- **-p, --password**: Required. Password to be used.
- **--en**: Will only encrypt.
- **--de**: Will only decrypt.
- **-s**: After decryption/encryption, will not delete previous older files.
- **--help**: Display this help screen.

## Examples

- Encrypt files in the "exampleDirectory" directory with the password "secretpass":

    ```bash
     filep process -d exampleDirectory -p secretpass --en
    ```

- Decrypt files in the "exampleDirectory" directory with the password "secretpass" and keep older files:

    ```bash
    filep process -d exampleDirectory -p secretpass --de -s
    ```
- Encrypt regular files and decrypt encrypted files in the "exampleDirectory" directory with the password "secretpass"

    ```bash
    filep process -d exampleDirectory -p secretpass
    ```
- Remove your password from the system. Be careful, TRYING TO DECRYPT FILES WITH A NEW PASSWORD WILL CORRUPT THEM...

    ```bash
    filep process -d exampleDirectory -p secretpass
    ```
