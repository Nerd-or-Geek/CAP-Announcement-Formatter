# Architecture Overview

## Technology Stack

### Frontend/UI Layer
- **Framework**: Avalonia UI 11.0+
- **Pattern**: MVVM (Model-View-ViewModel)
- **Language**: C# with .NET 8
- **UI Components**: 
  - FluentTheme for modern appearance
  - Custom controls for drag-and-drop
  - Native platform controls where possible

### Core Business Logic
- **Library**: AnnouncementFormatter.Core
- **Responsibilities**:
  - Domain models
  - Business logic
  - Data persistence
  - Rendering engine
  - Widget management

### Data Layer
- **Document Format**: XML
- **Widget Definitions**: JSON
- **Templates**: HTML with inline CSS
- **Storage**: Local file system

### Preview Engine
- **Component**: WebView2 (Windows)
- **Fallback**: Platform-specific WebView
- **Rendering**: HTML/CSS only, no JavaScript

## Project Structure

```
CAP-Announcement-Formatter/
├── src/
│   ├── AnnouncementFormatter/           # Main UI Application
│   │   ├── Views/                       # XAML views
│   │   ├── ViewModels/                  # View models (MVVM)
│   │   ├── Controls/                    # Custom UI controls
│   │   ├── Converters/                  # Value converters
│   │   ├── Assets/                      # UI assets (icons, etc.)
│   │   ├── App.axaml                    # Application entry
│   │   └── Program.cs                   # Main entry point
│   │
│   └── AnnouncementFormatter.Core/      # Business Logic
│       ├── Models/                      # Domain models
│       │   ├── UserMode.cs
│       │   ├── FieldType.cs
│       │   ├── WidgetDefinition.cs
│       │   ├── WidgetField.cs
│       │   ├── DocumentWidget.cs
│       │   ├── DocumentFieldValue.cs
│       │   └── AnnouncementDocument.cs
│       │
│       └── Services/                    # Business services
│           ├── WidgetLibraryService.cs  # Widget management
│           ├── DocumentPersistenceService.cs
│           └── HtmlRenderingService.cs
│
├── assets/
│   ├── widgets/                         # Widget definitions (JSON)
│   ├── templates/                       # HTML templates
│   ├── icons/                          # Widget icons
│   └── examples/                       # Example documents
│
└── docs/                               # Documentation
```

## Component Responsibilities

### AnnouncementFormatter (UI Project)

**ViewModels:**
- `MainWindowViewModel`: Orchestrates entire application
- `DocumentWidgetViewModel`: Represents widget instance in UI

**Views:**
- `MainWindow`: Primary application window
- Custom controls for widget library, canvas, properties

**Responsibilities:**
- User interaction
- Visual presentation
- Mode switching
- Drag-and-drop coordination

### AnnouncementFormatter.Core (Business Logic)

**Models:**
- Define domain entities
- Data structures
- Enumerations

**Services:**
- `WidgetLibraryService`: Load/manage widget definitions
- `DocumentPersistenceService`: Save/load documents (XML)
- `HtmlRenderingService`: Render documents to HTML

**Responsibilities:**
- Business rules
- Data validation
- File I/O
- Rendering logic

## Data Flow

### Adding a Widget

```
1. User clicks widget in library (UI)
   ↓
2. MainWindowViewModel.AddWidget() called
   ↓
3. Create DocumentWidget instance
   ↓
4. Initialize with default field values
   ↓
5. Add to document and UI collection
   ↓
6. Trigger preview update
   ↓
7. HtmlRenderingService generates HTML
   ↓
8. WebView displays preview
```

### Saving a Document

```
1. User clicks Save button (UI)
   ↓
2. MainWindowViewModel.SaveDocumentAsync()
   ↓
3. DocumentPersistenceService.SaveDocumentAsync()
   ↓
4. Convert document model to XML structure
   ↓
5. Write XML to file system
   ↓
6. Update UI state (no unsaved changes)
```

### Loading Widgets

```
1. Application startup
   ↓
2. WidgetLibraryService.LoadWidgetsAsync()
   ↓
3. Scan assets/widgets/ directory
   ↓
4. Deserialize each JSON file
   ↓
5. Validate widget definitions
   ↓
6. Store in memory dictionary
   ↓
7. Populate UI widget library
   ↓
8. Filter by current user mode
```

## Design Patterns

### MVVM (Model-View-ViewModel)

**Models** (Core.Models):
- Pure data structures
- No UI dependencies
- Business logic

**Views** (AXAML files):
- Pure UI markup
- Data binding expressions
- No business logic

**ViewModels**:
- UI state management
- Commands
- Property change notifications
- Coordinate services

### Service Layer

All business logic encapsulated in services:
- Single responsibility
- Testable in isolation
- Dependency injection ready
- Reusable across UI layers

### Repository Pattern

DocumentPersistenceService acts as repository:
- Abstract storage details
- Consistent API
- Easy to swap implementations

## Mode-Based Feature Control

### Beginner Mode
- Limited widget selection
- Simple forms only
- No delete/reorder
- Locked layouts

### Intermediate Mode
- More widgets available
- Full editing capabilities
- Reorder/delete enabled
- Property editing

### Expert Mode
- All widgets
- File system access
- Widget creation
- Template editing
- Advanced properties

Implementation:
```csharp
public IEnumerable<WidgetDefinition> GetWidgetsForMode(UserMode mode)
{
    return _widgets.Values.Where(w => 
        w.AllowedModes == null || 
        w.AllowedModes.Contains(mode));
}
```

## Widget System Architecture

### Definition (JSON)
```json
{
  "id": "unique_id",
  "displayName": "User-facing name",
  "fields": [...],
  "template": "template.html"
}
```

### Template (HTML)
```html
<div class="widget">
    <h2>{{field_id}}</h2>
</div>
```

### Instance (in document)
```csharp
new DocumentWidget {
    WidgetDefinitionId = "unique_id",
    Fields = [
        new DocumentFieldValue { 
            Name = "field_id", 
            Value = "user input" 
        }
    ]
}
```

### Rendering
```csharp
// Load template
var template = File.ReadAllText(templatePath);

// Replace variables
foreach (var field in widget.Fields) {
    template = template.Replace(
        $"{{{{{field.Name}}}}}", 
        EscapeHtml(field.Value)
    );
}
```

## Extensibility Points

### Custom Widgets
- Add JSON definition to `assets/widgets/`
- Add HTML template to `assets/templates/`
- No code changes required

### Custom Renderers
- Implement new rendering service
- Replace HtmlRenderingService
- Support PDF, Word, etc.

### Custom Storage
- Implement persistence interface
- Support database, cloud, etc.
- Maintain XML compatibility

### Platform Support
- Avalonia handles cross-platform
- WebView varies by platform
- Platform-specific implementations available

## Security Considerations

### Offline Only
- No network communication
- No external dependencies
- All processing local

### File System Access
- User documents folder
- Application assets folder
- No system files

### HTML Rendering
- No JavaScript execution
- HTML sanitization
- CSS only (inline)

### Data Validation
- Input validation on fields
- XML schema validation
- File type verification

## Performance Considerations

### Widget Loading
- Lazy loading from disk
- Cached in memory
- Async operations

### Preview Rendering
- Debounced updates
- Async HTML generation
- Incremental updates

### Document Size
- Practical limit: 50-100 widgets
- XML compression possible
- Pagination future enhancement

## Testing Strategy

### Unit Tests
- Core business logic
- Services in isolation
- Model validation

### Integration Tests
- Widget loading
- Document persistence
- Rendering pipeline

### UI Tests
- User interactions
- Mode switching
- Drag-and-drop

### Manual Tests
- Cross-platform compatibility
- UI responsiveness
- Accessibility

## Build and Deployment

### Development Build
```bash
dotnet build
dotnet run --project src/AnnouncementFormatter
```

### Release Build
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

### Platform-Specific
- Windows: win-x64, win-x86, win-arm64
- macOS: osx-x64, osx-arm64
- Linux: linux-x64, linux-arm64

### Distribution
- Self-contained executable
- Include runtime
- Single folder deployment
- No installer required

## Future Architecture Enhancements

### Plugin System
- Dynamic widget loading
- Third-party widgets
- Marketplace (offline)

### Template Engine
- Advanced templating
- Conditional logic
- Loops and iteration

### Undo/Redo
- Command pattern
- State history
- Memory management

### Collaboration
- Merge documents
- Change tracking
- Conflict resolution

### Export Formats
- PDF generation
- Word document export
- Email integration
