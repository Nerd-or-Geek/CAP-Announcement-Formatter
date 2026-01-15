# Development Quick Start Guide

## Prerequisites

Before you begin, ensure you have the following installed:

1. **.NET 8 SDK** or later
   - Download: https://dotnet.microsoft.com/download
   - Verify: `dotnet --version`

2. **Visual Studio 2022** (optional but recommended)
   - Community Edition (free): https://visualstudio.microsoft.com/
   - Workload: .NET Desktop Development
   
   OR

3. **JetBrains Rider** (optional)
   - Download: https://www.jetbrains.com/rider/

4. **VS Code** with C# extension (alternative)
   - Download: https://code.visualstudio.com/
   - Extension: C# Dev Kit

## Getting Started

### 1. Clone or Download

If you received this as a file:
- Extract to a folder of your choice

If using Git:
```bash
cd C:\Users\cadet\Documents\GitHub
git init CAP-Announcement-Formatter
cd CAP-Announcement-Formatter
```

### 2. Build the Application

#### Using PowerShell (Recommended for Windows):

```powershell
# Navigate to project directory
cd C:\Users\cadet\Documents\GitHub\CAP-Announcement-Formatter

# Run build script
.\build.ps1
```

#### Using Command Line:

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Copy assets to output
# (Manual step - copy 'assets' folder to bin\Debug\net8.0\Assets)
```

### 3. Run the Application

#### Using PowerShell:

```powershell
.\run.ps1
```

#### Using Command Line:

```bash
dotnet run --project src\AnnouncementFormatter
```

#### Using Visual Studio:

1. Open `CAP-Announcement-Formatter.sln`
2. Set `AnnouncementFormatter` as startup project
3. Press F5 to run

## Project Structure

```
CAP-Announcement-Formatter/
├── CAP-Announcement-Formatter.sln    # Solution file
├── README.md                          # Project overview
├── build.ps1                          # Build script
├── run.ps1                            # Run script
├── publish.ps1                        # Publish script
│
├── src/
│   ├── AnnouncementFormatter/         # Main application (Avalonia UI)
│   │   ├── Views/                     # XAML views
│   │   ├── ViewModels/                # View models
│   │   ├── App.axaml                  # Application definition
│   │   └── Program.cs                 # Entry point
│   │
│   └── AnnouncementFormatter.Core/    # Business logic library
│       ├── Models/                    # Domain models
│       └── Services/                  # Business services
│
├── assets/
│   ├── widgets/                       # Widget definitions (JSON)
│   ├── templates/                     # HTML templates
│   ├── icons/                         # Icons (add your own)
│   └── examples/                      # Example documents
│
└── docs/
    ├── USER_GUIDE.md                  # User documentation
    ├── WIDGET_GUIDE.md                # Widget development
    ├── ARCHITECTURE.md                # Technical architecture
    └── QUICK_START.md                 # This file
```

## Development Workflow

### Making Changes

1. **UI Changes**: Edit `.axaml` files in `src/AnnouncementFormatter/Views/`
2. **Business Logic**: Edit `.cs` files in `src/AnnouncementFormatter.Core/`
3. **ViewModels**: Edit `.cs` files in `src/AnnouncementFormatter/ViewModels/`

### Hot Reload

Avalonia supports hot reload for XAML changes:
1. Run the application
2. Edit .axaml files
3. Changes apply automatically

### Debugging

#### Visual Studio:
- Set breakpoints by clicking line numbers
- Press F5 to start debugging
- Use Debug windows (Locals, Watch, Call Stack)

#### VS Code:
- Install C# Dev Kit extension
- Press F5 to launch debugger
- Use Debug sidebar for breakpoints

#### Rider:
- Click gutter to set breakpoints
- Press Shift+F9 to debug
- Use debugger windows

### Testing Changes

1. **Build**: `.\build.ps1`
2. **Run**: `.\run.ps1`
3. **Test**: Interact with the application
4. **Verify**: Check console output for errors

## Common Tasks

### Add a New Widget

1. Create JSON in `assets/widgets/`:
```json
{
  "id": "my_widget",
  "displayName": "My Widget",
  "category": "Custom",
  "template": "my_template.html",
  "fields": [...]
}
```

2. Create HTML in `assets/templates/`:
```html
<div class="widget">
    <h2>{{title}}</h2>
</div>
```

3. Restart application to load new widget

### Modify UI Layout

1. Open `src/AnnouncementFormatter/Views/MainWindow.axaml`
2. Edit XAML markup
3. Save and see hot reload (if running)
4. Or rebuild and run

### Add Business Logic

1. Open `src/AnnouncementFormatter.Core/Services/`
2. Add new service or modify existing
3. Update ViewModels to use service
4. Rebuild solution

## Building for Distribution

### Windows Executable

```powershell
# Build self-contained Windows app
.\publish.ps1 -Runtime win-x64

# Output: publish\win-x64\AnnouncementFormatter.exe
```

### Single File Executable

```powershell
# Build single executable file
.\publish.ps1 -Runtime win-x64 -SingleFile
```

### Other Platforms

```powershell
# macOS (Intel)
.\publish.ps1 -Runtime osx-x64

# macOS (Apple Silicon)
.\publish.ps1 -Runtime osx-arm64

# Linux
.\publish.ps1 -Runtime linux-x64
```

## Troubleshooting

### Build Fails

**Error: SDK not found**
- Install .NET 8 SDK
- Verify: `dotnet --version`

**Error: Missing NuGet packages**
- Run: `dotnet restore`
- Check internet connection (for first build)

**Error: Assets not found**
- Ensure `assets` folder exists
- Run build script to copy assets

### Application Won't Start

**Error: Missing DLL**
- Rebuild: `.\build.ps1`
- Check output directory has all files

**Error: Widgets not loading**
- Check `assets/widgets/` has JSON files
- Verify JSON syntax is valid
- Check console output for errors

**Error: Preview not working**
- WebView2 runtime required (Windows)
- Download: https://developer.microsoft.com/microsoft-edge/webview2/

### Runtime Errors

**Check Console Output**:
```powershell
# Run with verbose logging
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project src\AnnouncementFormatter
```

**Common Issues**:
- Invalid JSON in widget definitions
- Missing template files
- Incorrect file paths
- Permission issues (file access)

## IDE Setup

### Visual Studio 2022

1. Open `CAP-Announcement-Formatter.sln`
2. Right-click solution → Restore NuGet Packages
3. Build → Build Solution (Ctrl+Shift+B)
4. Debug → Start Debugging (F5)

### VS Code

1. Open folder: `CAP-Announcement-Formatter`
2. Install extensions:
   - C# Dev Kit
   - Avalonia for VS Code
3. Press F5 to launch
4. Select ".NET Core" launch configuration

### Rider

1. Open `CAP-Announcement-Formatter.sln`
2. Wait for NuGet restore
3. Run → Debug (Shift+F9)
4. Use Rider's refactoring tools

## Next Steps

### For Users
- Read [USER_GUIDE.md](USER_GUIDE.md)
- Try the application
- Create your first announcement

### For Widget Developers
- Read [WIDGET_GUIDE.md](WIDGET_GUIDE.md)
- Study example widgets
- Create custom widgets

### For Developers
- Read [ARCHITECTURE.md](ARCHITECTURE.md)
- Explore codebase
- Make modifications

## Resources

### Documentation
- [README.md](../README.md) - Project overview
- [USER_GUIDE.md](USER_GUIDE.md) - User documentation
- [WIDGET_GUIDE.md](WIDGET_GUIDE.md) - Widget development
- [ARCHITECTURE.md](ARCHITECTURE.md) - Technical details

### External Resources
- Avalonia UI: https://avaloniaui.net/
- .NET Documentation: https://learn.microsoft.com/dotnet/
- MVVM Pattern: https://learn.microsoft.com/dotnet/architecture/

### Support
This is an offline application. All documentation is included.

## Contributing

If you're extending this application:

1. Follow existing code style
2. Update documentation
3. Test on target platform
4. Consider backward compatibility

## License

[Add your license information here]

---

**Quick Commands Reference**

```powershell
# Build
.\build.ps1

# Run
.\run.ps1

# Publish for Windows
.\publish.ps1 -Runtime win-x64

# Clean build
Remove-Item -Recurse -Force src\*\bin,src\*\obj
.\build.ps1

# Run tests (when implemented)
dotnet test
```

**Need Help?**

Check documentation in `docs/` folder or console output for error messages.
