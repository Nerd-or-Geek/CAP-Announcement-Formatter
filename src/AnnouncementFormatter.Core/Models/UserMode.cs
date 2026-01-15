namespace AnnouncementFormatter.Core.Models;

/// <summary>
/// Represents the user's proficiency level, which determines UI complexity and available features.
/// </summary>
public enum UserMode
{
    /// <summary>
    /// Beginner: Drag-and-drop only, no code exposure, simple forms.
    /// </summary>
    Beginner,

    /// <summary>
    /// Intermediate: Can reorder widgets, edit properties, toggle sections.
    /// </summary>
    Intermediate,

    /// <summary>
    /// Expert/Developer: Full access to widget definitions, layouts, and variables.
    /// </summary>
    Expert
}
