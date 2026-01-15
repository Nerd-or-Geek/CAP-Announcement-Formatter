# User Guide - CAP Announcement Formatter

## Getting Started

The CAP Announcement Formatter is a desktop application for creating professional announcement documents without writing code.

## Interface Overview

### Top Menu Bar

- **New**: Create a new document
- **Save**: Save current document (XML format)
- **Export HTML**: Export document as HTML file
- **Mode Buttons**: Switch between Beginner, Intermediate, and Expert modes

### Main Areas

1. **Left Panel**: Widget Library - Available widgets to add to your document
2. **Center Panel**: Document Canvas - Your working document
3. **Right Panel**: Properties - Document and widget properties
4. **Bottom Panel**: Live Preview - See your document as it will appear

## User Modes

### 🟢 Beginner Mode

**Perfect for:** Non-technical users who need to create announcements quickly

**Features:**
- Simple drag-and-drop interface
- Pre-configured widgets
- Basic form fields only
- No technical options exposed

**How to use:**
1. Click a widget in the left panel to add it
2. Fill in the form fields
3. See live preview at the bottom
4. Export to HTML when done

### 🟡 Intermediate Mode

**Perfect for:** Users comfortable with more options

**Features:**
- All Beginner features plus:
- Reorder widgets (up/down arrows)
- Delete widgets
- Edit widget properties
- Toggle optional sections

**How to use:**
1. Add widgets by clicking
2. Use ▲▼ buttons to reorder
3. Use ✕ button to remove widgets
4. Edit all field values

### 🔴 Expert / Developer Mode

**Perfect for:** Developers and power users

**Features:**
- All Intermediate features plus:
- Create new widgets (JSON)
- Edit widget templates (HTML)
- Access layout rules (XML)
- Define custom fields
- Set validation rules

**How to use:**
1. Create widgets in `assets/widgets/`
2. Create templates in `assets/templates/`
3. Design tools for non-technical users
4. Test and refine

## Creating Your First Document

### Step 1: Choose Your Mode

Click the appropriate mode button (Beginner recommended for first-time users)

### Step 2: Add Widgets

Click on any widget in the left panel to add it to your document:

- **Meeting Announcement**: For meetings, briefings, events
- **Important Alert**: For urgent notifications
- **Information Card**: For general announcements
- **Regulation Change**: For policy updates (Intermediate/Expert only)

### Step 3: Fill in Details

Each widget will show form fields. Enter your information:

- Required fields are marked
- Hover over field labels for help text
- Use appropriate formats (dates, times, etc.)

### Step 4: Arrange Widgets

In Intermediate or Expert mode:

- Click ▲ to move widget up
- Click ▼ to move widget down
- Click ✕ to remove widget

### Step 5: Preview

Check the preview panel at the bottom to see how your document will look

### Step 6: Save or Export

- **Save**: Saves as XML (can edit later)
- **Export HTML**: Creates final HTML file (ready to print or email)

## Working with Widgets

### Meeting Announcement Widget

**Fields:**
- Meeting Title: Brief, descriptive title
- Date: Select from calendar
- Time: Format as "09:00 - 10:00"
- Location: Where the meeting takes place
- Details: Agenda, special instructions
- Attendance Required: Check if mandatory

**Best for:** Weekly briefings, training sessions, conferences

### Important Alert Widget

**Fields:**
- Alert Title: Clear, attention-grabbing
- Severity Level: Info, Warning, or Critical
- Effective Date/Time: When alert takes effect
- Alert Message: Complete description
- Action Required: What people should do
- Contact Information: Who to contact

**Best for:** Emergency notices, system maintenance, urgent updates

### Information Card Widget

**Fields:**
- Title: Brief heading
- Content: Main message
- Date: Optional date reference

**Best for:** General announcements, reminders, informational updates

### Regulation Change Widget

*Available in Intermediate/Expert mode only*

**Fields:**
- Regulation Number: Official identifier
- Title: Official regulation title
- Effective Date: When it takes effect
- Summary: High-level overview
- Detailed Changes: Complete description
- References: Related documents
- Issuing Authority: Who issued it

**Best for:** Policy changes, procedural updates, formal regulations

## Document Management

### Saving Documents

**Format:** XML (AnnouncementDocument format)

**Location:** Documents folder by default

**File name:** Based on document title

**Contains:**
- All widget data
- Field values
- Document metadata
- Creation/modification dates

### Opening Documents

Use the Open button to load previously saved documents

**Note:** Only files in the correct XML format can be opened

### Exporting to HTML

**Purpose:** Create final output for distribution

**Format:** HTML with inline CSS

**Compatible with:**
- Email clients
- Web browsers
- Printing
- PDF converters

**Location:** Documents folder

## Tips and Best Practices

### Content Tips

1. **Be Clear and Concise**: Use simple, direct language
2. **Include All Details**: Date, time, location, contact info
3. **Use Appropriate Widget**: Choose the widget that fits your content
4. **Proofread**: Check spelling and formatting before exporting

### Organization Tips

1. **Use Categories**: Group similar information together
2. **Logical Order**: Put most important information first
3. **Consistent Style**: Use similar formatting for similar content
4. **Test Preview**: Always check preview before exporting

### Mode Selection Tips

- **Start with Beginner**: Learn the basics first
- **Move to Intermediate**: When you need more control
- **Use Expert**: Only for creating custom widgets

## Keyboard Shortcuts

*Coming in future version*

## Troubleshooting

### Widget not appearing in library?

- Check widget JSON file is in `assets/widgets/`
- Verify JSON syntax is correct
- Restart application

### Preview not updating?

- Make a small change to any field
- Click outside the text box
- Wait a moment for refresh

### Can't export HTML?

- Ensure document has a title
- Check you have write permissions
- Verify output folder exists

### Document won't open?

- Ensure it's a valid XML file
- Check it was created by this application
- Try creating a new document instead

## Getting Help

### Documentation

- User Guide: This document
- Widget Guide: For creating custom widgets (Expert mode)
- README.md: Project overview and technical details

### Support

This is an offline application with no online support. Refer to documentation included with the application.

## Privacy and Security

✅ **Offline Only**: No internet connection required or used

✅ **No Accounts**: No registration or login needed

✅ **No Telemetry**: No usage data collected

✅ **Local Storage**: All data stays on your computer

✅ **No Cloud**: No cloud services or external dependencies

## Advanced Features (Expert Mode)

### Creating Custom Widgets

See [Widget Development Guide](WIDGET_GUIDE.md) for detailed instructions.

**Quick start:**
1. Copy an existing widget JSON file
2. Modify fields and metadata
3. Create corresponding HTML template
4. Test in Expert mode

### Editing Templates

Templates use HTML with inline CSS and variable substitution:

```html
<div class="widget">
    <h2>{{title}}</h2>
    <p>{{content}}</p>
</div>
```

### Document Structure (XML)

Documents are saved in XML format:

```xml
<Document Id="..." Title="..." CreatedAt="..." ModifiedAt="...">
    <Metadata>
        <Property Key="..." Value="..."/>
    </Metadata>
    <Widgets>
        <Widget Id="..." DefinitionId="..." Order="...">
            <Field Name="..."><![CDATA[...]]></Field>
        </Widget>
    </Widgets>
</Document>
```

## Future Features

Planned enhancements:

- PDF export
- Print preview
- Templates for common document types
- Undo/redo
- Keyboard shortcuts
- Drag-and-drop reordering
- Widget search
- Custom themes

## Version History

### Version 1.0.0 (Current)

- Initial release
- Three user modes
- Four example widgets
- XML document persistence
- HTML export
- Live preview
