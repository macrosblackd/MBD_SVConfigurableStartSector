# MBD_SVConfigurableStartSector

## Overview
MBD_SVConfigurableStartSector is a BepInEx plugin for Star Valor that allows you to customize the starting sector's properties including size, richness, asteroid density, and sector type.

## Features
- Configurable start sector size (VerySmall, Small, Medium, Large)
- Adjustable mineral richness (0.0 to 1.0)
- Customizable big asteroid density (None, Random, VeryDense)
- Selectable sector type (Normal, Nebula, PitchBlack, AsteroidRush)
- Easy configuration through BepInEx config file

## Requirements
- Star Valor (latest version recommended)
- BepInEx 5.x installed ([Download BepInEx](https://github.com/BepInEx/BepInEx/releases))
- .NET Framework 4.7.1

## Recommended

- **BepInEx.ConfigurationManager**: This plugin adds an in-game configuration menu that allows you to modify mod settings without editing config files manually. You can download it from the [BepInEx.ConfigurationManager releases page](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases).

## Installation

1. **Install BepInEx**  
   Download and install the latest BepInEx 5.x for Star Valor from the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases).

2. **Copy the plugin**  
   Download the latest release of this plugin. Copy the provided DLL file from the release into the `BepInEx/plugins` folder in your Star Valor installation directory.

3. **Launch the game**  
   Start Star Valor. The plugin will be loaded automatically by BepInEx.

## Configuration

The plugin creates a configuration file at `BepInEx/config/macrosblackd.starvalormods.configurablestartsector.cfg` with the following settings:

- **StartSectorSize**: Size of the start sector (VerySmall, Small, Medium, Large)
- **StartSectorRichness**: Mineral richness from 0.0 to 1.0 (default: 0.2)
- **StartSectorBigAsteroidDensity**: Big asteroid density (None, Random, VeryDense)
- **StartSectorType**: Sector type (Normal, Nebula, PitchBlack, AsteroidRush)
- **PluginEnabled**: Enable or disable the plugin (true/false)

## Usage

- This mod requires a new game to take effect.
- Configuration changes can be made in the config file or through BepInEx's configuration manager.
- The plugin will log sector generation information to the BepInEx log when creating a new game.