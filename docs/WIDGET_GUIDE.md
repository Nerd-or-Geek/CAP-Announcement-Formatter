# Widget Development Guide

## Overview

Widgets are the building blocks of announcements in the CAP Announcement Formatter. They are defined using JSON configuration files and rendered using HTML templates.

## Widget Definition Structure

A widget definition is a JSON file with the following structure:

```json
{
  "id": "unique_widget_id",
  "displayName": "Human Readable Name",
  "category": "Category Name",
  "description": "Brief description of the widget",
  "icon": "icon_filename.png",
  "template": "template_filename.html",
  "version": "1.0.0",
  "allowedModes": ["Beginner", "Intermediate", "Expert"],
  "fields": [
    {
      "id": "field_id",
      "type": "String",
      "label": "Field Label",
      "required": true,
      "defaultValue": "default",
      "placeholder": "Placeholder text",
      "helpText": "Help text for users",
      "options": ["option1", "option2"],
      "validationPattern": "regex pattern"
    }
  ]
}
```

## Field Types

- **String**: Single-line text input
- **Multiline**: Multi-line text area
- **Date**: Date picker
- **DateTime**: Date and time picker
- **Number**: Numeric input
- **Boolean**: Checkbox (true/false)
- **Dropdown**: Select from predefined options
- **Email**: Email address input with validation
- **Url**: URL input with validation

## Creating a Widget

### Step 1: Create Widget Definition

Create a new JSON file in `assets/widgets/` directory:

**Example: `custom_widget.json`**

```json
{
  "id": "custom_widget",
  "displayName": "Custom Widget",
  "category": "Custom",
  "description": "A custom widget example",
  "template": "custom.html",
  "fields": [
    {
      "id": "title",
      "type": "String",
      "label": "Title",
      "required": true
    },
    {
      "id": "content",
      "type": "Multiline",
      "label": "Content",
      "required": true
    }
  ]
}
```

### Step 2: Create HTML Template

Create a corresponding HTML template in `assets/templates/`:

**Example: `custom.html`**

```html
<div class="widget custom-widget" style="margin-bottom: 25px; padding: 20px; border-left: 4px solid #9b59b6; background-color: #f8f5fb;">
    <div class="widget-title" style="font-size: 1.4em; color: #2c3e50; margin-bottom: 15px; font-weight: bold;">
        {{title}}
    </div>
    <div class="widget-content">
        <p style="margin: 0; white-space: pre-wrap; line-height: 1.6;">{{content}}</p>
    </div>
</div>
```

### Step 3: Use Variable Substitution

In your HTML template, use `{{field_id}}` syntax to insert field values:

- `{{title}}` - Inserts the value of the "title" field
- `{{content}}` - Inserts the value of the "content" field

## Template Best Practices

1. **Use Inline CSS**: All styles must be inline for email compatibility
2. **Include widget class**: Add `class="widget"` to the root element
3. **Avoid External Resources**: No external images, scripts, or stylesheets
4. **Responsive Design**: Use percentage-based widths when possible
5. **Print-Friendly**: Use `page-break-inside: avoid` for widgets

## Example Widgets

The application comes with several example widgets:

- **meeting_basic.json**: Basic meeting announcement
- **alert_important.json**: Important alerts and warnings
- **info_card.json**: General information cards
- **regulation_change.json**: Formal regulation changes

Study these examples to understand widget structure and best practices.

## Widget Categories

Organize widgets into logical categories:

- **Meetings**: Meeting announcements
- **Alerts**: Urgent notifications and warnings
- **General**: General-purpose information
- **Regulations**: Policy and regulation changes
- **Events**: Event announcements
- **Custom**: User-defined categories

## User Mode Restrictions

Control widget visibility by mode:

```json
"allowedModes": ["Intermediate", "Expert"]
```

- Omit this field to allow all modes
- Restrict complex widgets to advanced users

## Validation

Add validation patterns for fields:

```json
{
  "id": "email",
  "type": "Email",
  "label": "Email Address",
  "validationPattern": "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$"
}
```

## Testing Your Widget

1. Save your JSON definition to `assets/widgets/`
2. Save your HTML template to `assets/templates/`
3. Restart the application
4. The widget should appear in the widget library
5. Drag it onto the document canvas to test

## Advanced Features

### Conditional Sections

Use field values to show/hide sections (Expert mode):

```html
<!-- In future versions, this will support conditionals -->
<div class="conditional-section">
    {{#if showSection}}
    <p>This section is conditional</p>
    {{/if}}
</div>
```

### Custom Styling

Each widget can have unique styling:

```html
<style>
    /* Widget-specific styles */
    .custom-widget .special-text {
        color: #e74c3c;
        font-weight: bold;
    }
</style>
```

## Troubleshooting

**Widget not appearing?**
- Check JSON syntax is valid
- Ensure "id" is unique
- Verify template file exists
- Check file names match

**Template not rendering?**
- Verify field IDs match between JSON and HTML
- Check for typos in `{{variable}}` syntax
- Ensure template file is in correct directory

**Fields not updating?**
- Restart the application
- Check field "id" values
- Verify field type is supported

## Publishing Widgets

To share widgets with others:

1. Export your JSON and HTML files
2. Include documentation
3. Test on a clean installation
4. Share via file transfer (no cloud required)

## Future Enhancements

Planned features for future versions:

- Widget marketplace (offline)
- Widget inheritance
- Conditional logic in templates
- Custom validators
- Widget versioning and updates
