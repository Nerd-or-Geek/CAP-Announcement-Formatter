# Contributing to CAP Announcement Formatter

Thank you for your interest in contributing to the CAP Announcement Formatter project!

## Getting Started

1. Read the [QUICK_START.md](QUICK_START.md) guide
2. Familiarize yourself with the [ARCHITECTURE.md](ARCHITECTURE.md)
3. Review existing code and patterns

## Development Setup

Ensure you have:
- .NET 8 SDK or later
- IDE of your choice (Visual Studio, Rider, or VS Code)
- Git (if contributing via version control)

## Code Style

### C# Conventions

- Follow Microsoft C# Coding Conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and short

Example:
```csharp
/// <summary>
/// Loads all widget definitions from the configured directory.
/// </summary>
/// <returns>A task representing the asynchronous operation.</returns>
public async Task LoadWidgetsAsync()
{
    // Implementation
}
```

### XAML Conventions

- Use proper indentation (4 spaces)
- Group related properties
- Use meaningful x:Name attributes
- Follow Avalonia UI guidelines

### File Organization

- One class per file
- File name matches class name
- Group related classes in folders
- Keep folder structure flat

## Making Changes

### For Bug Fixes

1. Identify the bug and root cause
2. Write a test that reproduces the bug (if applicable)
3. Fix the issue
4. Verify the test passes
5. Test manually

### For New Features

1. Discuss the feature (if major change)
2. Design the solution
3. Implement incrementally
4. Add documentation
5. Test thoroughly

### For New Widgets

1. Follow [WIDGET_GUIDE.md](WIDGET_GUIDE.md)
2. Create JSON definition
3. Create HTML template
4. Test with all user modes
5. Document usage

## Testing

### Manual Testing

1. Build the application
2. Test affected functionality
3. Test on target platform (Windows primarily)
4. Verify no regressions
5. Test with different user modes

### Test Checklist

- [ ] Application starts successfully
- [ ] Can create new document
- [ ] Can add widgets
- [ ] Can edit widget fields
- [ ] Can reorder widgets (Intermediate/Expert mode)
- [ ] Can delete widgets (Intermediate/Expert mode)
- [ ] Preview updates correctly
- [ ] Can save document
- [ ] Can load document
- [ ] Can export to HTML
- [ ] Mode switching works
- [ ] No errors in console

## Pull Request Process

If using version control:

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Update documentation
5. Submit pull request with description

## Documentation

When making changes, update relevant documentation:

- **Code changes**: Update XML comments
- **Architecture changes**: Update ARCHITECTURE.md
- **New features**: Update USER_GUIDE.md
- **New widgets**: Update WIDGET_GUIDE.md
- **Breaking changes**: Update README.md

## Widget Development Guidelines

### JSON Schema

Follow the established schema:
```json
{
  "id": "unique_identifier",
  "displayName": "Human Readable Name",
  "category": "Category",
  "description": "Brief description",
  "template": "template.html",
  "fields": [...]
}
```

### HTML Templates

- Use inline CSS only
- No external resources
- No JavaScript
- Email-safe HTML
- Print-friendly styles

### Field Types

Use appropriate field types:
- `String`: Short text
- `Multiline`: Long text
- `Date`: Date selection
- `DateTime`: Date and time
- `Boolean`: Yes/no checkbox
- `Dropdown`: Selection from options

## Commit Messages

Use clear, descriptive commit messages:

**Good:**
- "Add meeting widget with attendance tracking"
- "Fix XML save error with special characters"
- "Update preview rendering to handle empty fields"

**Bad:**
- "Update"
- "Fix bug"
- "Changes"

## Code Review

Before submitting:

1. **Self-review**: Read your own changes
2. **Test**: Verify functionality works
3. **Clean**: Remove debug code, comments
4. **Format**: Consistent style
5. **Document**: Add necessary documentation

## Performance Considerations

- Use async/await for I/O operations
- Avoid blocking UI thread
- Cache data when appropriate
- Profile before optimizing

## Accessibility

Consider accessibility:
- Keyboard navigation
- Screen reader compatibility
- High contrast support
- Font size scaling

## Platform Compatibility

Test on:
- Windows 10 and 11 (primary)
- macOS (if possible)
- Linux (if possible)

## Security

Remember:
- No network communication
- Local file system only
- Sanitize user input
- Validate file formats

## Breaking Changes

If making breaking changes:
- Document clearly
- Provide migration path
- Update version number
- Consider backward compatibility

## Questions?

Refer to existing code for examples and patterns. All documentation is in the `docs/` folder.

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to make this project better!
