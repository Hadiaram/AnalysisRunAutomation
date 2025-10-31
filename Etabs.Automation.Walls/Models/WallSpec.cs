namespace Etabs.Automation.Walls.Models;

/// <summary>
/// Specification for a wall area object, including geometry, section, orientation, and design labels.
/// </summary>
public class WallSpec
{
    /// <summary>
    /// Unique identifier/name for this wall (e.g., "W_Story1_1")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Story name where the wall is located
    /// </summary>
    public string StoryName { get; set; } = string.Empty;

    /// <summary>
    /// Wall section property name (e.g., "WALL_200mm_C30")
    /// </summary>
    public string SectionName { get; set; } = string.Empty;

    /// <summary>
    /// Wall thickness in millimeters
    /// </summary>
    public double ThicknessMm { get; set; }

    /// <summary>
    /// Material name for the wall section
    /// </summary>
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>
    /// X coordinates of wall corner points (meters)
    /// </summary>
    public double[] XCoords { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Y coordinates of wall corner points (meters)
    /// </summary>
    public double[] YCoords { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Z coordinates of wall corner points (meters)
    /// </summary>
    public double[] ZCoords { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Local axis rotation angle in degrees (β)
    /// </summary>
    public double LocalAxisAngleDeg { get; set; } = 0.0;

    /// <summary>
    /// Optional pier label for design grouping
    /// </summary>
    public string? PierLabel { get; set; }

    /// <summary>
    /// Optional spandrel label for design grouping
    /// </summary>
    public string? SpandrelLabel { get; set; }

    /// <summary>
    /// Openings to be added to this wall
    /// </summary>
    public List<OpeningSpec> Openings { get; set; } = new();

    /// <summary>
    /// Validates the wall specification.
    /// </summary>
    /// <param name="errors">Output: list of validation error messages</param>
    /// <returns>True if valid</returns>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Wall name is required");

        if (string.IsNullOrWhiteSpace(StoryName))
            errors.Add("Story name is required");

        if (string.IsNullOrWhiteSpace(SectionName))
            errors.Add("Section name is required");

        if (ThicknessMm <= 0)
            errors.Add("Thickness must be greater than 0");

        if (string.IsNullOrWhiteSpace(MaterialName))
            errors.Add("Material name is required");

        if (XCoords.Length < 3)
            errors.Add("At least 3 corner points are required");

        if (XCoords.Length != YCoords.Length || XCoords.Length != ZCoords.Length)
            errors.Add("Coordinate arrays must have the same length");

        // Validate openings
        foreach (var opening in Openings)
        {
            if (!opening.Validate(out var openingErrors))
            {
                errors.AddRange(openingErrors.Select(e => $"Opening '{opening.Name}': {e}"));
            }
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Creates a rectangular wall specification.
    /// </summary>
    /// <param name="name">Wall name</param>
    /// <param name="storyName">Story name</param>
    /// <param name="sectionName">Section property name</param>
    /// <param name="x0">Starting X coordinate (meters)</param>
    /// <param name="y0">Starting Y coordinate (meters)</param>
    /// <param name="z0">Bottom Z coordinate (meters)</param>
    /// <param name="lengthM">Wall length (meters)</param>
    /// <param name="heightM">Wall height (meters)</param>
    /// <param name="isAlongX">True if wall runs along X axis, false if along Y axis</param>
    /// <returns>Wall specification</returns>
    public static WallSpec CreateRectangular(
        string name,
        string storyName,
        string sectionName,
        double x0, double y0, double z0,
        double lengthM, double heightM,
        bool isAlongX = true)
    {
        double[] xCoords, yCoords, zCoords;

        if (isAlongX)
        {
            // Wall runs along X axis (vertical in X-Z plane)
            xCoords = new[] { x0, x0 + lengthM, x0 + lengthM, x0 };
            yCoords = new[] { y0, y0, y0, y0 };
            zCoords = new[] { z0, z0, z0 + heightM, z0 + heightM };
        }
        else
        {
            // Wall runs along Y axis (vertical in Y-Z plane)
            xCoords = new[] { x0, x0, x0, x0 };
            yCoords = new[] { y0, y0 + lengthM, y0 + lengthM, y0 };
            zCoords = new[] { z0, z0, z0 + heightM, z0 + heightM };
        }

        return new WallSpec
        {
            Name = name,
            StoryName = storyName,
            SectionName = sectionName,
            XCoords = xCoords,
            YCoords = yCoords,
            ZCoords = zCoords
        };
    }
}
