# XiRename _The renaming tool for Unity 3D_

![](https://img.shields.io/badge/unity-2018.3%20or%20later-green.svg)
[![⚙ Build and Release](https://github.com/hww/XiRename/actions/workflows/ci.yml/badge.svg)](https://github.com/hww/XiRename/actions/workflows/ci.yml)
[![openupm](https://img.shields.io/npm/v/com.hww.xirename?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.hww.xirename/)
[![](https://img.shields.io/github/license/hww/XiRename.svg)](https://github.com/hww/XiRename/blob/master/LICENSE)
[![semantic-release: angular](https://img.shields.io/badge/semantic--release-angular-e10079?logo=semantic-release)](https://github.com/semantic-release/semantic-release)

![Title Image](Docs/title_image.png)

Simple asstes renaming tool by [hww](https://github.com/hww)

## Introduction

The tool allows you to select and rename a group of files according to a pattern that can consist of: file name, prefix, suffix, and version or variant number.

Allows you to renumber a series of files by sorting the list of names.

The tool works according to the Studio naming convention, which you can set up through the configuration panel.

![Tool Image](Docs/notes-image.png)

The rules can be configured in the panel (below) or from the C# source code.

![Settings Image](Docs/settings-screenshot.png)

## Install

The package is available on the openupm registry. You can install it via openupm-cli.

```bash
openupm add com.hww.xirename
```
You can also install via git url by adding this entry in your manifest.json

```bash
"com.hww.xirename": "https://github.com/hww/XiRename.git#upm"
```
## TODO

The current state of the project _Alpha_. The tool has not been tested at the moment and needs to be tested and improved: the API, interface and default NamingConvention settings.

- [x] Basic functionality
- [x] Configurabe and safe (no enum) the objects tagging
- [ ] Update documentation
- [ ] Per project verification tool


## Define the Naming Convention with API

The file name consists of tokens (name, prefix, suffix, version or variant). The order of the tokens must be determined.

```C#
fieldOrder.Clear();
fieldOrder.Add(ETokenType.Prefix);
fieldOrder.Add(ETokenType.Name);
fieldOrder.Add(ETokenType.Variant);
fieldOrder.Add(ETokenType.Suffix);
```

Clear all existing file types.

```C#
fileTypes.Clear();
```            

Create a new type Textures2D with category Textures.

```C#
  kind = Define("Textures2D", "Textures", ".tga, .bmp, .jpg, .png, .gif, .psd");
```

Let's add some prefixes and suffixes

```C#
kind.DefinePreffix("Render Target", "RT");
kind.DefinePreffix("Cube Render Target", "RTC");
kind.DefinePreffix("Texture Light Profile", "TLP");
kind.DefineSuffix("Texture (Diffuse/Albedo/Base Color", "D");
kind.DefineSuffix("Texture (Normal)", "N");
```
