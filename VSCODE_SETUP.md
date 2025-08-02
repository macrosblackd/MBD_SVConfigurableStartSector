# VS Code Setup for MBD_SVConfigurableStartSector

This workspace has been configured with VS Code settings and hotkeys for efficient development of your Star Valor mod.

## 🚀 Quick Start

1. **Open the workspace** in VS Code:
   - Open VS Code
   - File → Open Folder → Select `MBD_SVConfigurableStartSector` folder
   - Or open `MBD_SVConfigurableStartSector.code-workspace` file

2. **Install recommended extensions**:
   - C# (by Microsoft)
   - MSBuild project tools
   - C# Extensions

## ⌨️ Build Hotkeys

| Hotkey | Action | Description |
|--------|--------|-------------|
| `Ctrl+Shift+B` | Build Debug | Build the project in Debug mode |
| `Ctrl+Shift+R` | Build Release | Build the project in Release mode |
| `Ctrl+Shift+C` | Clean | Clean build artifacts |
| `Ctrl+Shift+D` | Rebuild | Clean and rebuild in Debug mode |

## 🔧 Available Tasks

### Local Build Tasks
- **build** - Build in Debug mode using MSBuild
- **build-release** - Build in Release mode using MSBuild
- **clean** - Clean build artifacts
- **rebuild** - Clean and rebuild in Debug mode

### Parent Workspace Build Tasks
- **build-from-parent** - Build using the parent workspace system (Debug)
- **build-release-from-parent** - Build using the parent workspace system (Release)

## 🎯 How to Use

### Using Hotkeys
1. Press `Ctrl+Shift+B` to build in Debug mode
2. The build output will appear in the Terminal panel
3. Success/failure will be shown in the status bar

### Using Command Palette
1. Press `Ctrl+Shift+P` to open command palette
2. Type "Tasks: Run Task"
3. Select your desired build task

### Using Terminal
```bash
# Debug build
msbuild MBD_SVConfigurableStartSector.sln /p:Configuration=Debug

# Release build
msbuild MBD_SVConfigurableStartSector.sln /p:Configuration=Release

# Clean
msbuild MBD_SVConfigurableStartSector.sln /t:Clean
```

## 📁 Project Structure

```
MBD_SVConfigurableStartSector/
├── .vscode/                    # VS Code configuration
│   ├── settings.json          # Editor settings
│   ├── tasks.json             # Build tasks
│   ├── launch.json            # Debug configurations
│   └── keybindings.json       # Custom hotkeys
├── ConfigurableStartSector.cs  # Main mod code
├── MBD_SVConfigurableStartSector.csproj
├── MBD_SVConfigurableStartSector.sln
└── Properties/
    └── AssemblyInfo.cs
```

## ⚙️ Configuration Details

### Build System
- **Target Framework**: .NET Framework 4.7.1
- **Platform**: Any CPU
- **MSBuild Path**: Visual Studio 2022 Community
- **Output**: Automatically copied to Star Valor plugins folder

### VS Code Settings
- **Solution**: `MBD_SVConfigurableStartSector.sln`
- **Excluded folders**: `bin/`, `obj/`
- **C# features**: Full IntelliSense, semantic highlighting, formatting

### Debug Configuration
- **Launch Star Valor with Mod** - Builds and launches the game
- **Launch Star Valor with Mod (Release)** - Builds Release and launches
- **Attach to Star Valor** - Attach debugger to running game

## 🔍 Troubleshooting

### Build Issues
1. **MSBuild not found**: Ensure Visual Studio 2022 is installed
2. **Reference errors**: Check that Star Valor and BepInEx are installed
3. **Permission errors**: Run VS Code as administrator if needed

### Hotkey Issues
1. **Hotkeys not working**: Check if another extension is using the same keys
2. **Task not found**: Reload VS Code window (`Ctrl+Shift+P` → "Developer: Reload Window")

### Debug Issues
1. **Can't attach to Star Valor**: Make sure the game is running
2. **Breakpoints not hitting**: Ensure you're building in Debug mode

## 📚 Additional Resources

- [Star Valor Modding Guide](../DEVELOPMENT_SETUP.md)
- [BepInEx Documentation](https://docs.bepinex.dev/)
- [Harmony Documentation](https://github.com/pardeike/Harmony/wiki)

## 🎮 Testing Your Mod

1. Build the project (`Ctrl+Shift+B`)
2. Launch Star Valor
3. Check the BepInEx console for mod loading messages
4. Test your mod functionality in-game

## 🔄 Development Workflow

1. **Make changes** to your code
2. **Build** (`Ctrl+Shift+B`)
3. **Test** in Star Valor
4. **Iterate** as needed
5. **Commit** your changes to git

The build system automatically copies your DLL to the Star Valor plugins folder, so you can test immediately after building! 